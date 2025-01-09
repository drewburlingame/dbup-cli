using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests;

public class StatusTests
{
    public StatusTests(ITestOutputHelper output) => Ambient.Output = output;
    
    private readonly TestHost host = new();

    [Fact]
    public async Task ShouldPrintGeneralInformation_IfNoScriptsToExecute()
    {
        var result = host.Run("status", ProjectPaths.GetConfigPath("noscripts.yml"))
            .ShouldSucceed();

        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("[I] Database is up-to-date. Upgrade is not required.");
    }

    [Fact]
    public async Task ShouldPrintGeneralInformation_IfThereAreTheScriptsToExecute()
    {
        var result = host.Run("status", ProjectPaths.GetConfigPath("onescript.yml"))
            .ShouldExitWithCode(-1);

        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("[I] 1 scripts to execute.");
    }

    [Fact]
    public async Task ShouldPrintScriptName_IfThereAreTheScriptsToExecute()
    {
        var result = host.Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);

        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("c001.sql");
    }

    [Fact]
    public async Task ShouldReturnMinusOne_IfThereAreTheScriptsToExecute()
    {
        var result = host.Run("status", ProjectPaths.GetConfigPath("onescript.yml"), "-n")
            .ShouldExitWithCode(-1);
        
        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("c001.sql");
    }

    [Fact]
    public async Task ShouldUseSpecifiedEnvFiles()
    {
        var statusYml = ProjectPaths.GetConfigPath("Status", "status.yml");
        var file1Env = ProjectPaths.GetConfigPath("Status", "file1.env");
        var file2Env = ProjectPaths.GetConfigPath("Status", "file2.env");
        
        var result = host.Run($"status {statusYml} -n --env {file1Env} --env {file2Env}")
            .ShouldExitWithCode(-1);
        
        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("c001.sql");
    }
}