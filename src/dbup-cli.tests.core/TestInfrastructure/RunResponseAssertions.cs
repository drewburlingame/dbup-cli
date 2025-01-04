using CommandDotNet.TestTools;
using FluentAssertions;

namespace DbUp.Cli.Tests.TestInfrastructure;

public static class RunResponseAssertions
{
    public static AppRunnerResult ShouldSucceed(this AppRunnerResult result, int exitCode = 0)
    {
        result.EscapedException.Should().BeNull();
        if (result.ExitCode != 0)
        {
            Assert.Fail($"ExitCode={result.ExitCode}");
        }
        return result;
    }
    
    // TODO: assert should not be nullable. force all usages to describe error so there are no false positives
    public static AppRunnerResult ShouldFail(this AppRunnerResult result, int exitCode = 1)
    {
        result.ShouldExitWithCode(exitCode);
        return result;
    }
    
    public static AppRunnerResult ShouldExitWithCode(this AppRunnerResult result, int exitCode)
    {
        result.ExitCode.Should().Be(exitCode);
        return result;
    }
}