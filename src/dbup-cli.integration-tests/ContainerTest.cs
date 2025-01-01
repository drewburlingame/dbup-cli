using System.Data;
using DbUp.Cli.Tests.TestInfrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.IntegrationTests;

public abstract class ContainerTest<TBuilderEntity, TContainerEntity, TConfigurationEntity>(string provider)
    where TBuilderEntity : ContainerBuilder<TBuilderEntity, TContainerEntity, TConfigurationEntity>
    where TContainerEntity : IDatabaseContainer
    where TConfigurationEntity : IContainerConfiguration
{
    
    private readonly string dbScriptsDir = Path.Combine(ProjectPaths.ScriptsDir, provider);
    private readonly ToolEngine engine = new(new CliEnvironment(), new CaptureLogsLogger());

    private IDatabaseContainer container;
    private string serverConnString;
    private string dbConnString;
    private string dbName;

    public TestContext TestContext { get; set; }

    protected abstract TBuilderEntity NewBuilder { get; }
    protected virtual int DbNameLengthLimit => 60;

    protected abstract string ReplaceDbInConnString(string connectionString, string dbName);
    protected abstract IDbConnection GetConnection(string connectionString);
    protected abstract void AssertDbDoesNotExist(string connectionString, string dbName);

    protected abstract string QueryCountOfScript001 { get; }
    protected abstract string QueryCountOfScript001FromCustomJournal { get; }
    
    private string GetConfigPath(string name = "dbup.yml", string subPath = "EmptyScript") =>
        new DirectoryInfo(Path.Combine(dbScriptsDir, subPath, name)).FullName;
    
    [TestInitialize]
    public async Task TestInitialize()
    {
        if (container is null)
        {
            container = NewBuilder
                .WithCleanUp(true) // in case test run crashes or stopped while debugging
                .WithAutoRemove(true) // for faster removal in prep for next test
                .Build();

            await container.StartAsync();
            serverConnString = container.GetConnectionString();
        }

        dbName = $"DbUp_{TestContext.TestName}";
        dbName.Length.Should().BeLessOrEqualTo(DbNameLengthLimit);
        dbConnString = ReplaceDbInConnString(serverConnString, dbName);
        Environment.SetEnvironmentVariable("CONNSTR", dbConnString);
    }

    [TestCleanup]
    public async Task TestCleanup() => await (container?.StopAsync() ?? Task.CompletedTask);

    [TestMethod]
    public void DatabaseShouldNotExistBeforeTestRun() => AssertDbDoesNotExist();

    [TestMethod]
    public void Ensure_CreateANewDb()
    {
        var result = engine.Run("upgrade", "--ensure", GetConfigPath());
        result.ShouldSucceed();
        ConfirmUpgradeViaJournal(QueryCountOfScript001);
    }

    [TestMethod]
    public void UpgradeCommand_ShouldUseASpecifiedJournal()
    {
        var result = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "JournalTableScript"));
        result.ShouldSucceed();
        ConfirmUpgradeViaJournal(QueryCountOfScript001FromCustomJournal);
    }

    [TestMethod]
    public virtual void UpgradeCommand_ShouldFailOnCommandTimeout()
    {
        var r = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "Timeout"));
        r.ShouldFail(assert: error => error.Should().StartWith("Failed to perform upgrade:").And.Contain("Timeout"));
        GetCountOfScript(QueryCountOfScript001).Should().Be(0);
    }
    
    [TestMethod]
    public virtual void Drop_DropADb()
    {
        engine.Run("upgrade", "--ensure", GetConfigPath());
        var result = engine.Run("drop", GetConfigPath());
        result.ShouldSucceed();
        AssertDbDoesNotExist();
    }

    private void AssertDbDoesNotExist() => AssertDbDoesNotExist(dbConnString, dbName);

    private void ConfirmUpgradeViaJournal(string query)
    {
        GetCountOfScript(query).Should().Be(1);
    }

    private object GetCountOfScript(string query)
    {
        using var connection = GetConnection(dbConnString);
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = query;
        var count = command.ExecuteScalar();
        return count;
    }
}