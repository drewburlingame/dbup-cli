using CommandDotNet;
using CommandDotNet.TestTools;
using DbUp.Cli.Configuration;
using DbUp.Cli.Tests.RecordingDb;
using DbUp.Engine.Transactions;

namespace DbUp.Cli.Tests;

public class TestHost
{
    public CaptureLogsLogger Logger { get; } = new();
    public TestEnvironment Environment { get; }
    public DelegateConnectionFactory TestConnectionFactory { get; }
    public AppRunner AppRunner { get; }

    public TestHost(string currentDirectory = null)
    {
        Environment = new TestEnvironment(currentDirectory);
        var recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
        TestConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        AppRunner = Program.NewAppRunner(
            Environment,
            console => new CaptureLogsLogger(console),
            TestConnectionFactory);
    }

    public AppRunnerResult Run(string args) => 
        AppRunner.RunInMem(args, Ambient.WriteLine, config: CommandDotNetTestConfigs.Default);
    
    public AppRunnerResult Run(params string[] args) => 
        AppRunner.RunInMem(args, Ambient.WriteLine, config: CommandDotNetTestConfigs.Default);

    public string GetConfigPath(string filename) => Path.Combine(Environment.CurrentDirectory, filename);
    
    public string EnsureTempDbUpYmlFileExists()
    {
        var dbupYmlPath = GetConfigPath("dbup.yml");
        if (!File.Exists(dbupYmlPath))
        {
            EnsureDirectoryExists(Path.GetDirectoryName(dbupYmlPath));
            File.WriteAllText(dbupYmlPath, ConfigLoader.GetDefaultConfigFile());
        }
        return dbupYmlPath;
    }
    
    public void EnsureDirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (Directory.Exists(path)) return;
        Directory.CreateDirectory(path);
    }

    public void EnsureFileDoesNotExist(string path)
    {
        if (!File.Exists(path)) return;
        File.Delete(path);
    }
}