using DbUp.Builder;
using DbUp.Cli.CommandLineOptions;
using DbUp.Cli.DbUpCustomization;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Helpers;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using DbUp.Cli.DbProviders;

namespace DbUp.Cli
{
    public static class ConfigurationHelper
    {
        private static Option<DbProvider, Error> ProviderFor(Provider provider) =>
            provider switch
            {
                Provider.SqlServer => new SqlServerDbProvider().Some<DbProvider, Error>(),
                Provider.AzureSql => new AzureSqlServerDbProvider().Some<DbProvider, Error>(),
                Provider.PostgreSQL => new PostgresDbProvider().Some<DbProvider, Error>(),
                Provider.MySQL => new MySqlDbProvider().Some<DbProvider, Error>(),
                _ => Option.None<DbProvider, Error>(Error.Create(Constants.ConsoleMessages.UnsupportedProvider, provider.ToString()))
            };

        public static Option<UpgradeEngineBuilder, Error> SelectDbProvider(Provider provider, string connectionString, int connectionTimeoutSec) =>
            ProviderFor(provider).Match(
                    some: p => p.SelectDbProvider(new ConnectionInfo(connectionString, connectionTimeoutSec)),
                    none: Option.None<UpgradeEngineBuilder,  Error>);

        public static Option<bool, Error> EnsureDb(IUpgradeLog logger, Provider provider, string connectionString, int connectionTimeoutSec)
        {
            try
            {
                return ProviderFor(provider).Match(
                    some: p => p.EnsureDb(logger, new ConnectionInfo(connectionString, connectionTimeoutSec)),
                    none: Option.None<bool, Error>);
            }
            catch (Exception ex)
            {
                return Option.None<bool, Error>(Error.Create("EnsureDb failed: {0}", ex.Message));
            }
        }

        public static Option<bool, Error> DropDb(IUpgradeLog logger, Provider provider, string connectionString, int connectionTimeoutSec)
        {
            try
            {
                return ProviderFor(provider).Match(
                    some: p => p.DropDb(logger, new ConnectionInfo(connectionString, connectionTimeoutSec)),
                    none: Option.None<bool, Error>);
            }
            catch (Exception ex)
            {
                return Option.None<bool, Error>(Error.Create("DropDb failed: {0}", ex.Message));
            }
        }

        public static Option<UpgradeEngineBuilder, Error> SelectJournal(this Option<UpgradeEngineBuilder, Error> builderOrNone, Provider provider, Journal journal) =>
            builderOrNone.Match(
                some: builder => journal == null
                    ? builder.JournalTo(new NullJournal()).Some<UpgradeEngineBuilder, Error>()
                    : journal.IsDefault
                        ? builder.Some<UpgradeEngineBuilder, Error>()
                        : ProviderFor(provider).Match(
                            some: p => p.SelectJournal(builder, journal),
                            none: Option.None<UpgradeEngineBuilder, Error>),
                none: Option.None<UpgradeEngineBuilder, Error>);

        public static Option<UpgradeEngineBuilder, Error> SelectTransaction(this Option<UpgradeEngineBuilder, Error> builderOrNone, Transaction tran) =>
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

        public static Option<UpgradeEngineBuilder, Error> SelectLogOptions(this Option<UpgradeEngineBuilder, Error> builderOrNone, IUpgradeLog logger, VerbosityLevel verbosity) =>
            builderOrNone
                .Match(
                    some: builder => verbosity != VerbosityLevel.Min
                            ? builder.LogTo(logger).Some<UpgradeEngineBuilder, Error>()
                            : builder.LogToNowhere().Some<UpgradeEngineBuilder, Error>(),
                    none: error => Option.None<UpgradeEngineBuilder, Error>(error))
                .Match(
                    some: builder => verbosity == VerbosityLevel.Detail
                            ? builder.LogScriptOutput().Some<UpgradeEngineBuilder, Error>()
                            : builderOrNone,
                    none: Option.None<UpgradeEngineBuilder, Error>);

        public static Option<UpgradeEngineBuilder, Error> OverrideConnectionFactory(this Option<UpgradeEngineBuilder, Error> builderOrNone, Option<IConnectionFactory> connectionFactory) =>
            builderOrNone.Match(
                some: builder => connectionFactory.Match(
                    some: factory =>
                    {
                        builder.Configure(c => ((DatabaseConnectionManager)c.ConnectionManager).OverrideFactoryForTest(factory));
                        return builder.Some<UpgradeEngineBuilder, Error>();
                    },
                    none: () => builderOrNone),
                none: Option.None<UpgradeEngineBuilder, Error>);

        public static Option<UpgradeEngineBuilder, Error> AddVariables(this Option<UpgradeEngineBuilder, Error> builderOrNone, Dictionary<string, string> vars, bool disableVars) =>
            builderOrNone.Match(
                some: builder => (disableVars ? builder.WithVariablesDisabled() : builder.WithVariables(vars)).Some<UpgradeEngineBuilder, Error>(),
                none: Option.None<UpgradeEngineBuilder, Error>
            );

        public static Option<bool, Error> LoadEnvironmentVariables(IEnvironment environment, string configFilePath, IEnumerable<string> envFiles)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));
            if (configFilePath == null)
                throw new ArgumentNullException(nameof(configFilePath));

            // .env file  in a current folder
            var defaultEnvFile = Path.Combine(environment.GetCurrentDirectory(), Constants.Default.DotEnvFileName);
            if (environment.FileExists(defaultEnvFile))
            {
                DotNetEnv.Env.Load(defaultEnvFile);
            }
            // .env.local file  in a current folder
            var defaultEnvLocalFile = Path.Combine(environment.GetCurrentDirectory(), Constants.Default.DotEnvLocalFileName);
            if (environment.FileExists(defaultEnvLocalFile))
            {
                DotNetEnv.Env.Load(defaultEnvLocalFile);
            }

            // .env file next to a dbup.yml
            var configFileEnv = Path.Combine(new FileInfo(configFilePath).DirectoryName, Constants.Default.DotEnvFileName);
            if (environment.FileExists(configFileEnv))
            {
                DotNetEnv.Env.Load(configFileEnv);
            }
            // .env.local file next to a dbup.yml
            var configFileEnvLocal = Path.Combine(new FileInfo(configFilePath).DirectoryName, Constants.Default.DotEnvLocalFileName);
            if (environment.FileExists(configFileEnvLocal))
            {
                DotNetEnv.Env.Load(configFileEnvLocal);
            }

            if (envFiles != null)
            {
                foreach (var file in envFiles)
                {
                    Error error = null;
                    ConfigLoader.GetFilePath(environment, file)
                        .Match(
                            some: path => DotNetEnv.Env.Load(path),
                            none: err => error = err);

                    if (error != null)
                    {
                        return Option.None<bool, Error>(error);
                    }
                }
            }

            return true.Some<bool, Error>();
        }
    }
}
