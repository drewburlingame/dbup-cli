using DbUp.Builder;
using DbUp.Cli.CommandLineOptions;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Helpers;
using Optional;

namespace DbUp.Cli;

public static class UpgradeEngineBuilderExtensions
{
    public static Option<UpgradeEngineBuilder, Error> SelectJournal(this Option<UpgradeEngineBuilder, Error> builderOrNone, Provider provider, Journal journal) =>
        builderOrNone.Match(
            some: builder => journal == null
                ? builder.JournalTo(new NullJournal()).Some<UpgradeEngineBuilder, Error>()
                : journal.IsDefault
                    ? builder.Some<UpgradeEngineBuilder, Error>()
                    : Providers.ProviderFor(provider).Match(
                        some: p => p.SelectJournal(builder, journal),
                        none: Option.None<UpgradeEngineBuilder, Error>),
            none: Option.None<UpgradeEngineBuilder, Error>);
    
    internal static Option<UpgradeEngineBuilder, Error> SelectTransaction(this Option<UpgradeEngineBuilder, Error> builderOrNone, Transaction tran) =>
        builderOrNone.Match(
            some: builder =>
                tran == Transaction.None
                    ? builder.WithoutTransaction().Some<UpgradeEngineBuilder, Error>()
                    : tran == Transaction.PerScript
                        ? builder.WithTransactionPerScript().Some<UpgradeEngineBuilder, Error>()
                        : tran == Transaction.Single
                            ? builder.WithTransaction().Some<UpgradeEngineBuilder, Error>()
                            : Option.None<UpgradeEngineBuilder, Error>(Error.Create(Constants.ConsoleMessages.InvalidTransaction, tran)),
            none: Option.None<UpgradeEngineBuilder, Error>);

    internal static Option<UpgradeEngineBuilder, Error> SelectLogOptions(this Option<UpgradeEngineBuilder, Error> builderOrNone, IUpgradeLog logger, VerbosityLevel verbosity) =>
        builderOrNone
            .Match(
                some: builder => verbosity != VerbosityLevel.Min
                    ? builder.LogTo(logger).Some<UpgradeEngineBuilder, Error>()
                    : builder.LogToNowhere().Some<UpgradeEngineBuilder, Error>(),
                none: Option.None<UpgradeEngineBuilder, Error>)
            .Match(
                some: builder => verbosity == VerbosityLevel.Detail
                    ? builder.LogScriptOutput().Some<UpgradeEngineBuilder, Error>()
                    : builderOrNone,
                none: Option.None<UpgradeEngineBuilder, Error>);

    internal static Option<UpgradeEngineBuilder, Error> OverrideConnectionFactory(this Option<UpgradeEngineBuilder, Error> builderOrNone, Option<IConnectionFactory> connectionFactory) =>
        builderOrNone.Match(
            some: builder => connectionFactory.Match(
                some: factory =>
                {
                    builder.Configure(c => ((DatabaseConnectionManager)c.ConnectionManager).OverrideFactoryForTest(factory));
                    return builder.Some<UpgradeEngineBuilder, Error>();
                },
                none: () => builderOrNone),
            none: Option.None<UpgradeEngineBuilder, Error>);

    internal static Option<UpgradeEngineBuilder, Error> AddVariables(this Option<UpgradeEngineBuilder, Error> builderOrNone, Dictionary<string, string> vars, bool disableVars) =>
        builderOrNone.Match(
            some: builder => (disableVars ? builder.WithVariablesDisabled() : builder.WithVariables(vars)).Some<UpgradeEngineBuilder, Error>(),
            none: Option.None<UpgradeEngineBuilder, Error>
        );
}