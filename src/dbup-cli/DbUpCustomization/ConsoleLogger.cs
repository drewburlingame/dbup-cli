using CommandDotNet;
using DbUp.Engine.Output;

namespace DbUp.Cli.DbUpCustomization;

public class ConsoleLogger(IConsole console) : IUpgradeLog
{
    public void LogTrace(string format, params object[] args) => Write("T", string.Format(format, args));

    public void LogDebug(string format, params object[] args) => Write("D", string.Format(format, args));

    public void LogInformation(string format, params object[] args) => Write("I", string.Format(format, args));

    public void LogWarning(string format, params object[] args) => 
        WriteWithColor("W", ConsoleColor.Yellow, string.Format(format, args));

    public void LogError(string format, params object[] args) => 
        WriteWithColor("E", ConsoleColor.Red, string.Format(format, args));

    public void LogError(Exception ex, string format, params object[] args) => 
        WriteWithColor("E", ConsoleColor.Red, $"{string.Format(format, args)}{Environment.NewLine}{ex}");

    private void WriteWithColor(string level, ConsoleColor color, string log)
    {
        Console.ForegroundColor = color;
        try
        {
            Write(level, log);
        }
        finally
        {
            Console.ResetColor();
        }
    }

    private void Write(string level, string log) => console?.Out.WriteLine($"[{level}] {log}");
}