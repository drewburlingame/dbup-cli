using CommandDotNet;
using JetBrains.Annotations;

namespace DbUp.Cli.Cmd;

[UsedImplicitly]
public class MigrationArgs : IArgumentModel
{
    [OrderByPositionInClass]
    public ConfigFileArgs ConfigFileArgs { get; set; } = null!;
    internal string File => ConfigFileArgs.File;
    
    public EnvFilesArgs EnvFilesArgs { get; set; } = null!;
    internal List<string> EnvFiles => EnvFilesArgs.EnvFiles;
}