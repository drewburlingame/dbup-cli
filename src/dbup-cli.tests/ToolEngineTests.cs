using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests;

[TestClass]
public class ToolEngineTests
{
    private readonly TestHost host = new();
    private readonly string tempDbUpYmlPath = ProjectPaths.GetTempPath("dbup.yml");

    [TestMethod]
    public void InitCommand_ShouldCreateDefaultConfig_IfItIsNotPresent()
    {
        host.EnsureDirectoryExists(ProjectPaths.TempDir);
        host.EnsureFileDoesNotExist(tempDbUpYmlPath);
        host.ToolEngine.Run("init").ShouldSucceed();
        host.Environment.FileExists(tempDbUpYmlPath).Should().BeTrue();
    }

    [TestMethod]
    public void InitCommand_ShouldReturn1AndNotCreateConfig_IfItIsPresent()
    {
        // ensure exists
        host.ToolEngine.Run("init");
        var lastWriteTime = new FileInfo(tempDbUpYmlPath).LastWriteTime;
        
        host.ToolEngine.Run("init").ShouldFail(assert: error => error.Should().StartWith("File already exists"));
        host.Environment.FileExists(tempDbUpYmlPath).Should().BeTrue();
        var newLastWriteTime = new FileInfo(tempDbUpYmlPath).LastWriteTime;
        newLastWriteTime.Should().Be(lastWriteTime);
    }

    [TestMethod]
    public void VersionCommand_ShouldReturnZero() => host.ToolEngine.Run("version").ShouldSucceed();

    [TestMethod]
    public void StatusCommand_ShouldPrintGeneralInformation_IfNoScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("noscripts.yml"))
            .ShouldSucceed();

        host.Logger.InfoMessages.Last().Should().StartWith("Database is up-to-date");
    }

    [TestMethod]
    public void StatusCommand_ShouldPrintGeneralInformation_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"))
            .ShouldExitWithCode(-1);

        host.Logger.InfoMessages.Last().Should().StartWith("You have 1 more scripts");
    }

    [TestMethod]
    public void StatusCommand_ShouldPrintScriptName_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);

        host.Logger.InfoMessages.Last().Should().EndWith("c001.sql");
    }

    [TestMethod]
    public void StatusCommand_ShouldReturnMinusOne_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);
        
        host.Logger.InfoMessages.Last().Should().EndWith("c001.sql");
    }

    [TestMethod]
    public void StatusCommand_ShouldUseSpecifiedEnvFiles()
    {
        host.ToolEngine.Run("status", 
                ProjectPaths.GetConfigPath("Status", "status.yml"), 
                "-n",
                "--env",
                ProjectPaths.GetConfigPath("Status", "file1.env"),
                ProjectPaths.GetConfigPath("Status", "file2.env"))
            .ShouldExitWithCode(-1);
        
        host.Logger.InfoMessages.Last().Should().EndWith("c001.sql");
    }

    [TestMethod]
    public void MarkAsExecutedCommand_WhenCalled_ShouldNotMakeAnyChangesInDb()
    {
        host.ToolEngine
            .Run("mark-as-executed", ProjectPaths.GetConfigPath("mark-as-executed.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().NotContain("print 'You should not see this message'");
    }

}