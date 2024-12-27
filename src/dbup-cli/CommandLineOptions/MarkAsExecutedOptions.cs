﻿using System.Collections.Generic;
using CommandLine;

namespace DbUp.Cli.CommandLineOptions
{
    [Verb("mark-as-executed", HelpText = "Mark all scripts as executed")]
    class MarkAsExecutedOptions: VerbosityBase
    {
        [Option("ensure", HelpText = "Create database if it is not exists")]
        public bool Ensure { get; set; }

        [Option('e', "env", Required = false, HelpText = "Path to an environment file. Can be more than one file specified. The path can be absolute or relative against a current directory")]
        public IEnumerable<string> EnvFiles { get; set; }
    }
}
