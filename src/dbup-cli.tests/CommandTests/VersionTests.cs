using DbUp.Cli.Tests.TestInfrastructure;

namespace DbUp.Cli.Tests.CommandTests;

public class VersionTests
{
    private readonly TestHost host = new();

    [Fact]
    public void ShouldReturnZero() => host.ToolEngine.Run("version").ShouldSucceed();
}