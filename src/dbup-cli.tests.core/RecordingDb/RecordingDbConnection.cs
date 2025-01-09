using System.Data;
using DbUp.Engine;

namespace DbUp.Cli.Tests.RecordingDb;

internal class RecordingDbConnection(CaptureLogsLogger logger, string schemaTableName) : IDbConnection
{
    private readonly Dictionary<string, Func<object>> scalarResults = new();
    private readonly Dictionary<string, Func<int>> nonQueryResults = new();
    private SqlScript[] runScripts = null!;

    public IDbTransaction BeginTransaction()
    {
        logger.LogDbOperation("Begin transaction");
        return new RecordingDbTransaction(logger);
    }

    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        logger.LogDbOperation($"Begin transaction with isolationLevel of {il}");
        return new RecordingDbTransaction(logger);
    }

    public void Open() => logger.LogDbOperation("Open connection");

    public void Dispose() => logger.LogDbOperation("Dispose connection");

    public IDbCommand CreateCommand() => new RecordingDbCommand(logger, runScripts, schemaTableName, scalarResults, nonQueryResults);

    public string ConnectionString { get; set; }
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    public int ConnectionTimeout { get; private set; }
    public string Database { get; private set; }
    public ConnectionState State { get; private set; }
    
    public void Close() => throw new NotImplementedException();
    public void ChangeDatabase(string databaseName) => throw new NotImplementedException();
}