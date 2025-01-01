using DbUp.Builder;
using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine;
using DbUp.SqlServer;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace DbUp.Cli.Tests;

[TestClass]
public class ProvidersTests
{
    private readonly List<SqlScript> scripts =
    [
        new("Script1.sql", "create table Foo (Id int identity)")
        //new SqlScript("Script2.sql", "alter table Foo add column Name varchar(255)"),
        //new SqlScript("Script3.sql", "insert into Foo (Name) values ('test')")
    ];

    [TestMethod]
    public void SelectDbProvider_ShouldReturnNone_IfAProviderIsNotSupported()
    {
        var builder = Providers.CreateUpgradeEngineBuilder(Provider.UnsupportedProvider, 
            @"Data Source=(localdb)\dbup;Initial Catalog=dbup-tests;Integrated Security=True", 60);
        builder.HasValue.Should().BeFalse();
        builder.GetErrorOrThrow().Should().Be("Unsupported provider: UnsupportedProvider");
    }

    [TestMethod]
    public void SelectDbProvider_ShouldReturnReturnAValidProvider_ForSqlServer()
    {
        var option = Providers.CreateUpgradeEngineBuilder(Provider.SqlServer, 
            @"Data Source=(localdb)\dbup;Initial Catalog=dbup-tests;Integrated Security=True", 60);
        var builder = option.GetValueOrThrow();
        
        builder.Configure(c => c.ConnectionManager.Should().BeOfType(typeof(SqlConnectionManager)));
        builder.WithScripts(new TestScriptProvider(scripts));
        builder.Build();
    }
}