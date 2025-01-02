using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests;

public class StatusTests
{
    private readonly TestHost host = new();

    [Fact]
    public async Task ShouldPrintGeneralInformation_IfNoScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("noscripts.yml"))
            .ShouldSucceed();

        var logs = await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Info));
        logs.Text.Should().EndWith("[I] Database is up-to-date. Upgrade is not required.");
    }

    [Fact]
    public async Task ShouldPrintGeneralInformation_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"))
            .ShouldExitWithCode(-1);

        var logs = await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Info));
        logs.Text.Should().EndWith("[I] You have 1 more scripts to execute.");
    }

    [Fact]
    public async Task ShouldPrintScriptName_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);

        var logs = await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Info));
        logs.Text.Should().EndWith("c001.sql");
    }

    [Fact]
    public async Task ShouldReturnMinusOne_IfThereAreTheScriptsToExecute()
    {
        host.ToolEngine
            .Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);
        
        var logs = await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Info));
        logs.Text.Should().EndWith("c001.sql");
    }

    [Fact]
    public async Task ShouldUseSpecifiedEnvFiles()
    {
        host.ToolEngine.Run("status", 
                ProjectPaths.GetConfigPath("Status", "status.yml"), 
                "-n",
                "--env",
                ProjectPaths.GetConfigPath("Status", "file1.env"),
                ProjectPaths.GetConfigPath("Status", "file2.env"))
            .ShouldExitWithCode(-1);
        
        var logs = await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Info));
        logs.Text.Should().EndWith("c001.sql");
    }
}