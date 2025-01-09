using CommandDotNet;
using CommandDotNet.TestTools;
using DbUp.Cli.Configuration;
using DbUp.Cli.Tests.RecordingDb;
using DbUp.Engine.Transactions;
using TestEnvironment = DbUp.Cli.Tests.TestEnvironment;

namespace DbUp.Cli.Tests;

public class TestHost
{
    public CaptureLogsLogger Logger { get; } = new();
    public TestEnvironment Environment { get; }
    public DelegateConnectionFactory TestConnectionFactory { get; }
    public AppRunner AppRunner { get; }

    public TestHost()
    {
        Environment = new TestEnvironment();
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
    
    public string EnsureTempDbUpYmlFileExists()
    {
        var dbupYmlPath = ProjectPaths.GetTempPath("dbup.yml");
        if (!File.Exists(dbupYmlPath))
        {
            EnsureDirectoryExists(ProjectPaths.TempDir);
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