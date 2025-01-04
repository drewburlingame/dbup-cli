using System.Data;
using System.Text.RegularExpressions;
using CommandDotNet;
using CommandDotNet.TestTools;
using DbUp.Cli.Tests.TestInfrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNetEnv;
using FluentAssertions;

namespace DbUp.Cli.IntegrationTests;

public abstract class ContainerTest<TBuilderEntity, TContainerEntity, TConfigurationEntity>(string provider)
: IAsyncLifetime
    where TBuilderEntity : ContainerBuilder<TBuilderEntity, TContainerEntity, TConfigurationEntity>
    where TContainerEntity : IDatabaseContainer
    where TConfigurationEntity : IContainerConfiguration
{
    private readonly string dbScriptsDir = Path.Combine(ProjectPaths.ScriptsDir, provider);
    private readonly AppRunner appRunner = Program.NewAppRunner(new CliEnvironment(), 
        console => new CaptureLogsLogger(console));

    private static readonly Dictionary<Type, IDatabaseContainer> ContainerMap = new();
    private static readonly SemaphoreSlim Semaphore = new(1);

    private IDatabaseContainer container;
    private string serverConnString;
    private string dbConnString;
    private string dbName;

    protected abstract TBuilderEntity NewBuilder { get; }
    protected virtual int DbNameLengthLimit => 60;

    protected abstract string ReplaceDbInConnString(string connectionString, string dbName);
    protected abstract IDbConnection GetConnection(string connectionString);
    protected abstract void AssertDbDoesNotExist(string connectionString, string dbName);

    protected abstract string QueryCountOfScript001 { get; }
    protected abstract string QueryCountOfScript001FromCustomJournal { get; }
    
    private string GetConfigPath(string name = "dbup.yml", string subPath = "EmptyScript") =>
        new DirectoryInfo(Path.Combine(dbScriptsDir, subPath, name)).FullName;

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
        var result = appRunner.RunInMem($"upgrade --ensure {GetConfigPath()}");
        result.ShouldSucceed();
        await Verify(result.Console.AllText());
        ConfirmUpgradeViaJournal(QueryCountOfScript001);
    }

    [Fact]
    public async Task UpgradeCommand_ShouldUseASpecifiedJournal()
    {
        var result = appRunner.RunInMem($"upgrade --ensure {GetConfigPath("dbup.yml", "JournalTableScript")}");
        result.ShouldSucceed();
        await Verify(result.Console.AllText());
        ConfirmUpgradeViaJournal(QueryCountOfScript001FromCustomJournal);
    }

    [Fact]
    public virtual async Task UpgradeCommand_ShouldFailOnCommandTimeout()
    {
        var result = appRunner.RunInMem($"upgrade --ensure {GetConfigPath("dbup.yml", "Timeout")}");
        result.ShouldFail();
        var output = result.Console.AllText();
        await Verify(output);
        var failureExplanation = output!.Split(Environment.NewLine).FirstOrDefault(l => l.StartsWith("Failed to perform upgrade:"));
        failureExplanation.Should().NotBeNull().And.Contain("Timeout");
        GetCountOfScript(QueryCountOfScript001).Should().Be(0);
    }
    
    [Fact]
    public virtual async Task Drop_DropADb()
    {
        appRunner.RunInMem($"upgrade --ensure {GetConfigPath()}");
        var result = appRunner.RunInMem($"drop {GetConfigPath()}");
        result.ShouldSucceed();
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
        if (ContainerMap.TryGetValue(typeof(TContainerEntity), out var container))
        {
            return container;
        }

        await Semaphore.WaitAsync();
        try
        {
            if (ContainerMap.TryGetValue(typeof(TContainerEntity), out container))
            {
                return container;
            }
            else
            {
                container = builder()
                    .WithCleanUp(true) // in case test run crashes or stopped while debugging
                    .WithAutoRemove(true) // for faster removal in prep for next test
                    .Build();

                await container.StartAsync();
                ContainerMap.Add(typeof(TContainerEntity), container);
                return container;
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }
}