using System.Text.RegularExpressions;

namespace DbUp.Cli.Tests;

public static partial class CaptureLogsLoggerExtensions
{
    public static List<string> GetExecutedScripts(this CaptureLogsLogger logger)
    {
        var exp = ExecutedScriptsRegex();
        return logger.InfoMessages
            .Select(m => exp.Match(m))
            .Where(m => m.Success)
            .Select(m => m.Groups[1].Value)
            .ToList();
    }

    [GeneratedRegex(@"Executing Database Server script '(.+\.sql)'")]
    private static partial Regex ExecutedScriptsRegex();
}