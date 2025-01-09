using System.Data;
using FluentAssertions;
using MySql.Data.MySqlClient;
using Testcontainers.MySql;
using MySqlConfiguration = Testcontainers.MySql.MySqlConfiguration;

namespace DbUp.Cli.IntegrationTests;

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

    [Fact(Skip = "MySql provider does not throw on timeout. Find another way to confirm.")]
    public override Task UpgradeCommand_ShouldFailOnCommandTimeout() => Task.CompletedTask;

    [Fact(Skip = "Not supported")]
    public override Task Drop_DropADb() => Task.CompletedTask;
}