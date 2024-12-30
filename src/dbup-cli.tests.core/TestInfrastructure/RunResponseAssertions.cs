using FluentAssertions;

namespace DbUp.Cli.Tests.TestInfrastructure;

public static class RunResponseAssertions
{
    public static void ShouldSucceed(this RunResult result, int exitCode = 0)
    {
        result.Error?.Should().BeNull();
        result.ShouldExitWithCode(exitCode);
    }

    // TODO: assert should not be nullable.  force all usages to describe error so there are no false positives
    public static void ShouldFail(this RunResult result, Action<string> assert, int exitCode = 1)
    {
        result.ShouldExitWithCode(exitCode);
        result.Error.Should().NotBeNull();
        assert ??= e => e.Should().Be("");
        assert.Invoke(result.Error);
    }
    
    public static void ShouldExitWithCode(this RunResult result, int exitCode) => 
        result.ExitCode.Should().Be(exitCode);
}