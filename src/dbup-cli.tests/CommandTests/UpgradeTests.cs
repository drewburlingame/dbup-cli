using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests;

public class UpgradeTests
{
    private readonly TestHost host = new();
    
    [Fact]
    public void ShouldRespectScriptEncoding()
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("encoding.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().Contain("print 'Превед, медвед'");
    }
    
    [InlineData("d0a1.sql")]
    [InlineData("d0aa1.sql")]
    [InlineData("c001.sql")]
    [InlineData("c0a1.sql")]
    [InlineData("c0b1.sql")]
    [InlineData("e001.sql")]
    [InlineData("e0a1.sql")]
    [InlineData("e0b1.sql")]
    [Theory]
    public void ToolEngine_ShouldRespectScriptFiltersAndMatchFiles(string filename)
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().Contain(filename);
    }

    [InlineData("d001.sql")]
    [InlineData("d01.sql")]
    [InlineData("d0b1.sql")]
    [InlineData("c01.sql")]
    [InlineData("c0aa1.sql")]
    [InlineData("e01.sql")]
    [InlineData("e0aa1.sql")]
    [Theory]
    public void ToolEngine_ShouldRespectScriptFiltersAndNotMatchFiles(string filename)
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().NotContain(filename);
    }
}