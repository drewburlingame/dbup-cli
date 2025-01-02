using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests;

public class UpgradeTests
{
    private readonly TestHost host = new();
    
    [Fact]
    public async Task ShouldRespectScriptEncoding()
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("encoding.yml"))
            .ShouldSucceed();

        var logs = await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Operation));
        logs.Text.Should().Contain("print 'Превед, медвед'");
    }
    
    [InlineData("d0a1.sql")]
    [InlineData("d0aa1.sql")]
    [InlineData("c001.sql")]
    [InlineData("c0a1.sql")]
    [InlineData("c0b1.sql")]
    [InlineData("e001.sql")]
    [InlineData("e0a1.sql")]
    [InlineData("e0b1.sql")]
    [Theory]
    public async Task ToolEngine_ShouldRespectScriptFiltersAndMatchFiles(string filename)
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();
        
        var logs = await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Info));
        logs.Text.Should().Contain(filename);
    }

    [InlineData("d001.sql")]
    [InlineData("d01.sql")]
    [InlineData("d0b1.sql")]
    [InlineData("c01.sql")]
    [InlineData("c0aa1.sql")]
    [InlineData("e01.sql")]
    [InlineData("e0aa1.sql")]
    [Theory]
    public async Task ToolEngine_ShouldRespectScriptFiltersAndNotMatchFiles(string filename)
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();

        var logs = await Verify(host.Logger.SummaryText(CaptureLogsLogger.Level.Info));
        logs.Text.Should().NotContain(filename);
    }
}