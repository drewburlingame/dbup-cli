namespace DbUp.Cli.Configuration;

public class UnsupportedProviderException(string provider) : DbUpCliException($"Unsupported provider: {provider}")
{
    public string Provider { get; } = provider;
}