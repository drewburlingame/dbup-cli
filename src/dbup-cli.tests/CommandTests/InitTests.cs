using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.CommandTests;

[TestClass]
public class InitTests
{
    private readonly TestHost host = new();
    private readonly string tempDbUpYmlPath = ProjectPaths.GetTempPath("dbup.yml");

    [TestMethod]
    public void ShouldCreateDefaultConfig_IfItIsNotPresent()
    {
        host.EnsureDirectoryExists(ProjectPaths.TempDir);
        host.EnsureFileDoesNotExist(tempDbUpYmlPath);
        host.ToolEngine.Run("init").ShouldSucceed();
        host.Environment.FileExists(tempDbUpYmlPath).Should().BeTrue();
    }

    [TestMethod]
    public void ShouldReturn1AndNotCreateConfig_IfItIsPresent()
    {
        // ensure exists
        host.ToolEngine.Run("init");
        var lastWriteTime = new FileInfo(tempDbUpYmlPath).LastWriteTime;
        
        host.ToolEngine.Run("init").ShouldFail(assert: error => error.Should().StartWith("File already exists"));
        host.Environment.FileExists(tempDbUpYmlPath).Should().BeTrue();
        var newLastWriteTime = new FileInfo(tempDbUpYmlPath).LastWriteTime;
        newLastWriteTime.Should().Be(lastWriteTime);
    }
}