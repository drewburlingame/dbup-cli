using CommandLine;

namespace DbUp.Cli.CommandLineOptions;

[Verb("init", HelpText = "Create a new config file")]
internal class InitOptions: OptionsBase
{
}