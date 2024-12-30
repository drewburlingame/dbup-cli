namespace DbUp.Cli.Tests.TestInfrastructure;

public class TestEnvironment(string currentDirectory = null) : CliEnvironment
{
    public override string GetCurrentDirectory() => currentDirectory ?? ProjectPaths.TempDir;
}