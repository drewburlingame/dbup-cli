using DbUp.Builder;
using DbUp.Cli.Configuration;
using DbUp.Engine.Output;

namespace DbUp.Cli.DbProviders;

internal class SqlServerDbProvider : DbProvider
{
    public override string Provider => "SqlServer";
    
    public override UpgradeEngineBuilder CreateUpgradeEngineBuilder(ConnectionInfo connectionInfo) =>
        DeployChanges.To
            .SqlDatabase(connectionInfo.ConnectionString)
            .WithExecutionTimeout(connectionInfo.Timeout);

    public override void EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo) => 
        EnsureDatabase.For.SqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);

    public override void DropDb(IUpgradeLog logger, ConnectionInfo connectionInfo) => 
        DropDatabase.For.SqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);

    public override UpgradeEngineBuilder SelectJournal(UpgradeEngineBuilder builder, Journal journal) =>
        builder.JournalToSqlTable(journal.Schema, journal.Table);
}