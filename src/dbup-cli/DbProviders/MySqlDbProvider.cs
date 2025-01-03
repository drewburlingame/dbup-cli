using DbUp.Builder;
using DbUp.Engine.Output;
using DbUp.MySql;

namespace DbUp.Cli.DbProviders;

public class MySqlDbProvider : DbProvider
{
    public override Provider Provider => Provider.MySQL;
    
    public override UpgradeEngineBuilder CreateUpgradeEngineBuilder(ConnectionInfo connectionInfo) =>
        DeployChanges.To
            .MySqlDatabase(connectionInfo.ConnectionString)
            .WithExecutionTimeout(connectionInfo.Timeout);

    public override void EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo) => 
        EnsureDatabase.For.MySqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);

    public override UpgradeEngineBuilder SelectJournal(UpgradeEngineBuilder builder, Journal journal)
    {
        builder.Configure(c => c.Journal = 
            new MySqlTableJournal(() => c.ConnectionManager, () => c.Log, journal.Schema, journal.Table));
        return builder;
    }
}