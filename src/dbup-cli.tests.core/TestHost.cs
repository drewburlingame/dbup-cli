using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Transactions;
using Optional;

namespace DbUp.Cli.Tests;

public class TestHost
{
    private readonly string tempDbUpYmlPath = ProjectPaths.GetTempPath("dbup.yml");

    public CaptureLogsLogger Logger { get; } = new();
    public TestEnvironment Environment { get; }
    public DelegateConnectionFactory TestConnectionFactory { get; }
    public ToolEngine ToolEngine { get; private set; }

    public TestHost()
    {
        Environment = new TestEnvironment();
        var recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
        TestConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
        ToolEngine = new ToolEngine(Environment, Logger, (TestConnectionFactory as IConnectionFactory).Some()); 
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