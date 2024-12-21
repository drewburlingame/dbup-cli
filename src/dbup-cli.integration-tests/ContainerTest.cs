using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using DbUp.Cli.Tests.TestInfrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.IntegrationTests;

public abstract class ContainerTest<TBuilderEntity, TContainerEntity, TConfigurationEntity>
    where TBuilderEntity : ContainerBuilder<TBuilderEntity, TContainerEntity, TConfigurationEntity>
    where TContainerEntity : IDatabaseContainer
    where TConfigurationEntity : IContainerConfiguration
{
    private readonly string DbScriptsDir;
    private readonly CaptureLogsLogger Logger;
    private readonly IEnvironment Env;
    private ToolEngine engine;

    private static IDatabaseContainer Container;
    private static string ServerConnString;
    private string DbConnString;
    private string DbName;

    public TestContext TestContext { get; set; }

    protected ContainerTest(string provider)
    {
        DbScriptsDir = Path.Combine(ProjectPaths.ScriptsDir, provider);
        Logger = new CaptureLogsLogger();
        Env = new CliEnvironment();
        engine = new ToolEngine(Env, Logger);
    }
    
    protected abstract TBuilderEntity NewBuilder { get; }

    protected abstract string ReplaceDbInConnString(string connectionString, string dbName);
    protected abstract IDbConnection GetConnection(string connectionString);
    protected abstract void AssertDbDoesNotExist(string connectionString, string dbName);

    protected abstract string QueryCountOfScript001 { get; }
    protected abstract string QueryCountOfScript001FromCustomJournal { get; }
    
    private string GetConfigPath(string name = "dbup.yml", string subPath = "EmptyScript") =>
        new DirectoryInfo(Path.Combine(DbScriptsDir, subPath, name)).FullName;
    
    [TestInitialize]
    public async Task TestInitialize()
    {
        if (Container is null)
        {
            Container = NewBuilder
                .WithCleanUp(true) // in case test run crashes or stopped while debugging
                .WithAutoRemove(true) // for faster removal in prep for next test
                .Build();

            await Container.StartAsync();
            ServerConnString = Container.GetConnectionString();
        }

        DbName = $"DbUp_{TestContext.TestName}";
        DbConnString = ReplaceDbInConnString(ServerConnString, DbName);
        Environment.SetEnvironmentVariable("CONNSTR", DbConnString);
    }

    [ClassCleanup]
    public static async Task ClassCleanup() => await (Container?.StopAsync() ?? Task.CompletedTask);

    [TestMethod]
    public void DatabaseShouldNotExistBeforeTestRun() => AssertDbDoesNotExist();

    [TestMethod]
    public void Ensure_CreateANewDb()
    {
        var result = engine.Run("upgrade", "--ensure", GetConfigPath());
        result.Should().Be(0);
        ConfirmUpgradeViaJournal(QueryCountOfScript001);
    }

    [TestMethod]
    public void UpgradeCommand_ShouldUseASpecifiedJournal()
    {
        var result = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "JournalTableScript"));
        result.Should().Be(0);
        ConfirmUpgradeViaJournal(QueryCountOfScript001FromCustomJournal);
    }

    [TestMethod]
    public void UpgradeCommand_ShouldUseConnectionTimeoutForLongRunningQueries()
    {
        var r = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "Timeout"));
        r.Should().Be(1);
    }
    
    [TestMethod]
    public virtual void Drop_DropADb()
    {
        engine.Run("upgrade", "--ensure", GetConfigPath());
        var result = engine.Run("drop", GetConfigPath());
        result.Should().Be(0);
        AssertDbDoesNotExist();
    }

    private void AssertDbDoesNotExist() => AssertDbDoesNotExist(DbConnString, DbName);

    private void ConfirmUpgradeViaJournal(string query)
    {
        using var connection = GetConnection(DbConnString);
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = query;
        var count = command.ExecuteScalar();

        count.Should().Be(1);
    }
}