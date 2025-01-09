using System.Reflection;

namespace DbUp.Cli.Tests.TestInfrastructure;

public static class ProjectPaths
{
    // Directory = src/dbup-cli.tests/bin/Debug/net9.0, RootDir becomes src/dbup-cli.tests/bin/
    public static string RootDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.Parent!.Parent!.FullName;
    public static string ScriptsDir = Path.Combine(RootDir, "..", "Scripts");
    public static string ConfigDir = Path.Combine(ScriptsDir, "Config");
    public static  string GetConfigPath(string name) => new DirectoryInfo(Path.Combine(ConfigDir, name)).FullName;
    public static  string GetConfigPath(params string[] names) => new DirectoryInfo(Path.Combine(names.Prepend(ConfigDir).ToArray())).FullName;
        
    public static string TempDir = Path.Combine(RootDir, "temp");
    public static string GetTempPath(string name) => Path.Combine(TempDir, name);
        
}