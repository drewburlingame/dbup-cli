namespace DbUp.Cli.Tests.TestInfrastructure;

public class TestEnvironment(string currentDirectory = null) : 
    CliEnvironment(currentDirectory ?? ProjectPaths.TempDir)
{
    private const string inMemFilePrefix = "/in-mem/";
    private Dictionary<string, string> fileByPath = new();

    /// <summary>Save the file in memory and return a path prefixed with inMem root</summary>
    public string WriteFileInMem(string content, string path = null)
    {
        if(path is null)
            path = $"{inMemFilePrefix}{fileByPath.Count + 1}";
        else if (!path.StartsWith("inMem"))
            path = $"{inMemFilePrefix}{path}";

        fileByPath[path] = content;

        return path;
    }

    public override bool FileExists(string path)
    {
        return path.StartsWith(inMemFilePrefix) 
            ? fileByPath.ContainsKey(path)
            : base.FileExists(path);
    }

    public override string ReadFile(string path)
    {
        return path.StartsWith(inMemFilePrefix) 
            ? fileByPath[path] 
            : base.ReadFile(path);
    }
}