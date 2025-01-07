using YamlDotNet.Core;

namespace DbUp.Cli.Configuration;

internal class ConfigParsingException(string path, YamlException innerException)
    : DbUpCliException("Configuration file error", innerException)
{
    public string Path { get; } = path;
}