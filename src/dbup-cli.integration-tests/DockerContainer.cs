using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using FakeItEasy;
using FluentAssertions;

namespace DbUp.Cli.IntegrationTests;

public class DockerContainer : IAsyncDisposable
{
    internal const string HostIp = "127.0.0.1";
    
    DockerClient DockerClient;
    string ContainerId;

    public async Task Initialize(string imageName, 
        List<string> environmentVariables, 
        List<string> cmd, 
        string port, 
        Func<DbConnection> createConnection)
    {
        DockerClient = OperatingSystem.IsWindows()
            ? new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient()
            : new DockerClientConfiguration().CreateClient();
        var pars = new CreateContainerParameters(new Config
        {
            Image = imageName,
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                { port, new EmptyStruct() }
            },
            Env = environmentVariables,
            NetworkDisabled = false, 
            Cmd = cmd
        })
        {
            HostConfig = new HostConfig
            {
                AutoRemove = true,
                PortBindings = new Dictionary<string, IList<PortBinding>>()
                {
                    { port, new List<PortBinding> { new() { HostPort = port, HostIP = HostIp } } }
                }
            }
        };

        try
        {
            await DockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = imageName
                },
                null, A.Fake<IProgress<JSONMessage>>());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        var cont = await DockerClient.Containers.CreateContainerAsync(pars);
        ContainerId = cont.ID;
        var res = await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters());
        res.Should().BeTrue();

        var started = DateTime.Now;
        var connected = false;
        Exception lastError = null;
        while (DateTime.Now - started < TimeSpan.FromMinutes(1))
        {
            await using var connection = createConnection();
            try
            {
                await connection.OpenAsync();
                connected = true;
                break;
            }
            catch(Exception ex)
            {
                lastError = ex;
                await Task.Delay(1000);
            }
        }

        if (!connected && lastError is not null)
        {
            await Console.Out.WriteLineAsync($"Last connection error: {lastError}");
        }

        connected.Should().BeTrue("Server should be awailable to connect");
    }

    public async ValueTask DisposeAsync() => await Cleanup();

    public async Task Cleanup()
    {
        var stopped = await DockerClient.Containers.StopContainerAsync(ContainerId,
            new ContainerStopParameters {WaitBeforeKillSeconds = 1});
        if (!stopped)
        {
            throw new Exception($"Container {ContainerId} was not stopped");
        }
    }
}