using System.Data;

namespace DbUp.Cli.Tests.RecordingDb;

internal class RecordingDbTransaction(CaptureLogsLogger logger) : IDbTransaction
{
    public void Dispose() => logger.LogDbOperation("Dispose transaction");
    public void Commit() => logger.LogDbOperation("Commit transaction");

    // ReSharper disable UnusedAutoPropertyAccessor.Local
    public IDbConnection Connection { get; private set; }
    public IsolationLevel IsolationLevel { get; private set; }

    public void Rollback() => throw new NotImplementedException();
}