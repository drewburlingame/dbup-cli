using CommandDotNet.Builders;
using CommandDotNet.TestTools;

namespace DbUp.Cli.Tests;

public static class CommandDotNetTestConfigs
{
    public static readonly TestConfig Default = new()
    {
        AppInfoOverride = new AppInfo(
            true, true, false, 
            typeof(Program).Assembly, ProjectPaths.RootDir,  
            "dbup", "1.0.0"),
        OnError =
        {
            CaptureAndReturnResult = true,
            Print = {ConsoleOutput = true}
        },
        OnSuccess =
        {
            Print = {ConsoleOutput = true}
        }
    };
}