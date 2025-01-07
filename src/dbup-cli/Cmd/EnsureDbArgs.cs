using CommandDotNet;
using JetBrains.Annotations;

namespace DbUp.Cli.Cmd;

[UsedImplicitly]
public class EnsureDbArgs : IArgumentModel
{
    [Option("ensure", Description = "Create a database if not exists")]
    public bool Ensure { get; set; }
}