using System.Data;
using DbUp.Engine;

namespace DbUp.Cli.Tests.TestInfrastructure;

public class RecordingDbConnection: IDbConnection
{
    private readonly Dictionary<string, Func<object>> scalarResults = new Dictionary<string, Func<object>>();
    private readonly Dictionary<string, Func<int>> nonQueryResults = new Dictionary<string, Func<int>>();
    private readonly CaptureLogsLogger logger;
    private readonly string schemaTableName;
    private SqlScript[] runScripts;

    public RecordingDbConnection(CaptureLogsLogger logger, string schemaTableName)
    {
        this.logger = logger;
        this.schemaTableName = schemaTableName;
    }

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

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void ChangeDatabase(string databaseName)
    {
        throw new NotImplementedException();
    }

    public IDbCommand CreateCommand()
    {
        return new RecordingDbCommand(logger, runScripts, schemaTableName, scalarResults, nonQueryResults);
    }

    public void Open()
    {
        logger.LogDbOperation("Open connection");
    }

    public void Dispose()
    {
        logger.LogDbOperation("Dispose connection");
    }

    public string ConnectionString { get; set; }

    public int ConnectionTimeout { get; private set; }

    public string Database { get; private set; }

    public ConnectionState State { get; private set; }

    public void SetupRunScripts(params SqlScript[] runScripts)
    {
        this.runScripts = runScripts;
    }

    public void SetupScalarResult(string sql, Func<object> action)
    {
        scalarResults.Add(sql, action);
    }

    public void SetupNonQueryResult(string sql, Func<int> result)
    {
        nonQueryResults.Add(sql, result);
    }
}