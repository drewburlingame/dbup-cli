namespace DbUp.Cli.Configuration;

internal class ConfigFileNotFoundException(string path)
    : DbUpCliException($"Config file is not found: {path}")
{
    public string Path { get; } = path;
}