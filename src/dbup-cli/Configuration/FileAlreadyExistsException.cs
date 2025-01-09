namespace DbUp.Cli.Configuration;

public class FileAlreadyExistsException(string path)
    : DbUpCliException($"File already exists: {path}")
{
    public string Path { get; } = path;
}