using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests;

public class MarkAsExecutedTests
{
    private readonly TestHost host = new();

    [Fact]
    public async Task WhenCalled_ShouldNotMakeAnyChangesInDb()
    {
        host.ToolEngine
            .Run("mark-as-executed", ProjectPaths.GetConfigPath("mark-as-executed.yml"))
            .ShouldSucceed();

        host.Logger.SummaryText().Should().NotContain("print 'You should not see this message'");

        // 02/01/2025 07:27:48
        await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Info));
    }
}