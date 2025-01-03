using System.Reflection;
using CommandLine;
using DbUp.Cli.CommandLineOptions;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.Cli;

public class ToolEngine
{
    private IEnvironment Environment { get; }
    private IUpgradeLog Logger { get; }
    private IConnectionFactory ConnectionFactory { get; }

    private readonly Parser ArgsParser = new(cfg =>
    {
        cfg.CaseInsensitiveEnumValues = true;
        cfg.AutoHelp = true;
        cfg.AutoVersion = true;
        cfg.HelpWriter = Console.Out;
    });

    public ToolEngine(IEnvironment environment, IUpgradeLog logger,IConnectionFactory connectionFactory)
    {
        // ConnectionFactory to override the default. Mostly used for mocking
        ConnectionFactory = connectionFactory;
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public ToolEngine(IEnvironment environment, IUpgradeLog logger)
        : this(environment, logger, null)
    {
    }

    public RunResult Run(params string[] args)
    {
        return ArgsParser
            .ParseArguments<InitOptions, UpgradeOptions, MarkAsExecutedOptions, DropOptions, StatusOptions>(args)
            .MapResult(
                (InitOptions opts) => WrapException(() => RunInitCommand(opts)),
                (UpgradeOptions opts) => WrapException(() => RunUpgradeCommand(opts)),
                (MarkAsExecutedOptions opts) => WrapException(() => RunMarkAsExecutedCommand(opts)),
                (DropOptions opts) => WrapException(() => RunDropCommand(opts)),
                (StatusOptions opts) => WrapException(() => RunStatusCommand(opts)),
                parseErrors => WrapException(() => ParseErrors(parseErrors)));
    }

    private int ParseErrors(IEnumerable<CommandLine.Error> parseErrors)
    {
        foreach (var err in parseErrors)
        {
            if (err.StopsProcessing)
            {
                // Autoimplemented verbs and params
                switch (err.Tag)
                {
                    case ErrorType.VersionRequestedError:
                    case ErrorType.HelpRequestedError:
                    case ErrorType.HelpVerbRequestedError:
                        return 0;
                }
            }
        }

        return 1;
    }

    private int RunStatusCommand(StatusOptions opts)
    {
        Environment.LoadEnvironmentVariables(opts.File, opts.EnvFiles);
        var configFilePath = Environment.GetFilePath(opts.File, fileShouldExist: true);
        var x = ConfigLoader.LoadMigration(configFilePath, Environment);
        var builder = Providers
            .CreateUpgradeEngineBuilder(x.Provider, x.ConnectionString, x.ConnectionTimeoutSec)
            .SelectJournal(x.Provider, x.JournalTo)
            .SelectTransaction(x.Transaction)
            .SelectLogOptions(Logger, VerbosityLevel.Min)
            .SelectScripts(x.Scripts, x.Naming)
            .AddVariables(x.Vars, x.DisableVars)
            .OverrideConnectionFactory(ConnectionFactory);
        
        var engine = builder.Build();
        if (!engine.TryConnect(out var message))
        {
            throw new FailedToConnectException(message);
        }

        int result = 0;
        if (engine.IsUpgradeRequired())
        {
            result = -1; // Indicates that the upgrade is required
            var scriptsToExecute = engine.GetScriptsToExecute().Select(s => s.Name)
                .ToList();
            PrintGeneralUpgradeInformation(scriptsToExecute);

            if (opts.NotExecuted)
            {
                Logger.LogInformation("");
                PrintScriptsToExecute(scriptsToExecute);
            }
        }
        else
        {
            Logger.LogInformation(
                "Database is up-to-date. Upgrade is not required.");
        }

        if (opts.Executed)
        {
            Logger.LogInformation("");
            PrintExecutedScripts(engine);
        }

        return result;
    }

    private void PrintGeneralUpgradeInformation(List<string> scripts)
    {
        Logger.LogInformation("Database upgrade is required.");
        Logger.LogInformation($"You have {scripts.Count} more scripts to execute.");
    }

    private void PrintScriptsToExecute(List<string> scripts)
    {
        Logger.LogInformation("These scripts will be executed:");
        scripts.ForEach(s => Logger.LogInformation($"    {s}"));
    }

    private void PrintExecutedScripts(UpgradeEngine engine)
    {
        var executed = engine.GetExecutedScripts();
        if (executed.Count == 0)
        {
            Logger.LogInformation("It seems you have no scripts executed yet.");
        }
        else
        {
            Logger.LogInformation("");
            Logger.LogInformation("Already executed scripts:");
            executed.ForEach(s => Logger.LogInformation($"    {s}"));
        }
    }

    private int RunUpgradeCommand(UpgradeOptions opts)
    {
        Environment.LoadEnvironmentVariables(opts.File, opts.EnvFiles);
        var configFilePath = Environment.GetFilePath(opts.File, fileShouldExist: true);
        var x = ConfigLoader.LoadMigration(configFilePath, Environment);
        var builder = Providers
            .CreateUpgradeEngineBuilder(x.Provider, x.ConnectionString, x.ConnectionTimeoutSec)
            .SelectJournal(x.Provider, x.JournalTo)
            .SelectTransaction(x.Transaction)
            .SelectLogOptions(Logger, opts.Verbosity)
            .SelectScripts(x.Scripts, x.Naming)
            .AddVariables(x.Vars, x.DisableVars)
            .OverrideConnectionFactory(ConnectionFactory);
        
        var engine = builder.Build();
        if (opts.Ensure)
        {
            Providers.EnsureDb(Logger, x.Provider, x.ConnectionString, x.ConnectionTimeoutSec);
        }

        var result = engine.PerformUpgrade();

        AssertSuccess(result, "perform upgrade");

        return 0;
    }

    private int RunMarkAsExecutedCommand(MarkAsExecutedOptions opts)
    {
        Environment.LoadEnvironmentVariables(opts.File, opts.EnvFiles);
        var configFilePath = Environment.GetFilePath(opts.File, fileShouldExist: true);
        var x = ConfigLoader.LoadMigration(configFilePath, Environment);
        var builder = Providers
            .CreateUpgradeEngineBuilder(x.Provider, x.ConnectionString, x.ConnectionTimeoutSec)
            .SelectJournal(x.Provider, x.JournalTo)
            .SelectTransaction(x.Transaction)
            .SelectLogOptions(Logger, opts.Verbosity)
            .SelectScripts(x.Scripts, x.Naming)
            .AddVariables(x.Vars, x.DisableVars)
            .OverrideConnectionFactory(ConnectionFactory);
        
            var engine = builder.Build();
            if (opts.Ensure)
            {
                Providers.EnsureDb(Logger, x.Provider, x.ConnectionString, x.ConnectionTimeoutSec);
            }

            if (!engine.TryConnect(out var message))
            {
                throw new FailedToConnectException(message);
            }
            
            return AssertSuccess("mark as executed", engine.MarkAsExecuted);
    }

    private int RunDropCommand(DropOptions opts)
    {
        Environment.LoadEnvironmentVariables(opts.File, opts.EnvFiles);
        var configFilePath = Environment.GetFilePath(opts.File, fileShouldExist: true);
        var x = ConfigLoader.LoadMigration(configFilePath, Environment);
        Providers.DropDb(Logger, x.Provider, x.ConnectionString, x.ConnectionTimeoutSec);
        return 0;
    }

    private int RunInitCommand(InitOptions opts)
    {
        var filePath = Environment.GetFilePath(opts.File, fileShouldExist: false);
        Environment.WriteFile(filePath, GetDefaultConfigFile());
        return 0;
    }

    public static string GetDefaultConfigFile()
    {
        using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Constants.Default.ConfigFileResourceName);
        if (resourceStream is null) 
            throw new Exception($"Missing default embedded resource: {Constants.Default.ConfigFileResourceName}");
        
        using var reader = new StreamReader(resourceStream);
        return reader.ReadToEnd();
    }
    
    private RunResult WrapException(Func<int> f)
    {
        try
        {
            var x = f();
            return new RunResult(x, null);
        }
        catch (Exception ex)
        {
            var msg = ex.FlattenErrorMessages();
            Console.WriteLine(msg);
            return new RunResult(1, msg);
        }
    }


    private static int AssertSuccess(string description, Func<DatabaseUpgradeResult> action) => 
        AssertSuccess(action(), description);

    private static int AssertSuccess(DatabaseUpgradeResult result, string description)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Successful) return 0;

        var msg = $"Failed to {description}: {result.Error?.FlattenErrorMessages() ?? "Undefined error"}";

        if (result.ErrorScript != null)
        {
            msg += $"{System.Environment.NewLine}    Script: {result.ErrorScript.Name}";
        }

        throw new CommandFailedException(msg);
    }
}