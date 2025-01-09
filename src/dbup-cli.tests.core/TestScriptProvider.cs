using DbUp.Engine;
using DbUp.Engine.Transactions;

namespace DbUp.Cli.Tests;

public class TestScriptProvider(List<SqlScript> sqlScripts) : IScriptProvider
{
    public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager) => sqlScripts;
}