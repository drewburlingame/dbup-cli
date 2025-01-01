using DbUp.Support;
using FluentAssertions;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

public class GetSqlScriptOptionsTests
{
    [Fact]
    public void ShouldSetScriptTypeToRunOnce_IfRunAlwaysIsSetToFalse()
    {
        var batch = new ScriptBatch("", runAlways: false, false, 1, "");
        var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

        options.ScriptType.Should().Be(ScriptType.RunOnce);
    }

    [Fact]
    public void ShouldSetScriptTypeToRunAlways_IfRunAlwaysIsSetToTrue()
    {
        var batch = new ScriptBatch("", runAlways: true, false, 1, "");
        var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

        options.ScriptType.Should().Be(ScriptType.RunAlways);
    }

    [Fact]
    public void ShouldSetGroupOrderToValidValue()
    {
        var batch = new ScriptBatch("", runAlways: true, false, 5, "");
        var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

        options.RunGroupOrder.Should().Be(5);
    }
}