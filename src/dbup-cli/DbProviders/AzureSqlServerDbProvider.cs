using System;
using DbUp.Builder;
using DbUp.Cli.DbUpCustomization;
using DbUp.Engine.Output;
using Optional;

namespace DbUp.Cli.DbProviders;

public class AzureSqlServerDbProvider : DbProvider
{
    public override Provider Provider => Provider.SqlServer;

    public override Option<UpgradeEngineBuilder, Error> SelectDbProvider(ConnectionInfo connectionInfo) =>
        UseAzureSqlIntegratedSecurity(connectionInfo.ConnectionString)
            ? DeployChanges.To
                .AzureSqlDatabaseWithIntegratedSecurity(connectionInfo.ConnectionString)
                .WithExecutionTimeout(connectionInfo.Timeout)
                .Some<UpgradeEngineBuilder, Error>()
            : DeployChanges.To
                .SqlDatabase(connectionInfo.ConnectionString)
                .WithExecutionTimeout(connectionInfo.Timeout)
                .Some<UpgradeEngineBuilder, Error>();

    public override Option<bool, Error> EnsureDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        if (UseAzureSqlIntegratedSecurity(connectionInfo.ConnectionString))
        {
            EnsureDatabase.For.AzureSqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        }
        else
        {
            EnsureDatabase.For.SqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        }
        return true.Some<bool, Error>();
    }

    public override Option<bool, Error> DropDb(IUpgradeLog logger, ConnectionInfo connectionInfo)
    {
        if (UseAzureSqlIntegratedSecurity(connectionInfo.ConnectionString))
        {
            DropDatabase.For.AzureSqlDatabase(connectionInfo.ConnectionString, logger);
        }
        else
        {
            DropDatabase.For.SqlDatabase(connectionInfo.ConnectionString, logger, connectionInfo.ConnectionTimeoutSec);
        }
        return true.Some<bool, Error>();
    }
    
    private static bool UseAzureSqlIntegratedSecurity(string connectionString) =>
        !(connectionString.Contains("Password", StringComparison.InvariantCultureIgnoreCase) ||
          connectionString.Contains("Integrated Security", StringComparison.InvariantCultureIgnoreCase) ||
          connectionString.Contains("Trusted_Connection", StringComparison.InvariantCultureIgnoreCase));
}