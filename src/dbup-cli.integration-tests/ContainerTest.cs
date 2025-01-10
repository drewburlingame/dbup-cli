using System.Data;
using System.Diagnostics.CodeAnalysis;
using CommandDotNet;
using CommandDotNet.TestTools;
using DbUp.Cli.Tests;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FluentAssertions;

namespace DbUp.Cli.IntegrationTests;

public abstract class ContainerTest<TBuilderEntity, TContainerEntity, TConfigurationEntity> : IAsyncLifetime
    where TBuilderEntity : ContainerBuilder<TBuilderEntity, TContainerEntity, TConfigurationEntity>
    where TContainerEntity : IDatabaseContainer
    where TConfigurationEntity : IContainerConfiguration
{
    private readonly string dbScriptsDir;
    private readonly AppRunner appRunner = Program.NewAppRunner(new CliEnvironment(), 
        console => new CaptureLogsLogger(console));

    [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Expected to not be shared across concrete types")] 
    // ReSharper disable once InconsistentNaming
    private static IDatabaseContainer Container;
    [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Expected to not be shared across concrete types")]
    private static readonly SemaphoreSlim Semaphore = new(1);

    private IDatabaseContainer container;
    private string serverConnString;
    private string dbConnString;
    private string dbName;

    protected ContainerTest(string provider, ITestOutputHelper output)
    {
        dbScriptsDir = Path.Combine(Caller.Directory(), "Scripts", provider);
        Ambient.Output = output;
    }

    protected abstract TBuilderEntity NewBuilder { get; }
    protected virtual int DbNameLengthLimit => 60;

    protected abstract string ReplaceDbInConnString(string connectionString, string dbName);
    protected abstract IDbConnection GetConnection(string connectionString);
    protected abstract void AssertDbDoesNotExist(string connectionString, string dbName);

    protected abstract string QueryCountOfScript001 { get; }
    protected abstract string QueryCountOfScript001FromCustomJournal { get; }
    
    private string GetConfigPath(string subPath = "EmptyScript") =>
        new DirectoryInfo(Path.Combine(dbScriptsDir, subPath, "dbup.yml")).FullName;

    private AppRunnerResult Run(string args) => appRunner.RunInMem(args, Ambient.WriteLine);

    public async ValueTask InitializeAsync()
    {
        dbName = $"DbUp_{TestContext.Current.TestMethod!.MethodName}";
        dbName.Length.Should().BeLessOrEqualTo(DbNameLengthLimit);
        
        container = await GetContainer(() => NewBuilder);
        serverConnString = container.GetConnectionString();
        dbConnString = ReplaceDbInConnString(serverConnString, dbName);
        Environment.SetEnvironmentVariable("CONNSTR", dbConnString);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public void DatabaseShouldNotExistBeforeTestRun() => AssertDbDoesNotExist();

    [Fact]
    public async Task Ensure_CreateANewDb()
    {
        var result = Run($"upgrade --ensure {GetConfigPath()}").ShouldSucceed();
        await Verify(result.Console.AllText());
        ConfirmUpgradeViaJournal(QueryCountOfScript001);
    }

    [Fact]
    public async Task UpgradeCommand_ShouldUseASpecifiedJournal()
    {
        var result = Run($"upgrade --ensure {GetConfigPath("JournalTableScript")}").ShouldSucceed();
        await Verify(result.Console.AllText());
        ConfirmUpgradeViaJournal(QueryCountOfScript001FromCustomJournal);
    }

    [Fact]
    public virtual async Task UpgradeCommand_ShouldFailOnCommandTimeout()
    {
        var result = Run($"upgrade --ensure {GetConfigPath("Timeout")}").ShouldFail();
        var output = result.Console.AllText();
        await Verify(output)
            // the exception message is inconsistent between mac and github's ubuntu-latest
            .ScrubLinesWithReplace(line => line.Replace("Unknown error 258", "Unknown error: 258"));
        var failureExplanation = output!.Split(Environment.NewLine).FirstOrDefault(l => l.StartsWith("Failed to perform upgrade:"));
        failureExplanation.Should().NotBeNull().And.Contain("Timeout");
        GetCountOfScript(QueryCountOfScript001).Should().Be(0);
    }

    [Fact]
    public virtual async Task Status_Issue8_Fails_RunAlways_scripts_return_error()
    {
        var configPath = GetConfigPath("Status");
        appRunner.RunInMem($"upgrade --ensure {configPath}");
        
        // It shouldn't actually fail if the script has already been applied.
        var result = Run($"status {configPath}").ShouldFail(-1);
        var output = result.Console.AllText();
        await Verify(output);
        GetCountOfScript(QueryCountOfScript001).Should().Be(0);
    }
    
    [Fact]
    public virtual async Task Drop_DropADb()
    {
        appRunner.RunInMem($"upgrade --ensure {GetConfigPath()}");
        var result = Run($"drop {GetConfigPath()}").ShouldSucceed();
        await Verify(result.Console.AllText());
        AssertDbDoesNotExist();
    }

    private void AssertDbDoesNotExist() => AssertDbDoesNotExist(dbConnString, dbName);

    private void ConfirmUpgradeViaJournal(string query) => 
        GetCountOfScript(query).Should().Be(1);

    private object GetCountOfScript(string query)
    {
        using var connection = GetConnection(dbConnString);
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = query;
        var count = command.ExecuteScalar();
        return count;
    }

    private static async Task<IDatabaseContainer> GetContainer(Func<TBuilderEntity> builder)
    {
        if (Container is not null)
        {
            return Container;
        }

        await Semaphore.WaitAsync();
        try
        {
            if (Container is null)
            {
                Container = builder()
                    .WithCleanUp(true) // in case test run crashes or stopped while debugging
                    .WithAutoRemove(true) // for faster removal in prep for next test
                    .Build();

                await Container.StartAsync();
            }
            
            return Container;
        }
        finally
        {
            Semaphore.Release();
        }
    }
}