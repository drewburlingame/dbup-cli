using JetBrains.Annotations;

namespace DbUp.Cli.Configuration;

[UsedImplicitly]
internal class ConfigFile
{
    public Migration DbUp { get; set; } = null!;
}