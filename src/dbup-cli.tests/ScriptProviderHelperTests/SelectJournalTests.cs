using DbUp.Builder;
using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

[TestClass]
public class SelectJournalTests
{
    private readonly TestHost host = new();

    private string GetBasePath() => Path.Combine(ProjectPaths.ScriptsDir, "Default");

    [TestMethod]
    public void ScriptProviderHelper_SelectJournal_ShouldAddAllTheScripts()
    {
        var scripts = new List<ScriptBatch>
        {
            new (ScriptProviderHelper.GetFolder(GetBasePath(), "SubFolder1"), false, false, 0, Constants.Default.Encoding),
            new (ScriptProviderHelper.GetFolder(GetBasePath(), "SubFolder2"), false, false, 0, Constants.Default.Encoding)
        };

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, NamingOptions.Default);

        upgradeEngineBuilder.MatchSome(x =>
        {
            x.Build().PerformUpgrade();
        });

        var excutedScripts = host.Logger.GetExecutedScripts();

        excutedScripts.Should().HaveCount(3);
        excutedScripts[0].Should().Be("003.sql");
        excutedScripts[1].Should().Be("004.sql");
        excutedScripts[2].Should().Be("005.sql");
    }

    [TestMethod]
    public void ScriptProviderHelper_SelectJournal_ShouldReturnNone_IfTheListOfScriptsIsEmpty()
    {
        var scripts = new List<ScriptBatch>();

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, NamingOptions.Default);

        upgradeEngineBuilder.HasValue.Should().BeFalse();
        upgradeEngineBuilder.GetErrorOrThrow().Should().Be("At least one script should be present");
    }
}