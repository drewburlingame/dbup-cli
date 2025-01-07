using DbUp.Builder;
using DbUp.Cli.Configuration;
using DbUp.Cli.DbUpCustomization;
using DbUp.Engine.Output;

namespace DbUp.Cli.DbProviders;

internal class AzureSqlServerDbProvider : DbProvider
{
    public override Provider Provider => Provider.SqlServer;

    public override UpgradeEngineBuilder CreateUpgradeEngineBuilder(ConnectionInfo connectionInfo) =>
        UseAzureSqlIntegratedSecurity(connectionInfo.ConnectionString)
            ? DeployChanges.To
                .AzureSqlDatabaseWithIntegratedSecurity(connectionInfo.ConnectionString)
                .WithExecutionTimeout(connectionInfo.Timeout)
            : DeployChanges.To
                .SqlDatabase(connectionInfo.ConnectionString)
                .WithExecutionTimeout(connectionInfo.Timeout);

    public override void EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        if (UseAzureSqlIntegratedSecurity(connectionInfo.ConnectionString))
        {
            EnsureDatabase.For.AzureSqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        }
        else
        {
            EnsureDatabase.For.SqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        }
    }

    public override void DropDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        if (UseAzureSqlIntegratedSecurity(connectionInfo.ConnectionString))
        {
            DropDatabase.For.AzureSqlDatabase(connectionInfo.ConnectionString, logger);
        }
        else
        {
            DropDatabase.For.SqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        }
    }
    
    private static bool UseAzureSqlIntegratedSecurity(string connectionString) =>
        !(connectionString.Contains("Password", StringComparison.InvariantCultureIgnoreCase) ||
          connectionString.Contains("Integrated Security", StringComparison.InvariantCultureIgnoreCase) ||
          connectionString.Contains("Trusted_Connection", StringComparison.InvariantCultureIgnoreCase));
}