using DbUp.Cli.Configuration;

namespace DbUp.Cli.DbProviders;

public class UnsupportedProviderException(Provider provider) : DbUpCliException($"Unsupported provider: {provider}")
{
    public Provider Provider { get; } = provider;
}