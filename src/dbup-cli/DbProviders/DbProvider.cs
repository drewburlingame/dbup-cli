using DbUp.Builder;
using DbUp.Engine.Output;
using Optional;

namespace DbUp.Cli.DbProviders;

public abstract class DbProvider
{
    // todo: convert to string at the end of the refactor
    public abstract Provider Provider { get; }

    public abstract Option<UpgradeEngineBuilder, Error> SelectDbProvider(ConnectionInfo connectionInfo);

    public abstract Option<bool, Error> EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo);

    public virtual Option<bool, Error> DropDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        return Option.None<bool, Error>(Error.Create($"dbup provider for {Provider} does not support 'drop' command"));
    }

    public virtual Option<UpgradeEngineBuilder, Error> SelectJournal(UpgradeEngineBuilder builder, Journal journal)
    {
        return Option.None<UpgradeEngineBuilder, Error>(Error.Create($"JournalTo does not support provider for {Provider}"));
    }
}