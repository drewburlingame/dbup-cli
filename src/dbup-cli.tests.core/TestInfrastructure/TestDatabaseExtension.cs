using System.Data;
using DbUp.Builder;
using DbUp.Engine.Transactions;

namespace DbUp.Cli.Tests.TestInfrastructure;

/// <summary>
/// Configures DbUp to use SqlServer with a fake connection
/// </summary>
public static class TestDatabaseExtension
{
    public static UpgradeEngineBuilder OverrideConnectionFactory(this UpgradeEngineBuilder engineBuilder, IDbConnection connection) => 
        engineBuilder.OverrideConnectionFactory(new DelegateConnectionFactory(l => connection));

    public static UpgradeEngineBuilder OverrideConnectionFactory(this UpgradeEngineBuilder engineBuilder, IConnectionFactory connectionFactory)
    {
        engineBuilder.Configure(c => 
            ((DatabaseConnectionManager)c.ConnectionManager).OverrideFactoryForTest(connectionFactory));
        return engineBuilder;
    }
}