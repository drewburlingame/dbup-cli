using DbUp.Builder;
using DbUp.Engine.Output;
using DbUp.MySql;
using Optional;

namespace DbUp.Cli.DbProviders;

public class MySqlDbProvider : DbProvider
{
    public override Provider Provider => Provider.MySQL;
    
    public override Option<UpgradeEngineBuilder, Error> CreateUpgradeEngineBuilder(ConnectionInfo connectionInfo) =>
        DeployChanges.To
            .MySqlDatabase(connectionInfo.ConnectionString)
            .WithExecutionTimeout(connectionInfo.Timeout)
            .Some<UpgradeEngineBuilder, Error>();

    public override Option<bool, Error> EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        EnsureDatabase.For.MySqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        return true.Some<bool, Error>();
    }

    public override Option<UpgradeEngineBuilder, Error> SelectJournal(UpgradeEngineBuilder builder, Journal journal)
    {
        builder.Configure(c => c.Journal = new MySqlTableJournal(() => c.ConnectionManager, () => c.Log, journal.Schema, journal.Table));
        return builder.Some<UpgradeEngineBuilder, Error>();
    }
}