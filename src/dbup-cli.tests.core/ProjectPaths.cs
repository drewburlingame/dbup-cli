using System.Reflection;

namespace DbUp.Cli.Tests;

public static class ProjectPaths
{
    // Directory = src/dbup-cli.tests/bin/Debug/net9.0, RootDir becomes src/dbup-cli.tests/bin/
    internal static readonly string RootDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.Parent!.Parent!.FullName;
    internal static readonly string TempDir = Path.Combine(RootDir, "temp");
    public static string GetTempPath(string name) => Path.Combine(TempDir, name);
        
}