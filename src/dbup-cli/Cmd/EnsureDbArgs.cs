using CommandDotNet;

namespace DbUp.Cli.Cmd;

public class EnsureDbArgs : IArgumentModel
{
    [Option("ensure", Description = "Create a database if not exists")]
    public bool Ensure { get; set; }
}