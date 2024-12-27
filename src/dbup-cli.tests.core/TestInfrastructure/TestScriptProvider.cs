using System.Collections.Generic;
using DbUp.Engine;
using DbUp.Engine.Transactions;

namespace DbUp.Cli.Tests.TestInfrastructure;

public class TestScriptProvider: IScriptProvider
{
    private readonly List<SqlScript> sqlScripts;

    public TestScriptProvider(List<SqlScript> sqlScripts)
    {
        this.sqlScripts = sqlScripts;
    }

    public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
    {
        return sqlScripts;
    }
}