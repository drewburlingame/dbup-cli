using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests;

public class UpgradeTests
{
    public UpgradeTests(ITestOutputHelper output) => Ambient.Output = output;

    private readonly TestHost host = new();
    
    [Fact]
    public void ShouldRespectScriptEncoding()
    {
        host.Run("upgrade", ProjectPaths.GetConfigPath("encoding.yml"))
            .ShouldSucceed();

        host.Logger.SummaryText(CaptureLogsLogger.Level.Operation).Should().Contain("print 'Превед, медвед'");
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
    public async Task ShouldRespectScriptFiltersAndMatchFiles(string filename)
    {
        var result = host.Run("upgrade", ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();
        
        var output = await Verify(result.Console.AllText());
        output.Text.Should().Contain(filename);
    }

    [InlineData("d001.sql")]
    [InlineData("d01.sql")]
    [InlineData("d0b1.sql")]
    [InlineData("c01.sql")]
    [InlineData("c0aa1.sql")]
    [InlineData("e01.sql")]
    [InlineData("e0aa1.sql")]
    [Theory]
    public async Task ShouldRespectScriptFiltersAndNotMatchFiles(string filename)
    {
        var result = host.Run("upgrade", ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();

        var output = await Verify(result.Console.AllText());
        output.Text.Should().NotContain(filename);
    }
    
    [InlineData("detail")]
    [InlineData("min")]
    [InlineData("normal")]
    [Theory]
    public async Task Should_respect_verbosity(string verbosity)
    {
        var result = host.Run("upgrade", "-v", verbosity, ProjectPaths.GetConfigPath("filter.yml"))
            .ShouldSucceed();

        await Verify(result.Console.AllText());
    }
}