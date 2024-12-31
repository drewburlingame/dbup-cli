using DbUp.Builder;
using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

[TestClass]
public class NamingOptionsTests
{
    private readonly TestHost host = new();

    [TestMethod]
    public void ScriptNamingScheme_WithDefaultNamingSettings_ShouldUseDefaultNamingScheme()
    {
        var scripts = new List<ScriptBatch>
        {
            new ScriptBatch(ScriptProviderHelper.GetFolder(ProjectPaths.ConfigDir, "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = NamingOptions.Default;

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, namingOptions);

        upgradeEngineBuilder.MatchSome(x =>
        {
            x.Build().PerformUpgrade();
        });

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("SubFolder.001.sql");
    }

    [TestMethod]
    public void ScriptNamingScheme_With_UseOnlyFileName_Set_ShoudUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new ScriptBatch(ScriptProviderHelper.GetFolder(ProjectPaths.ConfigDir, "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(useOnlyFileName: true, false, null);

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, namingOptions);

        upgradeEngineBuilder.MatchSome(x =>
        {
            x.Build().PerformUpgrade();
        });

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("001.sql");
    }

    [TestMethod]
    public void ScriptNamingScheme_With_IncludeBaseFolderName_Set_ShoudUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new ScriptBatch(ScriptProviderHelper.GetFolder(ProjectPaths.ConfigDir, "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(false, includeBaseFolderName: true, null);

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, namingOptions);

        upgradeEngineBuilder.MatchSome(x =>
        {
            x.Build().PerformUpgrade();
        });

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("Naming.SubFolder.001.sql");
    }

    [TestMethod]
    public void ScriptNamingScheme_With_IncludeBaseFolderName_And_UseOnlyFileName_Set_ShoudUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new ScriptBatch(ScriptProviderHelper.GetFolder(ProjectPaths.ConfigDir, "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(useOnlyFileName: true, includeBaseFolderName: true, null);

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, namingOptions);

        upgradeEngineBuilder.MatchSome(x =>
        {
            x.Build().PerformUpgrade();
        });

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("Naming.001.sql");
    }

    [TestMethod]
    public void ScriptNamingScheme_With_Prefix_Set_ShoudUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new ScriptBatch(ScriptProviderHelper.GetFolder(ProjectPaths.ConfigDir, "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(false, false, "prefix_");

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, namingOptions);

        upgradeEngineBuilder.MatchSome(x =>
        {
            x.Build().PerformUpgrade();
        });

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("prefix_SubFolder.001.sql");
    }

    [TestMethod]
    public void ScriptNamingScheme_With_Prefix_Set_ShoudTrimPrefixAndUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new ScriptBatch(ScriptProviderHelper.GetFolder(ProjectPaths.ConfigDir, "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(false, false, " prefix_ ");

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, namingOptions);

        upgradeEngineBuilder.MatchSome(x =>
        {
            x.Build().PerformUpgrade();
        });

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("prefix_SubFolder.001.sql");
    }

    [TestMethod]
    public void ScriptNamingScheme_With_IncludeBaseFolderName_And_Prefix_Set_ShoudUsePrefixBeforeFolderName()
    {
        var scripts = new List<ScriptBatch>
        {
            new ScriptBatch(ScriptProviderHelper.GetFolder(ProjectPaths.ConfigDir, "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(false, includeBaseFolderName: true, "prefix_");

        var upgradeEngineBuilder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger).Some<UpgradeEngineBuilder, Error>()
            .SelectScripts(scripts, namingOptions);

        upgradeEngineBuilder.MatchSome(x =>
        {
            x.Build().PerformUpgrade();
        });

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("prefix_Naming.SubFolder.001.sql");
    }
}