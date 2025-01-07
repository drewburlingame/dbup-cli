using CommandDotNet;
using JetBrains.Annotations;

namespace DbUp.Cli.Cmd;

[UsedImplicitly]
public class ConfigFileArgs : IArgumentModel
{
    [Operand(Description = "Path to a configuration file. " +
                           "The path can be absolute or relative against a current directory")]
    public string File { get; set; } = Constants.Default.ConfigFileName;
}