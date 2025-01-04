using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DbUp.Cli.Tests;

public static class InitializeVerify
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // since TestContainers assigns new random ports each run, we need to scrub the port for consistent runs
        // pgsql & mysql
        VerifierSettings.ScrubLinesWithReplace(l => Regex.Replace(l, @"Port=\d+", "Port=port_num"));
        // mssql
        VerifierSettings.ScrubLinesWithReplace(l => Regex.Replace(l, @"(Data Source=127\.0\.0\.1,)\d+", "$1port_num"));
        
        VerifierSettings.ScrubInlineDateTimes("MM/dd/yyyy HH:mm:ss");
        VerifierSettings.ScrubInlineGuids();
        VerifyDiffPlex.Initialize();
    }
}