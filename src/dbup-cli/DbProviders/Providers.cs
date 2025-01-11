using DbUp.Builder;
using DbUp.Cli.Configuration;
using DbUp.Engine.Output;

namespace DbUp.Cli.DbProviders;

public static class Providers
{
    private static readonly Dictionary<string, DbProvider> ProvidersMap = new List<DbProvider>
        {
            new SqlServerDbProvider(),
            new AzureSqlServerDbProvider(),
            new PostgresDbProvider(),
            new MySqlDbProvider()
        }
        .ToDictionary(p => p.Provider, StringComparer.OrdinalIgnoreCase);

    internal static bool IsSupportedProvider(string provider) => ProvidersMap.ContainsKey(provider);

    internal static DbProvider ProviderFor(string provider) =>
        ProvidersMap.GetValueOrDefault(provider) ?? throw new UnsupportedProviderException(provider);

    public static UpgradeEngineBuilder CreateUpgradeEngineBuilder(Migration migration) =>
        CreateUpgradeEngineBuilder(migration.Provider, migration.ConnectionString, migration.ConnectionTimeoutSec);
    
    public static UpgradeEngineBuilder CreateUpgradeEngineBuilder(
        string provider, string connectionString, int connectionTimeoutSec) =>
        ProviderFor(provider)
            .CreateUpgradeEngineBuilder(new ConnectionInfo(connectionString, connectionTimeoutSec));

    public static void EnsureDb(IUpgradeLog logger, Migration migration) => 
        ProviderFor(migration.Provider)
            .EnsureDb(logger, new ConnectionInfo(migration.ConnectionString, migration.ConnectionTimeoutSec));

    public static void DropDb(IUpgradeLog logger, Migration migration) => 
        ProviderFor(migration.Provider)
            .DropDb(logger, new ConnectionInfo(migration.ConnectionString, migration.ConnectionTimeoutSec));
}