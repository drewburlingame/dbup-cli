using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.CommandTests;

[TestClass]
public class StatusTests
{
    private readonly TestHost host = new();

    [TestMethod]
    public void ShouldPrintGeneralInformation_IfNoScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("noscripts.yml"))
            .ShouldSucceed();

        host.Logger.InfoMessages.Last().Should().StartWith("Database is up-to-date");
    }

    [TestMethod]
    public void ShouldPrintGeneralInformation_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"))
            .ShouldExitWithCode(-1);

        host.Logger.InfoMessages.Last().Should().StartWith("You have 1 more scripts");
    }

    [TestMethod]
    public void ShouldPrintScriptName_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);

        host.Logger.InfoMessages.Last().Should().EndWith("c001.sql");
    }

    [TestMethod]
    public void ShouldReturnMinusOne_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);
        
        host.Logger.InfoMessages.Last().Should().EndWith("c001.sql");
    }

    [TestMethod]
    public void ShouldUseSpecifiedEnvFiles()
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
}