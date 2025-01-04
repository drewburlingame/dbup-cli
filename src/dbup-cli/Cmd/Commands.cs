using CommandDotNet;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.Cli.Cmd;

public class Commands
{
    [Command(Description = "Create a new config file")]
    public void Init(ConfigFileArgs args,  
    // the parameters below are injected from configuration
    IEnvironment environment, IConsole console)
    {
        var filePath = environment.GetFilePath(args.File, fileShouldExist: false);
        environment.WriteFile(filePath, ConfigLoader.GetDefaultConfigFile());
        console.WriteLine($"default config file created: {args.File}");
    }
    
    [Command(Description = "Drops database if exists. SqlServer and AzureSql providers only.")]
    public void Drop(MigrationArgs migrationArgs,  
        // the parameters below are injected from configuration
        IEnvironment environment, IUpgradeLog logger)
    {
        var migration = LoadMigration(migrationArgs, environment);
        Providers.DropDb(logger, migration);
        logger.LogInformation($"{migration.Provider} database dropped");
    }

    [Command(Description = "Upgrade database")]
    public void Upgrade(UpgradeEngineArgs upgradeEngineArgs, EnsureDbArgs ensureDbArgs, 
        // the parameters below are injected from configuration
        IEnvironment environment, IUpgradeLog logger, IConnectionFactory connectionFactory)
    {
        var migration = LoadMigration(upgradeEngineArgs, environment);
        var engine = BuildEngine(logger, connectionFactory, migration, upgradeEngineArgs.Verbosity);
        EnsureDb(ensureDbArgs, logger, migration);
        AssertSuccess("perform upgrade", engine.PerformUpgrade);
    }
    
    [Command(Description = "Mark all scripts as executed")]
    public void MarkAsExecuted(UpgradeEngineArgs upgradeEngineArgs, EnsureDbArgs ensureDbArgs, 
        // the parameters below are injected from configuration
        IEnvironment environment, IUpgradeLog logger, IConnectionFactory connectionFactory)
    {
        var migration = LoadMigration(upgradeEngineArgs, environment);
        var engine = BuildEngine(logger, connectionFactory, migration, upgradeEngineArgs.Verbosity);
        EnsureDb(ensureDbArgs, logger, migration);
        EnsureCanConnect(engine); // TODO: is this needed?  If so, why not in Upgrade?
        AssertSuccess("mark as executed", engine.MarkAsExecuted);
    }

    [Command(Description = "Show upgrade status")]
    public int Status(UpgradeEngineArgs upgradeEngineArgs, 
        [Option('x', Description = "Print names of executed scripts")]
        bool showExecuted,
        [Option('n', Description = "Print names of scripts to be execute")]
        bool showNotExecuted, 
        // the parameters below are injected from configuration
        IEnvironment environment, IUpgradeLog logger, IConnectionFactory connectionFactory, IConsole console)
    {
        var migration = LoadMigration(upgradeEngineArgs, environment);
        var engine = BuildEngine(logger, connectionFactory, migration, upgradeEngineArgs.Verbosity);
        EnsureCanConnect(engine); // TODO: is this needed?  If so, why not in Upgrade?

        var upgradeRequired = engine.IsUpgradeRequired();
        
        if (upgradeRequired)
        {
            var scripts = engine.GetScriptsToExecute().Select(s => s.Name).ToList();
            logger.LogInformation("Database upgrade is required.");
            logger.LogInformation($"{scripts.Count} scripts to execute.");
            
            if (showNotExecuted)
            {
                logger.LogInformation("");
                logger.LogInformation("These scripts will be executed:");
                scripts.ForEach(s => logger.LogInformation($"    {s}"));
            }
        }
        else
        {
            logger.LogInformation("Database is up-to-date. Upgrade is not required.");
        }
        
        if (showExecuted)
        {
            logger.LogInformation("");
            var scripts = engine.GetExecutedScripts();
            if (scripts.Count == 0)
            {
                logger.LogInformation("It seems you have no scripts executed yet.");
            }
            else
            {
                logger.LogInformation("");
                logger.LogInformation($"Already executed scripts ({scripts.Count}):");
                scripts.ForEach(s => logger.LogInformation($"    {s}"));
            }
        }

        return upgradeRequired ? -1 : 0;
    }

    private static Migration LoadMigration(MigrationArgs migrationArgs, IEnvironment environment)
    {
        environment.LoadEnvironmentVariables(migrationArgs.File, migrationArgs.EnvFiles);
        var configFilePath = environment.GetFilePath(migrationArgs.File, fileShouldExist: true);
        return ConfigLoader.LoadMigration(configFilePath, environment);
    }

    private static UpgradeEngine BuildEngine(IUpgradeLog logger,
        IConnectionFactory connectionFactory, Migration migration, VerbosityLevel verbosity) =>
        Providers
            .CreateUpgradeEngineBuilder(migration.Provider, migration.ConnectionString, migration.ConnectionTimeoutSec)
            .SelectJournal(migration.Provider, migration.JournalTo)
            .SelectTransaction(migration.Transaction)
            .SelectLogOptions(logger, verbosity)
            .SelectScripts(migration.Scripts, migration.Naming)
            .AddVariables(migration.Vars, migration.DisableVars)
            .OverrideConnectionFactory(connectionFactory)
            .Build();
    
    private static void EnsureDb(EnsureDbArgs ensureDbArgs, IUpgradeLog logger, Migration migration)
    {
        if (!ensureDbArgs.Ensure) return;
        
        Providers.EnsureDb(logger, migration);
    }

    private static void EnsureCanConnect(UpgradeEngine engine)
    {
        if (engine.TryConnect(out var message)) return;
        
        throw new FailedToConnectException(message);
    }
    
    private static void AssertSuccess(string description, Func<DatabaseUpgradeResult> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        
        var result = action();
        if (result.Successful) return;

        var msg = $"Failed to {description}: {result.Error?.FlattenErrorMessages() ?? "Undefined error"}";
        if (result.ErrorScript != null)
        {
            msg += $" @ Script: {result.ErrorScript.Name}";
        }
        throw new CommandFailedException(msg);
    }
}