using DbUp.Cli.Configuration;
using DbUp.Cli.DbProviders;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine;
using DbUp.SqlServer;
using FluentAssertions;

namespace DbUp.Cli.Tests;

public class ProvidersTests
{
    private readonly List<SqlScript> scripts =
    [
        new("Script1.sql", "create table Foo (Id int identity)")
        //new SqlScript("Script2.sql", "alter table Foo add column Name varchar(255)"),
        //new SqlScript("Script3.sql", "insert into Foo (Name) values ('test')")
    ];

    [Fact]
    public void SelectDbProvider_ShouldThrow_IfAProviderIsNotSupported()
    {
        var ex = Assert.Throws<UnsupportedProviderException>(() => Providers.CreateUpgradeEngineBuilder(
            Provider.UnsupportedProvider,
            @"Data Source=(localdb)\dbup;Initial Catalog=dbup-tests;Integrated Security=True",
            60));

        ex.Message.Should().Be("Unsupported provider: UnsupportedProvider");
    }

    [Fact]
    public void SelectDbProvider_ShouldReturnReturnAValidProvider_ForSqlServer()
    {
        var builder = Providers.CreateUpgradeEngineBuilder(Provider.SqlServer, 
            @"Data Source=(localdb)\dbup;Initial Catalog=dbup-tests;Integrated Security=True", 60);
        
        builder.Configure(c => c.ConnectionManager.Should().BeOfType(typeof(SqlConnectionManager)));
        builder.WithScripts(new TestScriptProvider(scripts));
        builder.Build();
    }
}