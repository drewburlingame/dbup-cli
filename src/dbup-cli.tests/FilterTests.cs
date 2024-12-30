using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests;

[TestClass]
public class FilterTests
{
    private readonly TestHost host = new();

    [TestMethod]
    public void CreateFilter_NullOrWhiteSpaceString_ShouldReturnNull()
    {
        var filter = ScriptProviderHelper.CreateFilter("  ");
        filter.Should().BeNull();

        filter = ScriptProviderHelper.CreateFilter(null);
        filter.Should().BeNull();
    }

    [TestMethod]
    public void CreateFilter_GeneralString_ShouldMatchTheSameStringInTheDifferentLetterCase()
    {
        var filter = ScriptProviderHelper.CreateFilter("script.sql");
        var result = filter("Script.SQL");
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CreateFilter_GeneralString_ShouldNotMatchTheSubstring()
    {
        var filter = ScriptProviderHelper.CreateFilter("script.sql");
        var result = filter("script");
        result.Should().BeFalse();
    }

    [DataRow("scr?ipt.sql", "scrAipt.SQL")]
    [DataRow("scr*ipt.sql", "script.SQL")]
    [DataRow("scr*ipt.sql", "scrAAAipt.SQL")]
    [DataTestMethod]
    public void CreateFilter_WildcardString_ShouldMatchTheTestedString(string filterString, string testedString)
    {
        var filter = ScriptProviderHelper.CreateFilter(filterString);
        var result = filter(testedString);
        result.Should().BeTrue();
    }

    [DataRow("scr?ipt.sql", "script.sql")]
    [DataRow("scr?ipt.sql", "scrAAipt.sql")]
    [DataRow("scr?ipt.sql", "1scrAipt.sql")]
    [DataTestMethod]
    public void CreateFilter_WildcardString_ShouldNotMatchTheTestedString(string filterString, string testedString)
    {
        var filter = ScriptProviderHelper.CreateFilter(filterString);
        var result = filter(testedString);
        result.Should().BeFalse();
    }

    [DataRow("/scr.ipt\\.sql/", "scrAipt.SQL")]
    [DataRow("/scr.?ipt\\.sql/", "script.SQL")]
    //[DataRow("//script\\.sql//", "/script.SQL/")] // TODO: test this later
    [DataRow("//", "it is equal to empty filter")]
    [DataTestMethod]
    public void CreateFilter_RegexString_ShouldMatchTheTestedString(string filterString, string testedString)
    {
        var filter = ScriptProviderHelper.CreateFilter(filterString);
        var result = filter(testedString);
        result.Should().BeTrue();
    }

    [DataRow("/scr.ipt\\.sql/", "script.SQL")]
    [DataRow("/scr.ipt\\.sql/", "scrAAipt.SQL")]
    [DataRow("/scr.?ipt\\.sql/", "scrAAipt.SQL")]
    [DataRow("/scr.ipt\\.sql/", "1scrAipt.SQL")]
    [DataRow("/scr.ipt\\.sql/", "scrAipt.SQL1")]
    [DataTestMethod]
    public void CreateFilter_RegexString_ShouldNotMatchTheTestedString(string filterString, string testedString)
    {
        var filter = ScriptProviderHelper.CreateFilter(filterString);
        var result = filter(testedString);
        result.Should().BeFalse();
    }

    [DataRow("d0a1.sql")]
    [DataRow("d0aa1.sql")]
    [DataRow("c001.sql")]
    [DataRow("c0a1.sql")]
    [DataRow("c0b1.sql")]
    [DataRow("e001.sql")]
    [DataRow("e0a1.sql")]
    [DataRow("e0b1.sql")]
    [DataTestMethod]
    public void ToolEngine_ShouldRespectScriptFiltersAndMatchFiles(string filename)
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().Contain(filename);
    }

    [DataRow("d001.sql")]
    [DataRow("d01.sql")]
    [DataRow("d0b1.sql")]
    [DataRow("c01.sql")]
    [DataRow("c0aa1.sql")]
    [DataRow("e01.sql")]
    [DataRow("e0aa1.sql")]
    [DataTestMethod]
    public void ToolEngine_ShouldRespectScriptFiltersAndNotMatchFiles(string filename)
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().NotContain(filename);
    }
}