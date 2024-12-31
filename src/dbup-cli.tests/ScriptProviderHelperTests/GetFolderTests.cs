using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

[TestClass]
public class GetFolderTests
{
    [TestMethod]
    public void ShouldReturnCurrentFolder_IfTheFolderIsNullOrWhiteSpace()
    {
        var current = Directory.GetCurrentDirectory();
        var path = ScriptProviderHelper.GetFolder(current, null);
        path.Should().Be(current);
    }

    [TestMethod]
    public void ShouldThrowAnException_IfTheBaseFolderIsNullOrWhiteSpace()
    {
        Action nullAction = () => ScriptProviderHelper.GetFolder(null, null);
        Action whitespaceAction = () => ScriptProviderHelper.GetFolder("   ", null);

        nullAction.Should().Throw<ArgumentException>();
        whitespaceAction.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void ShouldReturnFullyQualifiedFolder_IfTheFolderIsARelativePath()
    {
        var current = Directory.GetCurrentDirectory();
        var path = ScriptProviderHelper.GetFolder(current, "upgrades");
        path.Should().Be( Path.Combine(current, "upgrades"));
    }

    [TestMethod]
    public void ShouldReturnOriginalFolder_IfTheFolderIsAFullyQualifiedPath()
    {
        var current = Directory.GetCurrentDirectory();
        var upgradesRootedPath = ProjectPaths.GetTempPath("upgrades");
        var path = ScriptProviderHelper.GetFolder(current, upgradesRootedPath);
        path.Should().Be(upgradesRootedPath);
    }
}