using DbUp.Cli.Tests.TestInfrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.CommandTests;

[TestClass]
public class VersionTests
{
    private readonly TestHost host = new();

    [TestMethod]
    public void ShouldReturnZero() => host.ToolEngine.Run("version").ShouldSucceed();
}