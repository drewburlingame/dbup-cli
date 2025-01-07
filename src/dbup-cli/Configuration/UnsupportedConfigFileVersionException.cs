namespace DbUp.Cli.Configuration;

public class UnsupportedConfigFileVersionException(string version)
    : DbUpCliException($"Unsupported version of a config file: '{version}'. Expected `1`")
{
    public string Version { get; } = version;
}