using System.Data;
using System.Data.Common;
using DbUp.Engine;

namespace DbUp.Cli.Tests.RecordingDb;

internal class RecordingDbCommand(
    CaptureLogsLogger logger,
    SqlScript[] runScripts,
    string schemaTableName,
    Dictionary<string, Func<object>> scalarResults,
    Dictionary<string, Func<int>> nonQueryResults)
    : IDbCommand
{
    public IDbDataParameter CreateParameter()
    {
        logger.LogDbOperation("Create parameter");
        return new RecordingDbDataParameter();
    }
    
    public void Dispose() => logger.LogDbOperation("Dispose command");

    public int ExecuteNonQuery()
    {
        logger.LogDbOperation($"Execute non query command: {CommandText}");

        if (CommandText == "error")
            ThrowError();

        return nonQueryResults.TryGetValue(CommandText, out var result) 
            ? result() 
            : 0;
    }

    public object ExecuteScalar()
    {
        logger.LogDbOperation($"Execute scalar command: {CommandText}");

        if (CommandText == "error")
            ThrowError();

        // Are we checking if schemaversions exists
        if (CommandText.IndexOf(schemaTableName, StringComparison.OrdinalIgnoreCase) != -1)
        {
            if (runScripts != null)
                return 1;
            return 0;
        }

        return scalarResults.TryGetValue(CommandText, out var result) 
            ? result() 
            : null;
    }

    public IDataReader ExecuteReader()
    {
        logger.LogDbOperation($"Execute reader command: {CommandText}");

        if (CommandText == "error")
            ThrowError();

        // Reading SchemaVersions
        if (CommandText.IndexOf(schemaTableName, StringComparison.OrdinalIgnoreCase) != -1)
        {
            return new ScriptReader(runScripts);
        }

        return new EmptyReader();
    }

    private void ThrowError() => throw new TestDbException();

    public IDbConnection Connection { get; set; }
    public IDbTransaction Transaction { get; set; }

    /// <summary>
    /// Set to 'error' to throw when executed
    /// </summary>
    public string CommandText { get; set; }
    public int CommandTimeout { get; set; }
    public CommandType CommandType { get; set; }
    public IDataParameterCollection Parameters { get; } = new RecordingDataParameterCollection(logger);
    public UpdateRowSource UpdatedRowSource { get; set; }
    
    public IDataReader ExecuteReader(CommandBehavior behavior) => throw new NotImplementedException();
    public void Prepare() => throw new NotImplementedException();
    public void Cancel() => throw new NotImplementedException();

    private class TestDbException: DbException
    {
    }
}