using DbUp.Builder;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine;
using FluentAssertions;
using Optional;

namespace DbUp.Cli.Tests;

public class UpgradeEngineBuilderExtensionsTests
{
    private readonly UpgradeEngineBuilder upgradeEngineBuilder;
    private readonly TestHost host = new();

    public UpgradeEngineBuilderExtensionsTests()
    {
        List<SqlScript> scripts1 = [
            new("Script1.sql", "create table Foo (Id int identity)")
        ];

        upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .WithScripts(new TestScriptProvider(scripts1))
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger);
    }

    [Fact]
    public void PerformUpgrade_ShouldUseCustomVersionsTable_IfCustomJournalIsPassed()
    {
        upgradeEngineBuilder.Some<UpgradeEngineBuilder, Error>()
            .SelectJournal(Provider.SqlServer, new Journal("test_scheme", "test_SchemaVersion"));

        upgradeEngineBuilder.Build().PerformUpgrade();
        host.Logger.InfoMessages.Should().Contain("[I] Creating the [test_scheme].[test_SchemaVersion] table");
    }

    [Fact]
    public void PerformUpgrade_ShouldUseDefaultVersionsTable_IfDefaultJournalIsPassed()
    {
        upgradeEngineBuilder.Some<UpgradeEngineBuilder, Error>()
            .SelectJournal(Provider.SqlServer, Journal.Default);

        upgradeEngineBuilder.Build().PerformUpgrade();
        host.Logger.InfoMessages.Should().Contain("[I] Creating the [SchemaVersions] table");
    }

    [Fact]
    public void SelectJournal_ShouldSelectNullJournal_IfNoneValueIsPassed()
    {
        upgradeEngineBuilder.Some<UpgradeEngineBuilder, Error>()
            .SelectJournal(Provider.SqlServer, null);

        upgradeEngineBuilder.Build().PerformUpgrade();
        host.Logger.InfoMessages.Should().NotContain(x => x.StartsWith("Creating the ", StringComparison.Ordinal));
    }
}