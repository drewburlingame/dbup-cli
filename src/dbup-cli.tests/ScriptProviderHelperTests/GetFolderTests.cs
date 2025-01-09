using FluentAssertions;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

public class GetFolderTests
{
    [Fact]
    public void ShouldReturnCurrentFolder_IfTheFolderIsNullOrWhiteSpace()
    {
        var current = Directory.GetCurrentDirectory();
        var path = ScriptProviderHelper.GetFolder(current, null);
        path.Should().Be(current);
    }

    [Fact]
    public void ShouldThrowAnException_IfTheBaseFolderIsNullOrWhiteSpace()
    {
        Action nullAction = () => ScriptProviderHelper.GetFolder(null, null);
        Action whitespaceAction = () => ScriptProviderHelper.GetFolder("   ", null);

        nullAction.Should().Throw<ArgumentException>();
        whitespaceAction.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ShouldReturnFullyQualifiedFolder_IfTheFolderIsARelativePath()
    {
        var current = Directory.GetCurrentDirectory();
        var path = ScriptProviderHelper.GetFolder(current, "upgrades");
        path.Should().Be( Path.Combine(current, "upgrades"));
    }

    [Fact]
    public void ShouldReturnOriginalFolder_IfTheFolderIsAFullyQualifiedPath()
    {
        var current = Directory.GetCurrentDirectory();
        var upgradesRootedPath = ProjectPaths.GetTempPath("upgrades");
        var path = ScriptProviderHelper.GetFolder(current, upgradesRootedPath);
        path.Should().Be(upgradesRootedPath);
    }
}