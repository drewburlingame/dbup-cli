using System;
using System.Data;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testcontainers.MsSql;

namespace DbUp.Cli.IntegrationTests
{
    [TestClass]
    public class MsSqlTests() 
        : ContainerTest<MsSqlBuilder, MsSqlContainer, MsSqlConfiguration>("SqlServer")
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
}
