using DbUp.Builder;
using DbUp.Cli.DbProviders;
using DbUp.Engine.Output;
using Optional;

namespace DbUp.Cli;

public static class Providers
{
    internal static Option<DbProvider, Error> ProviderFor(Provider provider) =>
        provider switch
        {
            Provider.SqlServer => new SqlServerDbProvider().Some<DbProvider, Error>(),
            Provider.AzureSql => new AzureSqlServerDbProvider().Some<DbProvider, Error>(),
            Provider.PostgreSQL => new PostgresDbProvider().Some<DbProvider, Error>(),
            Provider.MySQL => new MySqlDbProvider().Some<DbProvider, Error>(),
            _ => Option.None<DbProvider, Error>(Error.Create(Constants.ConsoleMessages.UnsupportedProvider, provider.ToString()))
        };

    public static Option<UpgradeEngineBuilder, Error> CreateUpgradeEngineBuilder(Provider provider, string connectionString, int connectionTimeoutSec) =>
        ProviderFor(provider).Match(
            some: p => p.CreateUpgradeEngineBuilder(new ConnectionInfo(connectionString, connectionTimeoutSec)),
            none: Option.None<UpgradeEngineBuilder,  Error>);

    internal static Option<bool, Error> EnsureDb(IUpgradeLog logger, Provider provider, string connectionString, int connectionTimeoutSec)
    {
        try
        {
            return ProviderFor(provider).Match(
                some: p => p.EnsureDb(logger, new ConnectionInfo(connectionString, connectionTimeoutSec)),
                none: Option.None<bool, Error>);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Create("EnsureDb failed: {0}", ex.Message));
        }
    }

    internal static Option<bool, Error> DropDb(IUpgradeLog logger, Provider provider, string connectionString, int connectionTimeoutSec)
    {
        try
        {
            return ProviderFor(provider).Match(
                some: p => p.DropDb(logger, new ConnectionInfo(connectionString, connectionTimeoutSec)),
                none: Option.None<bool, Error>);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Create("DropDb failed: {0}", ex.Message));
        }
    }
}