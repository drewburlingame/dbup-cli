namespace DbUp.Cli.Cmd;

public class UpgradeEngineArgs : MigrationArgs
{
    public VerbosityArgs VerbosityArgs { get; set; }
    internal VerbosityLevel Verbosity => VerbosityArgs.Verbosity;
}