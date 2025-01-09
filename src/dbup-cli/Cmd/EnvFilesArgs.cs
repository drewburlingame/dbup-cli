using CommandDotNet;
using JetBrains.Annotations;

namespace DbUp.Cli.Cmd;

[UsedImplicitly]
public class EnvFilesArgs : IArgumentModel
{
    [Option('e', "env", Description =
        "Path to an environment file. Can be more than one file specified. " +
        "The path can be absolute or relative against a current directory")]
    public List<string> EnvFiles { get; set; } = [];
}