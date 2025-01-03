using System.Text;

namespace DbUp.Cli;

/// <summary>
/// Environment implementation to use in cli tool
/// </summary>
public class CliEnvironment: IEnvironment
{
    public virtual bool DirectoryExists(string path) => Directory.Exists(path);
    public virtual bool FileExists(string path) => File.Exists(path);
    public virtual string GetCurrentDirectory() => Directory.GetCurrentDirectory();
    
    public virtual string ReadFile(string path) => File.ReadAllText(path, Encoding.UTF8);
    
    public virtual void WriteFile(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        if (File.Exists(path))
        {
            throw new FileAlreadyExistsException(path);
        }

        File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}