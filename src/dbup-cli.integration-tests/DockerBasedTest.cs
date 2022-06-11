﻿using Docker.DotNet;
using Docker.DotNet.Models;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace DbUp.Cli.IntegrationTests
{
    public class DockerBasedTest
    {
        private const string DockerEngineUri = "npipe://./pipe/docker_engine";
        private const string HostIp = "127.0.0.1";

        DockerClient DockerClient;
        string ContainerId;

        protected Task DockerInitialize(string imageName, List<string> environmentVariables, string port, Func<DbConnection> createConnection)
        {
            return DockerInitialize(imageName, environmentVariables, new List<string>(), port, createConnection);
        }

        protected async Task DockerInitialize(string imageName, List<string> environmentVariables, List<string> cmd, string port, Func<DbConnection> createConnection)
        {
            DockerClient = new DockerClientConfiguration(new Uri(DockerEngineUri)).CreateClient();
            var pars = new CreateContainerParameters(new Config()
            {
                Image = imageName,
                ExposedPorts = new Dictionary<string, EmptyStruct>()
                {
                    { port, new EmptyStruct() }
                },
                Env = environmentVariables,
                NetworkDisabled = false, 
                Cmd = cmd
            });

            pars.HostConfig = new HostConfig()
            {
                AutoRemove = true,
                PortBindings = new Dictionary<string, IList<PortBinding>>()
                {
                    { port, new List<PortBinding> { new PortBinding() { HostPort = port, HostIP = HostIp } } }
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

                var cont = await DockerClient.Containers.CreateContainerAsync(pars);
                ContainerId = cont.ID;
                var res = await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters());
                res.Should().BeTrue();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            var started = DateTime.Now;
            var connected = false;
            while (DateTime.Now - started < TimeSpan.FromMinutes(1))
            {
                using (var connection = createConnection())
                {
                    try
                    {
                        connection.Open();
                        connected = true;
                        break;
                    }
                    catch
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                }
            }

            connected.Should().BeTrue("Server should be awailable to connect");
        }

        protected async Task DockerCleanup(Func<DbConnection> createConnection, Func<DbConnection, DbCommand> createCommand)
        {
            await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters() { WaitBeforeKillSeconds = 1 });

            var started = DateTime.Now;
            while (DateTime.Now - started < TimeSpan.FromMinutes(2))
            {
                using (var connection = createConnection())
                using (var command = createCommand(connection))
                {
                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteScalarAsync();
                        await Task.Delay(1000);
                    }
                    catch
                    {
                        await Task.Delay(1000);
                        return;
                    }
                }
            }
        }
    }
}
