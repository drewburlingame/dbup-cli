using System;
using System.Data;

namespace DbUp.Cli.Tests.TestInfrastructure
{
    public class RecordingDbTransaction: IDbTransaction
    {
        readonly CaptureLogsLogger logger;

        public RecordingDbTransaction(CaptureLogsLogger logger)
        {
            this.logger = logger;
        }

        public void Dispose()
        {
            logger.LogDbOperation("Dispose transaction");
        }

        public void Commit()
        {
            logger.LogDbOperation("Commit transaction");
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public IDbConnection Connection { get; private set; }
        public IsolationLevel IsolationLevel { get; private set; }
    }
}