using DbUp.Builder;
using DbUp.Engine.Output;
using Optional;

namespace DbUp.Cli.DbProviders;

public class SqlServerDbProvider : DbProvider
{
    public override Provider Provider => Provider.SqlServer;
    
    public override Option<UpgradeEngineBuilder, Error> SelectDbProvider(ConnectionInfo connectionInfo) =>
        DeployChanges.To
            .SqlDatabase(connectionInfo.ConnectionString)
            .WithExecutionTimeout(connectionInfo.Timeout)
            .Some<UpgradeEngineBuilder, Error>();

    public override Option<bool, Error> EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        EnsureDatabase.For.SqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        return true.Some<bool, Error>();
    }

    public override Option<bool, Error> DropDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        DropDatabase.For.SqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        return true.Some<bool, Error>();
    }

    public override Option<UpgradeEngineBuilder, Error> SelectJournal(UpgradeEngineBuilder builder, Journal journal) =>
        builder
            .JournalToSqlTable(journal.Schema, journal.Table)
            .Some<UpgradeEngineBuilder, Error>();
}