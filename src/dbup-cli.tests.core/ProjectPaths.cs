using System.Reflection;

namespace DbUp.Cli.Tests;

public static class ProjectPaths
{
    // Directory = src/dbup-cli.tests/bin/Debug/net9.0, RootDir becomes src/dbup-cli.tests/bin/
    public static readonly string RootDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.Parent!.Parent!.FullName;
    public static readonly string ScriptsDir = Path.Combine(RootDir, "..", "Scripts");
    public static readonly string ConfigDir = Path.Combine(ScriptsDir, "Config");
        
    public static readonly string TempDir = Path.Combine(RootDir, "temp");
    public static string GetTempPath(string name) => Path.Combine(TempDir, name);
        
}