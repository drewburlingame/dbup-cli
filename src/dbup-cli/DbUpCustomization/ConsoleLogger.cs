using System;
using DbUp.Engine.Output;

namespace DbUp.Cli
{
    public class ConsoleLogger: IUpgradeLog
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

        private static void WriteWithColor(string level, ConsoleColor color, string log)
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

        private static void Write(string level, string log) => Console.WriteLine($"[{level}] {log}");
    }
}
