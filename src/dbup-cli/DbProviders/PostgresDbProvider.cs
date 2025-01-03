using DbUp.Builder;
using DbUp.Engine.Output;

namespace DbUp.Cli.DbProviders;

public class PostgresDbProvider : DbProvider
{
    public override Provider Provider => Provider.PostgreSQL;
    
    public override UpgradeEngineBuilder CreateUpgradeEngineBuilder(ConnectionInfo connectionInfo) =>
        DeployChanges.To
            .PostgresqlDatabase(connectionInfo.ConnectionString)
            .WithExecutionTimeout(connectionInfo.Timeout);

    public override void EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo) =>
        // Postgres provider does not support timeout...
        EnsureDatabase.For.PostgresqlDatabase(connectionInfo.ConnectionString, logger);

    public override UpgradeEngineBuilder SelectJournal(UpgradeEngineBuilder builder, Journal journal) =>
        builder.JournalToPostgresqlTable(journal.Schema, journal.Table);
}