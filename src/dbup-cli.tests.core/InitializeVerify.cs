using System.Runtime.CompilerServices;

namespace DbUp.Cli.Tests;

public static class InitializeVerify
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.ScrubInlineDateTimes("MM/dd/yyyy HH:mm:ss");
        VerifyDiffPlex.Initialize();
    }
}