using Optional;

namespace DbUp.Cli;

/// <summary>
/// Interface of an environment to mock it in tests
/// </summary>
public interface IEnvironment
{
    string GetCurrentDirectory();
    bool FileExists(string path);
    bool DirectoryExists(string path);
    string ReadFile(string path);
    Option<bool, Error> WriteFile(string path, string content);
}