using DotNetEnv;

namespace DbUp.Cli.Tests;

public class TestEnvironment(string currentDirectory = null) : 
    CliEnvironment(currentDirectory ?? ProjectPaths.TempDir)
{
    private const string InMemFilePrefix = "/in-mem/";
    private readonly Dictionary<string, string> fileByPath = new();

    /// <summary>Save the file in memory and return a path prefixed with inMem root</summary>
    public string WriteFileInMem(string content, string path = null)
    {
        if(path is null)
            path = $"{InMemFilePrefix}{fileByPath.Count + 1}";
        else if (!path.StartsWith("inMem"))
            path = $"{InMemFilePrefix}{path}";

        fileByPath[path] = content;

        return path;
    }

    public override bool FileExists(string path)
    {
        return path.StartsWith(InMemFilePrefix) 
            ? fileByPath.ContainsKey(path)
            : base.FileExists(path);
    }

    public override string ReadFile(string path)
    {
        return path.StartsWith(InMemFilePrefix) 
            ? fileByPath[path] 
            : base.ReadFile(path);
    }

    public override void LoadDotEnv(string path)
    {
        using Stream stream = path.StartsWith(InMemFilePrefix) 
            ? new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileByPath[path]))
            : File.OpenRead(path);
        Env.Load(stream);
    }
}