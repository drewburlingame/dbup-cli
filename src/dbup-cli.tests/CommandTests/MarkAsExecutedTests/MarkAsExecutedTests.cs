using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests.MarkAsExecutedTests;

public class MarkAsExecutedTests
{
    public MarkAsExecutedTests(ITestOutputHelper output) => Ambient.Output = output;
    
    private readonly TestHost host = new(Caller.Directory());

    [Fact]
    public async Task WhenCalled_ShouldNotMakeAnyChangesInDb()
    {
        var result = host.Run("mark-as-executed").ShouldSucceed();
        host.Logger.SummaryText().Should().NotContain("print 'You should not see this message'");
        await Verify(result.Console.AllText());
    }
}