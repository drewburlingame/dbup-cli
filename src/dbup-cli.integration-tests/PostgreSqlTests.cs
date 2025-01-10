using System.Data;
using FluentAssertions;
using Npgsql;
using Testcontainers.PostgreSql;

namespace DbUp.Cli.IntegrationTests;

public class PostgreSqlTests(ITestOutputHelper output)
    : ContainerTest<PostgreSqlBuilder, PostgreSqlContainer, PostgreSqlConfiguration>("PostgreSql", output)
{
    protected override PostgreSqlBuilder NewBuilder => new();

    protected override string ReplaceDbInConnString(string connectionString, string dbName) =>
        connectionString.Replace(";Database=postgres;", $";Database={dbName};");

    protected override IDbConnection GetConnection(string connectionString) =>
        new NpgsqlConnection(connectionString);

    protected override void AssertDbDoesNotExist(string connectionString, string dbName)
    {
        using var connection = GetConnection(connectionString);
        // ReSharper disable once AccessToDisposedClosure
        Action a = () => connection.Open();
        a.Should().Throw<PostgresException>($"Database {dbName} should not exist");
    }

    protected override string QueryCountOfScript001 =>
        "select count(*) from SchemaVersions where scriptname = '001.sql'";

    protected override string QueryCountOfScript001FromCustomJournal =>
        "select count(*) from journal where scriptname = '001.sql'";

    [Fact(Skip = "Not supported")]
    public override Task Drop_DropADb() => Task.CompletedTask;
}