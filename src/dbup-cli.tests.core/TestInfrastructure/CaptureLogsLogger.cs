using System;
using System.Collections.Generic;
using System.Text;
using DbUp.Engine.Output;

namespace DbUp.Cli.Tests.TestInfrastructure;

public class CaptureLogsLogger: IUpgradeLog
{
    private readonly StringBuilder logBuilder = new();
    private readonly ConsoleLogger inner = new();
        
    public List<string> InfoMessages { get; } = new();

    public string Log => logBuilder.ToString();

    public void LogTrace(string format, params object[] args)
    {
        inner.LogTrace(format, args);
        logBuilder.AppendLine("[T] " + string.Format(format, args));
    }

    public void LogDebug(string format, params object[] args)
    {
        inner.LogDebug(format, args);
        logBuilder.AppendLine("[D] " + string.Format(format, args));
    }

    public void LogInformation(string format, params object[] args)
    {
        inner.LogInformation(format, args);
        var formattedMsg = string.Format(format, args);
        InfoMessages.Add(formattedMsg);
        logBuilder.AppendLine("[I] " + formattedMsg);
    }

    public void LogWarning(string format, params object[] args)
    {
        inner.LogWarning(format, args);
        logBuilder.AppendLine("[W] " + string.Format(format, args));
    }

    public void LogError(string format, params object[] args)
    {
        inner.LogError(format, args);
        logBuilder.AppendLine("[E] " + string.Format(format, args));
    }

    public void LogError(Exception ex, string format, params object[] args)
    {
        inner.LogError(format, args);
        logBuilder.AppendLine("[E] " + $"{string.Format(format, args)}{Environment.NewLine}{ex}");
    }

    public void LogDbOperation(string operation)
    {
        var value = "[O] " + operation;
        Console.WriteLine(value);
        logBuilder.AppendLine(value);
    }
}