using DbUp.Builder;
using DbUp.Cli.Cmd;
using DbUp.Cli.Configuration;
using DbUp.Cli.DbProviders;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Helpers;

namespace DbUp.Cli;

public static class UpgradeEngineBuilderExtensions
{
    public static UpgradeEngineBuilder SelectJournal(this UpgradeEngineBuilder builder, string provider, Journal? journal) =>
        journal == null
            ? builder.JournalTo(new NullJournal())
            : journal.IsDefault
                ? builder
                : Providers.ProviderFor(provider).SelectJournal(builder, journal);

    internal static UpgradeEngineBuilder SelectTransaction(this UpgradeEngineBuilder builder, Transaction tran) =>
        tran == Transaction.None
            ? builder.WithoutTransaction()
            : tran == Transaction.PerScript
                ? builder.WithTransactionPerScript()
                : tran == Transaction.Single
                    ? builder.WithTransaction()
                    : throw new InvalidTransactionException(tran);

    internal static UpgradeEngineBuilder SelectLogOptions(this UpgradeEngineBuilder builder, IUpgradeLog logger, VerbosityLevel verbosity)
    {
        builder = verbosity != VerbosityLevel.min
            ? builder.LogTo(logger)
            : builder.LogToNowhere();
        if(verbosity == VerbosityLevel.detail)
            builder.LogScriptOutput();
        return builder;
    }

    internal static UpgradeEngineBuilder OverrideConnectionFactory(this UpgradeEngineBuilder builder, IConnectionFactory? factory)
    {
        if(factory is not null)
        {
            builder.Configure(c =>
            ((DatabaseConnectionManager) c.ConnectionManager).OverrideFactoryForTest(factory));
            
        }
        return builder;
    }

    internal static UpgradeEngineBuilder AddVariables(this UpgradeEngineBuilder builder,
        Dictionary<string, string>? vars, bool disableVars) =>
        disableVars
            ? builder.WithVariablesDisabled()
            : vars is null
                ? builder
                : builder.WithVariables(vars);
}