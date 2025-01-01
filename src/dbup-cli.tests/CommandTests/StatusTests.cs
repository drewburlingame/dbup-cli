using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests;

public class StatusTests
{
    private readonly TestHost host = new();

    [Fact]
    public void ShouldPrintGeneralInformation_IfNoScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("noscripts.yml"))
            .ShouldSucceed();

        host.Logger.InfoMessages.Last().Should().StartWith("Database is up-to-date");
    }

    [Fact]
    public void ShouldPrintGeneralInformation_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"))
            .ShouldExitWithCode(-1);

        host.Logger.InfoMessages.Last().Should().StartWith("You have 1 more scripts");
    }

    [Fact]
    public void ShouldPrintScriptName_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);

        host.Logger.InfoMessages.Last().Should().EndWith("c001.sql");
    }

    [Fact]
    public void ShouldReturnMinusOne_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);
        
        host.Logger.InfoMessages.Last().Should().EndWith("c001.sql");
    }

    [Fact]
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