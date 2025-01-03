using DbUp.Builder;
using DbUp.Engine.Output;

namespace DbUp.Cli.DbProviders;

public abstract class DbProvider
{
    // todo: convert to string at the end of the refactor
    public abstract Provider Provider { get; }

    public abstract UpgradeEngineBuilder CreateUpgradeEngineBuilder(ConnectionInfo connectionInfo);

    public abstract void EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo);

    public virtual void DropDb(IUpgradeLog logger, ConnectionInfo connectionInfo) => 
        throw new ProviderUnsupportedActionException(Provider, "dropping a database");

    public virtual UpgradeEngineBuilder SelectJournal(UpgradeEngineBuilder builder, Journal journal) => 
        throw new ProviderUnsupportedActionException(Provider, "journaling to a custom table");
}