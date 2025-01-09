using DbUp.Cli.Configuration;
using FluentAssertions;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

public class SelectJournalTests
{
    private readonly TestHost host = new();

    private string GetBasePath() => Path.Combine(ProjectPaths.ScriptsDir, "Default");

    [Fact]
    public void ScriptProviderHelper_SelectJournal_ShouldAddAllTheScripts()
    {
        var scripts = new List<ScriptBatch>
        {
            new (ScriptProviderHelper.GetFolder(GetBasePath(), "SubFolder1"), false, false, 0, Constants.Default.Encoding),
            new (ScriptProviderHelper.GetFolder(GetBasePath(), "SubFolder2"), false, false, 0, Constants.Default.Encoding)
        };

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger)
            .SelectScripts(scripts, NamingOptions.Default);

        builder.Build().PerformUpgrade();

        var excutedScripts = host.Logger.GetExecutedScripts();

        excutedScripts.Should().HaveCount(3);
        excutedScripts[0].Should().Be("003.sql");
        excutedScripts[1].Should().Be("004.sql");
        excutedScripts[2].Should().Be("005.sql");
    }

    [Fact]
    public void ScriptProviderHelper_SelectJournal_ShouldReturnNone_IfTheListOfScriptsIsEmpty()
    {
        var scripts = new List<ScriptBatch>();

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger);
            
         var ex = Assert.Throws<MissingScriptException>(() => builder.SelectScripts(scripts, NamingOptions.Default));

        ex.Message.Should().Be("At least one script should be present");
    }
}