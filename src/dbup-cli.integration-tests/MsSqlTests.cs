using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Npgsql;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class MsSqlTests() 
        : ContainerTest<MsSqlBuilder, MsSqlContainer, MsSqlConfiguration>("sqlserver")
    {
        protected override MsSqlBuilder NewBuilder => new();

        protected override string ReplaceDbInConnString(string connectionString, string dbName) => 
            connectionString.Replace(";Database=master;", $";Database={dbName};");

        protected override IDbConnection GetConnection(string connectionString) => 
            new SqlConnection(connectionString);

        protected override void AssertDbDoesNotExist(string connectionString, string dbName)
        {
            using var connection = GetConnection(connectionString);
            Action a = () => connection.Open();
            a.Should().Throw<SqlException>($"Database {dbName} should not exist");
        }

        protected override string QueryCountOfScript001 =>
            "select count(*) from SchemaVersions where scriptname = '001.sql'";

        protected override string QueryCountOfScript001FromCustomJournal =>
            "select count(*) from dbo.testTable where scriptname = '001.sql'";
    }
    
    [TestClass]
    public class PgSqlTests() : ContainerTest<PostgreSqlBuilder, PostgreSqlContainer, PostgreSqlConfiguration>("postgresql")
    {
        protected override PostgreSqlBuilder NewBuilder => new();

        protected override string ReplaceDbInConnString(string connectionString, string dbName) => 
            connectionString.Replace(";Database=postgres;", $";Database={dbName};");

        protected override IDbConnection GetConnection(string connectionString) => 
            new NpgsqlConnection(connectionString);

        protected override void AssertDbDoesNotExist(string connectionString, string dbName)
        {
            using var connection = GetConnection(connectionString);
            Action a = () => connection.Open();
            a.Should().Throw<PostgresException>($"Database {dbName} should not exist");
        }

        protected override string QueryCountOfScript001 =>
            "select count(*) from SchemaVersions where scriptname = '001.sql'";

        protected override string QueryCountOfScript001FromCustomJournal =>
            "select count(*) from public.journal where scriptname = '001.sql'";

        [Ignore("Not supported")]
        public override void Drop_DropADb()
        {
        }
    }
}
