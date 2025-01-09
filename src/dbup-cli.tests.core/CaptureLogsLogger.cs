using CommandDotNet;
using DbUp.Cli.DbUpCustomization;
using DbUp.Engine.Output;

namespace DbUp.Cli.Tests;

public class CaptureLogsLogger(IConsole console = null): IUpgradeLog
{
    private readonly ConsoleLogger inner = new(console);

    public enum Level
    {
        Trace,
        Debug,
        Operation,
        Info,
        Warn,
        Error
    }

    public readonly List<(Level level, string message)> Logs = new();
    
    public List<string> InfoMessages => SummaryLines(Level.Info).ToList();
    
    public string SummaryText() => SummaryText(Level.Trace);
    
    public string SummaryText(Level levelOrAbove) => string.Join('\n', SummaryLines(levelOrAbove));

    private IEnumerable<string> SummaryLines(Level levelOrAbove) =>
        Logs
            .Where(l => l.level >= levelOrAbove)
            .Select(l => $"[{l.level.ToString()[0]}] {l.message}");

    public void LogTrace(string format, params object[] args)
    {
        inner.LogTrace(format, args);
        Capture(Level.Trace, format, args);
    }

    public void LogDebug(string format, params object[] args)
    {
        inner.LogDebug(format, args);
        Capture(Level.Debug, format, args);
    }

    public void LogInformation(string format, params object[] args)
    {
        inner.LogInformation(format, args);
        Capture(Level.Info, format, args);
    }

    public void LogWarning(string format, params object[] args)
    {
        inner.LogWarning(format, args);
        Capture(Level.Warn, format, args);
    }

    public void LogError(string format, params object[] args)
    {
        inner.LogError(format, args);
        Capture(Level.Error, format, args);
    }

    public void LogError(Exception ex, string format, params object[] args)
    {
        inner.LogError(format, args);
        Capture(Level.Error, $"{string.Format(format, args)}{Environment.NewLine}{ex}");
    }

    public void LogDbOperation(string operation) => Capture(Level.Operation, operation);

    public void ClearCaptures() => Logs.Clear();

    private void Capture(Level level, string format, object[] args) => 
        Capture(level, string.Format(format, args));
    
    private void Capture(Level level, string message) => 
        Logs.Add((level, message));
}