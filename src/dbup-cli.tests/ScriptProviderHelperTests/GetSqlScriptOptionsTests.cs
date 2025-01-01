using DbUp.Support;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

[TestClass]
public class GetSqlScriptOptionsTests
{
    [TestMethod]
    public void ShouldSetScriptTypeToRunOnce_IfRunAlwaysIsSetToFalse()
    {
        var batch = new ScriptBatch("", runAlways: false, false, 1, "");
        var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

        options.ScriptType.Should().Be(ScriptType.RunOnce);
    }

    [TestMethod]
    public void ShouldSetScriptTypeToRunAlways_IfRunAlwaysIsSetToTrue()
    {
        var batch = new ScriptBatch("", runAlways: true, false, 1, "");
        var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

        options.ScriptType.Should().Be(ScriptType.RunAlways);
    }

    [TestMethod]
    public void ShouldSetGroupOrderToValidValue()
    {
        var batch = new ScriptBatch("", runAlways: true, false, 5, "");
        var options = ScriptProviderHelper.GetSqlScriptOptions(batch);

        options.RunGroupOrder.Should().Be(5);
    }
}