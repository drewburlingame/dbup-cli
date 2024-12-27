using DbUp.Builder;
using DbUp.Engine.Output;
using Optional;

namespace DbUp.Cli.DbProviders;

public class PostgresDbProvider : DbProvider
{
    public override Provider Provider => Provider.PostgreSQL;
    
    public override Option<UpgradeEngineBuilder, Error> SelectDbProvider(ConnectionInfo connectionInfo) =>
        DeployChanges.To
            .PostgresqlDatabase(connectionInfo.ConnectionString)
            .WithExecutionTimeout(connectionInfo.Timeout)
            .Some<UpgradeEngineBuilder, Error>();

    public override Option<bool, Error> EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        // Postgres provider does not support timeout...
        EnsureDatabase.For.PostgresqlDatabase(connectionInfo.ConnectionString, logger);
        return true.Some<bool, Error>();
    }

    public override Option<UpgradeEngineBuilder, Error> SelectJournal(UpgradeEngineBuilder builder, Journal journal) =>
        builder
            .JournalToPostgresqlTable(journal.Schema, journal.Table)
            .Some<UpgradeEngineBuilder, Error>();
}