using CommandDotNet;

namespace DbUp.Cli.Cmd;

public class VerbosityArgs : IArgumentModel
{
    [Option('v', Description = "Verbosity level. Can be one of: detail, normal or min")]
    public VerbosityLevel Verbosity { get; set; } = VerbosityLevel.normal;
}