using DbUp.Cli.Configuration;

namespace DbUp.Cli.DbProviders;

internal class ProviderUnsupportedActionException(Provider provider, string action)
    : DbUpCliException($"{provider} does not support {action}")
{
    public Provider Provider { get; } = provider;
}