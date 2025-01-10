using DbUp.Cli.Configuration;
using FluentAssertions;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

public class NamingOptionsTests
{
    private readonly TestHost host = new(Caller.Directory());

    [Fact]
    public void ScriptNamingScheme_WithDefaultNamingSettings_ShouldUseDefaultNamingScheme()
    {
        var scripts = new List<ScriptBatch>
        {
            new (ScriptProviderHelper.GetFolder(Caller.Directory(), "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = NamingOptions.Default;

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger)
            .SelectScripts(scripts, namingOptions);

        builder.Build().PerformUpgrade();

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("SubFolder.001.sql");
    }

    [Fact]
    public void ScriptNamingScheme_With_UseOnlyFileName_Set_ShouldUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new (ScriptProviderHelper.GetFolder(Caller.Directory(), "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(useOnlyFileName: true, false, null);

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger)
            .SelectScripts(scripts, namingOptions);

        builder.Build().PerformUpgrade();

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("001.sql");
    }

    [Fact]
    public void ScriptNamingScheme_With_IncludeBaseFolderName_Set_ShouldUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new (ScriptProviderHelper.GetFolder(Caller.Directory(), "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(false, includeBaseFolderName: true, null);

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger)
            .SelectScripts(scripts, namingOptions);

        builder.Build().PerformUpgrade();

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("Naming.SubFolder.001.sql");
    }

    [Fact]
    public void ScriptNamingScheme_With_IncludeBaseFolderName_And_UseOnlyFileName_Set_ShouldUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new (ScriptProviderHelper.GetFolder(Caller.Directory(), "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(useOnlyFileName: true, includeBaseFolderName: true, null);

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger)
            .SelectScripts(scripts, namingOptions);

        builder.Build().PerformUpgrade();

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("Naming.001.sql");
    }

    [Fact]
    public void ScriptNamingScheme_With_Prefix_Set_ShouldUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new (ScriptProviderHelper.GetFolder(Caller.Directory(), "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(false, false, "prefix_");

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger)
            .SelectScripts(scripts, namingOptions);

        builder.Build().PerformUpgrade();

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("prefix_SubFolder.001.sql");
    }

    [Fact]
    public void ScriptNamingScheme_With_Prefix_Set_ShouldTrimPrefixAndUseValidScriptName()
    {
        var scripts = new List<ScriptBatch>
        {
            new (ScriptProviderHelper.GetFolder(Caller.Directory(), "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(false, false, " prefix_ ");

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger)
            .SelectScripts(scripts, namingOptions);

        builder.Build().PerformUpgrade();

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("prefix_SubFolder.001.sql");
    }

    [Fact]
    public void ScriptNamingScheme_With_IncludeBaseFolderName_And_Prefix_Set_ShouldUsePrefixBeforeFolderName()
    {
        var scripts = new List<ScriptBatch>
        {
            new(ScriptProviderHelper.GetFolder(Caller.Directory(), "Naming"), false, true, 0, Constants.Default.Encoding)
        };

        var namingOptions = new NamingOptions(false, includeBaseFolderName: true, "prefix_");

        var builder = DeployChanges.To
            .SqlDatabase("testconn")
            .OverrideConnectionFactory(host.TestConnectionFactory)
            .LogTo(host.Logger)
            .SelectScripts(scripts, namingOptions);

        builder.Build().PerformUpgrade();

        var executedScripts = host.Logger.GetExecutedScripts();

        executedScripts[0].Should().Be("prefix_Naming.SubFolder.001.sql");
    }
}