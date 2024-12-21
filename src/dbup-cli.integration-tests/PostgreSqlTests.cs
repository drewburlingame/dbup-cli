using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FluentAssertions;
using System.Data;
using Npgsql;
using Testcontainers.PostgreSql;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class PostgreSqlTests()
        : ContainerTest<PostgreSqlBuilder, PostgreSqlContainer, PostgreSqlConfiguration>("postgresql")
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
            "select count(*) from journal where scriptname = '001.sql'";

        [Ignore("Not supported")]
        public override void Drop_DropADb()
        {
        }
    }
}
