using FluentAssertions;

namespace DbUp.Cli.Tests;

public class FilterTests
{
    [Fact]
    public void CreateFilter_NullOrWhiteSpaceString_ShouldReturnNull()
    {
        var filter = ScriptProviderHelper.CreateFilter("  ");
        filter.Should().BeNull();

        filter = ScriptProviderHelper.CreateFilter(null);
        filter.Should().BeNull();
    }

    [Fact]
    public void CreateFilter_GeneralString_ShouldMatchTheSameStringInTheDifferentLetterCase()
    {
        var filter = ScriptProviderHelper.CreateFilter("script.sql");
        var result = filter("Script.SQL");
        result.Should().BeTrue();
    }

    [Fact]
    public void CreateFilter_GeneralString_ShouldNotMatchTheSubstring()
    {
        var filter = ScriptProviderHelper.CreateFilter("script.sql");
        var result = filter("script");
        result.Should().BeFalse();
    }

    [InlineData("scr?ipt.sql", "scrAipt.SQL")]
    [InlineData("scr*ipt.sql", "script.SQL")]
    [InlineData("scr*ipt.sql", "scrAAAipt.SQL")]
    [Theory]
    public void CreateFilter_WildcardString_ShouldMatchTheTestedString(string filterString, string testedString)
    {
        var filter = ScriptProviderHelper.CreateFilter(filterString);
        var result = filter(testedString);
        result.Should().BeTrue();
    }

    [InlineData("scr?ipt.sql", "script.sql")]
    [InlineData("scr?ipt.sql", "scrAAipt.sql")]
    [InlineData("scr?ipt.sql", "1scrAipt.sql")]
    [Theory]
    public void CreateFilter_WildcardString_ShouldNotMatchTheTestedString(string filterString, string testedString)
    {
        var filter = ScriptProviderHelper.CreateFilter(filterString);
        var result = filter(testedString);
        result.Should().BeFalse();
    }

    [InlineData("/scr.ipt\\.sql/", "scrAipt.SQL")]
    [InlineData("/scr.?ipt\\.sql/", "script.SQL")]
    //[InlineData("//script\\.sql//", "/script.SQL/")] // TODO: test this later
    [InlineData("//", "it is equal to empty filter")]
    [Theory]
    public void CreateFilter_RegexString_ShouldMatchTheTestedString(string filterString, string testedString)
    {
        var filter = ScriptProviderHelper.CreateFilter(filterString);
        var result = filter(testedString);
        result.Should().BeTrue();
    }

    [InlineData("/scr.ipt\\.sql/", "script.SQL")]
    [InlineData("/scr.ipt\\.sql/", "scrAAipt.SQL")]
    [InlineData("/scr.?ipt\\.sql/", "scrAAipt.SQL")]
    [InlineData("/scr.ipt\\.sql/", "1scrAipt.SQL")]
    [InlineData("/scr.ipt\\.sql/", "scrAipt.SQL1")]
    [Theory]
    public void CreateFilter_RegexString_ShouldNotMatchTheTestedString(string filterString, string testedString)
    {
        var filter = ScriptProviderHelper.CreateFilter(filterString);
        var result = filter(testedString);
        result.Should().BeFalse();
    }
}