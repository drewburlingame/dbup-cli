using System.Data;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Testcontainers.MySql;
using MySqlConfiguration = Testcontainers.MySql.MySqlConfiguration;

namespace DbUp.Cli.IntegrationTests;

[TestClass]
public class MySqlTests()
    : ContainerTest<MySqlBuilder, MySqlContainer, MySqlConfiguration>("MySql")
{
    protected override MySqlBuilder NewBuilder => new MySqlBuilder()
        .WithUsername("root"); // root has perms for mysql for connecting to create the dbup db

    protected override int DbNameLengthLimit => 60;

    protected override string ReplaceDbInConnString(string connectionString, string dbName) =>
        connectionString.Replace(";Database=test;", $";Database={dbName};");

    protected override IDbConnection GetConnection(string connectionString) =>
        new MySqlConnection(connectionString);

    protected override void AssertDbDoesNotExist(string connectionString, string dbName)
    {
        using var connection = GetConnection(connectionString);
        Action a = () => connection.Open();
        a.Should().Throw<MySqlException>($"Database {dbName} should not exist");
    }

    protected override string QueryCountOfScript001 =>
        "select count(*) from schemaversions where scriptname = '001.sql'";

    protected override string QueryCountOfScript001FromCustomJournal =>
        "select count(*) from journal where scriptname = '001.sql'";

    [Ignore("MySql provider does not throw on timeout. Find another way to confirm.")]
    public override void UpgradeCommand_ShouldFailOnCommandTimeout()
    {
    }

    [Ignore("Not supported")]
    public override void Drop_DropADb()
    {
    }
}