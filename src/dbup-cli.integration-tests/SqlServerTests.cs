using DbUp.Cli.Tests.TestInfrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using FluentAssertions;
using System.Threading.Tasks;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class SqlServerTests : DockerBasedTest
    {
        private const string Pwd = "SaPwd1!$";
        private static readonly string DefaultConnString = BuildConnString("master");
        private static readonly string DbUpConnString = BuildConnString("dbup");

        private static string BuildConnString(string db)
        {
            return $"Database={db};Data Source={HostIp};Persist Security Info=True;User ID=sa;Password={Pwd};Encrypt=Yes;TrustServerCertificate=Yes;";
        }

        private static readonly string DbScriptsDir = Path.Combine(ProjectPaths.ScriptsDir, "SqlServer");
        
        readonly CaptureLogsLogger Logger;
        readonly IEnvironment Env;

        public SqlServerTests()
        {
            Env = new CliEnvironment();
            Logger = new CaptureLogsLogger();

            Environment.SetEnvironmentVariable("CONNSTR", DbUpConnString);
        }
        
        private static string GetConfigPath(string name = "dbup.yml", string subPath = "EmptyScript") =>
            new DirectoryInfo(Path.Combine(DbScriptsDir, subPath, name)).FullName;

        Func<DbConnection> CreateConnection = () => new SqlConnection(DefaultConnString);

        [TestInitialize]
        public async Task TestInitialize()
        {
            /*
             * Before the first run, download the image:
             * docker pull mcr.microsoft.com/mssql/server:2022-latest
             * */

            await DockerInitialize(
                "mcr.microsoft.com/mssql/server:2022-latest",
                [
                    "ACCEPT_EULA=Y",
                    $"MSSQL_SA_PASSWORD={Pwd}"
                ],
                "1433",
                CreateConnection
                );
        }

        [TestMethod]
        public void Ensure_CreateANewDb()
        {
            var engine = new ToolEngine(Env, Logger);

            var result = engine.Run("upgrade", "--ensure", GetConfigPath());
            result.Should().Be(0);

            using (var connection = new SqlConnection(DbUpConnString))
            using (var command = new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }

        [TestMethod]
        public void Drop_DropADb()
        {
            var engine = new ToolEngine(Env, Logger);

            engine.Run("upgrade", "--ensure", GetConfigPath());
            var result = engine.Run("drop", GetConfigPath());
            result.Should().Be(0);
            using (var connection = new SqlConnection(DbUpConnString))
            using (var command = new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<SqlException>("Database DbUp should not exist");
            }
        }

        [TestMethod]
        public void DatabaseShouldNotExistBeforeTestRun()
        {
            using (var connection = new SqlConnection(DbUpConnString))
            using (var command = new SqlCommand("select count(*) from SchemaVersions where scriptname = '001.sql'", connection))
            {
                Action a = () => connection.Open();
                a.Should().Throw<SqlException>("Database DbUp should not exist");
            }
        }

        [TestMethod]
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

            using (var connection = new SqlConnection(DbUpConnString))
            using (var command = new SqlCommand("select count(*) from dbo.testTable where scriptname = '001.sql'", connection))
            {
                connection.Open();
                var count = command.ExecuteScalar();

                count.Should().Be(1);
            }
        }
    }
}
