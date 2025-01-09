using JetBrains.Annotations;

namespace DbUp.Cli.Cmd;

[UsedImplicitly]
public class UpgradeEngineArgs : MigrationArgs
{
    public VerbosityArgs VerbosityArgs { get; set; } = null!;
    internal VerbosityLevel Verbosity => VerbosityArgs.Verbosity;
}