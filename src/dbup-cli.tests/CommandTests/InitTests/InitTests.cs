using FluentAssertions;

namespace DbUp.Cli.Tests.CommandTests.InitTests;

public class InitTests
{
    public InitTests(ITestOutputHelper output) => Ambient.Output = output;
    
    private readonly TestHost host = new(Caller.Directory());
    private readonly string tempDbUpYmlPath = Caller.ConfigFile();
    
    [Fact]
    public void ShouldCreateDefaultConfig_IfItIsNotPresent()
    {
        host.EnsureDirectoryExists(Caller.Directory());
        host.EnsureFileDoesNotExist(tempDbUpYmlPath);
        host.Run("init").ShouldSucceed();
        host.Environment.FileExists(tempDbUpYmlPath).Should().BeTrue();
    }

    [Fact]
    public void ShouldReturn1AndNotCreateConfig_IfItIsPresent()
    {
        // ensure exists
        host.Run("init");
        var lastWriteTime = new FileInfo(tempDbUpYmlPath).LastWriteTime;

        var result = host.Run("init");
        result.ShouldFail();
        var output = result.Console.AllText();
        output!.Split(Environment.NewLine).Last().Should().StartWith("File already exists");
        host.Environment.FileExists(tempDbUpYmlPath).Should().BeTrue();
        var newLastWriteTime = new FileInfo(tempDbUpYmlPath).LastWriteTime;
        newLastWriteTime.Should().Be(lastWriteTime);
    }
}