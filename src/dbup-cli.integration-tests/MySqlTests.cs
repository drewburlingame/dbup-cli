using DbUp.Cli.Tests.TestInfrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using FluentAssertions;
using System.Threading.Tasks;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class MySqlTests : DockerBasedTest
    {
        readonly CaptureLogsLogger Logger;
        readonly IEnvironment Env;

        private static readonly string Pwd = "MyPwd2020";
        private static readonly string DbName = "DbUp";
        private static readonly string ServerConnString = $"Server=127.0.0.1;Uid=root;Pwd={Pwd};";
        private static readonly string DbConnString = $"Server=127.0.0.1;Database={DbName};Uid=root;Pwd={Pwd};";
        
        private static readonly string DbScriptsDir = Path.Combine(ProjectPaths.ScriptsDir, "MySql");

        public MySqlTests()
        {
            Env = new CliEnvironment();
            Logger = new CaptureLogsLogger();

            Environment.SetEnvironmentVariable("CONNSTR", DbConnString);
        }

        private static string GetConfigPath(string name = "dbup.yml", string subPath = "EmptyScript") =>
            new DirectoryInfo(Path.Combine(DbScriptsDir, subPath, name)).FullName;

        Func<DbConnection> CreateConnection = () => new MySqlConnection(ServerConnString);

        [TestInitialize]
        public async Task TestInitialize()
        {
            /*
             * Before the first run, download the image:
             * docker pull mysql:9.1.0
             * */

            await DockerInitialize(
                "mysql:9.1.0",
                [$"MYSQL_ROOT_PASSWORD={Pwd}"],
                "3306",
                CreateConnection
            );
        }

        [TestMethod]
        public void Ensure_CreateANewDb()
        {
            var engine = new ToolEngine(Env, Logger);

            var result = engine.Run("upgrade", "--ensure", GetConfigPath());
            result.Should().Be(0);

            using (var connection = new MySqlConnection(DbConnString))
            using (var command = new MySqlCommand("select count(*) from schemaversions where scriptname = '001.sql'",
                       connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }

        /*
         * // Don't supported
        [TestMethod]
        public void Drop_DropADb()
        {
            var engine = new ToolEngine(Env, Logger);

            engine.Run("upgrade", "--ensure", GetConfigPath());
            var result = engine.Run("drop", GetConfigPath());
            result.Should().Be(0);
            using (var connection = new MySqlConnection(DbConnString))
            using (var command = new MySqlCommand("select count(*) from schemaversions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<SqlException>($"Database {DbName} should not exist");
            }
        }
        */

        [TestMethod]
        public void DatabaseShouldNotExistBeforeTestRun()
        {
            using (var connection = new MySqlConnection(DbConnString))
            using (var command = new MySqlCommand("select count(*) from schemaversions where scriptname = '001.sql'",
                       connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<MySqlException>($"Database {DbName} should not exist");
            }
        }

        [TestMethod]
        [Ignore("MySql does not throw when timeout is reached. This should be validated a different way.")]
        public void UpgradeCommand_ShouldUseConnectionTimeoutForLongrunningQueries()
        {
            var engine = new ToolEngine(Env, Logger);

            var r = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "Timeout"));
            r.Should().Be(1);
        }

        [TestMethod]
        public void UpgradeCommand_ShouldUseASpecifiedJournal()
        {
            var engine = new ToolEngine(Env, Logger);

            var result = engine.Run("upgrade", "--ensure", GetConfigPath("dbup.yml", "JournalTableScript"));
            result.Should().Be(0);

            using (var connection = new MySqlConnection(DbConnString))
            using (var command = new MySqlCommand("select count(*) from DbUp.testTable where scriptname = '001.sql'",
                       connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }
    }
}