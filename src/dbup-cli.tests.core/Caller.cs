using System.Runtime.CompilerServices;

namespace DbUp.Cli.Tests;

public static class Caller
{
    public static string Directory([CallerFilePath] string filepath = null) => Path.GetDirectoryName(filepath);
    
    public static string ConfigFile(string file = "dbup.yml", [CallerFilePath] string filepath = null) => 
        Path.Join(Path.GetDirectoryName(filepath), file);

    public static string ConfigFile(string[] names, [CallerFilePath] string filepath = null) =>
        Path.Combine(names.Prepend(Path.GetDirectoryName(filepath)).ToArray());
}