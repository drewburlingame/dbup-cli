using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests.StatusTests;

public class StatusTests
{
    public StatusTests(ITestOutputHelper output) => Ambient.Output = output;
    
    private readonly TestHost host = new(Caller.Directory());

    [Fact]
    public async Task ShouldPrintGeneralInformation_IfNoScriptsToExecute()
    {
        var result = host.Run($"status noscripts.yml").ShouldSucceed();
        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("[I] Database is up-to-date. Upgrade is not required.");
    }

    [Fact]
    public async Task ShouldPrintGeneralInformation_IfThereAreTheScriptsToExecute()
    {
        var result = host.Run("status scripts.yml").ShouldExitWithCode(-1);
        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("[I] 1 scripts to execute.");
    }

    [Fact]
    public async Task ShouldPrintScriptName_IfThereAreTheScriptsToExecute()
    {
        var result = host.Run("status -n scripts.yml").ShouldExitWithCode(-1);
        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("c001.sql");
    }

    [Fact]
    public async Task ShouldReturnMinusOne_IfThereAreTheScriptsToExecute()
    {
        var result = host.Run("status -n scripts.yml").ShouldExitWithCode(-1);
        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("c001.sql");
    }

    [Fact]
    public async Task ShouldUseSpecifiedEnvFiles()
    {
        var result = host.Run("status -n dotenv.yml --env file1.env --env file2.env").ShouldExitWithCode(-1);
        var logs = await Verify(result.Console.AllText());
        logs.Text.Should().EndWith("c001.sql");
    }
}