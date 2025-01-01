using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests;

public class MarkAsExecutedTests
{
    private readonly TestHost host = new();

    [Fact]
    public void WhenCalled_ShouldNotMakeAnyChangesInDb()
    {
        host.ToolEngine
            .Run("mark-as-executed", ProjectPaths.GetConfigPath("mark-as-executed.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().NotContain("print 'You should not see this message'");
    }
}