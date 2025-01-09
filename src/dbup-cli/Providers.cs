using DbUp.Builder;
using DbUp.Cli.DbProviders;
using DbUp.Engine.Output;

namespace DbUp.Cli;

public static class Providers
{
    internal static DbProvider ProviderFor(Provider provider) =>
        provider switch
        {
            Provider.SqlServer => new SqlServerDbProvider(),
            Provider.AzureSql => new AzureSqlServerDbProvider(),
            Provider.PostgreSQL => new PostgresDbProvider(),
            Provider.MySQL => new MySqlDbProvider(),
            _ => throw new UnsupportedProviderException(provider)
        };

    public static UpgradeEngineBuilder CreateUpgradeEngineBuilder(
        Provider provider, string connectionString, int connectionTimeoutSec) =>
        ProviderFor(provider).CreateUpgradeEngineBuilder(new ConnectionInfo(connectionString, connectionTimeoutSec));

    public static void EnsureDb(IUpgradeLog logger, Migration migration) => 
        EnsureDb(logger, migration.Provider, migration.ConnectionString, migration.ConnectionTimeoutSec);
    
    internal static void EnsureDb(
        IUpgradeLog logger, Provider provider, string connectionString, int connectionTimeoutSec) => 
        ProviderFor(provider).EnsureDb(logger, new ConnectionInfo(connectionString, connectionTimeoutSec));

    public static void DropDb(IUpgradeLog logger, Migration migration) => 
        DropDb(logger, migration.Provider, migration.ConnectionString, migration.ConnectionTimeoutSec);

    internal static void DropDb(
        IUpgradeLog logger, Provider provider, string connectionString, int connectionTimeoutSec) => 
        ProviderFor(provider).DropDb(logger, new ConnectionInfo(connectionString, connectionTimeoutSec));
}