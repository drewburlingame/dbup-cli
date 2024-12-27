using System;
using System.Collections.Generic;
using DbUp.Builder;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.SqlServer;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace DbUp.Cli.Tests
{
    [TestClass]
    public class ConfigurationHelperTests
    {
        readonly List<SqlScript> scripts;
        readonly CaptureLogsLogger logger;
        readonly DelegateConnectionFactory testConnectionFactory;
        readonly RecordingDbConnection recordingConnection;
        readonly UpgradeEngineBuilder upgradeEngineBuilder;

        public ConfigurationHelperTests()
        {
            scripts = new List<SqlScript>
            {
                new SqlScript("Script1.sql", "create table Foo (Id int identity)")
                //new SqlScript("Script2.sql", "alter table Foo add column Name varchar(255)"),
                //new SqlScript("Script3.sql", "insert into Foo (Name) values ('test')")
            };

            logger = new CaptureLogsLogger();
            recordingConnection = new RecordingDbConnection(logger, "SchemaVersions");
            testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);

            upgradeEngineBuilder = DeployChanges.To
                .SqlDatabase("testconn")
                .WithScripts(new TestScriptProvider(scripts))
                .OverrideConnectionFactory(testConnectionFactory)
                .LogTo(logger);
        }

        [TestMethod]
        public void SelectDbProvider_ShouldReturnNone_IfAProviderIsNotSupported()
        {
            var builder = ConfigurationHelper.SelectDbProvider(Provider.UnsupportedProvider, @"Data Source=(localdb)\dbup;Initial Catalog=dbup-tests;Integrated Security=True", 60);

            builder.HasValue.Should().BeFalse();
        }

        [TestMethod]
        public void SelectDbProvider_ShouldReturnReturnAValidProvider_ForSqlServer()
        {
            var builder = ConfigurationHelper.SelectDbProvider(Provider.SqlServer, @"Data Source=(localdb)\dbup;Initial Catalog=dbup-tests;Integrated Security=True", 60);
            builder.MatchSome(b => b.Configure(c => c.ConnectionManager.Should().BeOfType(typeof(SqlConnectionManager))));

            builder.HasValue.Should().BeTrue();
            builder.MatchSome(x =>
            {
                x.WithScripts(new TestScriptProvider(scripts));
                x.Build();
            });
        }

        [TestMethod]
        public void PerformUpgrade_ShouldUseCustomVersionsTable_IfCustomJournalIsPassed()
        {
            upgradeEngineBuilder.Some<UpgradeEngineBuilder, Error>()
                .SelectJournal(
                    Provider.SqlServer, new Journal("test_scheme", "test_SchemaVersion")
                );

            upgradeEngineBuilder.Build().PerformUpgrade();

            logger.InfoMessages.Should().Contain("Creating the [test_scheme].[test_SchemaVersion] table");
        }

        [TestMethod]
        public void PerformUpgrade_ShouldUseDefaultVersionsTable_IfDefaultJournalIsPassed()
        {
            upgradeEngineBuilder.Some<UpgradeEngineBuilder, Error>()
                .SelectJournal(Provider.SqlServer, Journal.Default);

            upgradeEngineBuilder.Build().PerformUpgrade();

            logger.InfoMessages.Should().Contain("Creating the [SchemaVersions] table");
        }

        [TestMethod]
        public void SelectJournal_ShouldSelectNullJournal_IfNoneValueIsPassed()
        {
            upgradeEngineBuilder.Some<UpgradeEngineBuilder, Error>()
                .SelectJournal(Provider.SqlServer, null);

            upgradeEngineBuilder.Build().PerformUpgrade();
            logger.InfoMessages.Should().NotContain(x => x.StartsWith("Creating the ", StringComparison.Ordinal));
        }
    }
}
