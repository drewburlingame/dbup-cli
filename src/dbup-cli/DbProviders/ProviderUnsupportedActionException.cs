namespace DbUp.Cli.DbProviders;

internal class ProviderUnsupportedActionException(string provider, string action)
    : DbUpCliException($"{provider} does not support {action}")
{
    public string Provider { get; } = provider;
}