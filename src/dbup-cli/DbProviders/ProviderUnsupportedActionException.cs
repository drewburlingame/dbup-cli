namespace DbUp.Cli.DbProviders;

public class ProviderUnsupportedActionException(Provider provider, string action)
    : DbUpCliException($"{provider} does not support {action}")
{
    public Provider Provider { get; } = provider;
}