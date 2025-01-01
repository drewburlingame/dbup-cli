using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.CommandTests;

[TestClass]
public class UpgradeTests
{
    private readonly TestHost host = new();
    
    [TestMethod]
    public void ShouldRespectScriptEncoding()
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("encoding.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().Contain("print 'Превед, медвед'");
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