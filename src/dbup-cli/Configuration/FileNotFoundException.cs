namespace DbUp.Cli.Configuration;

public class FileNotFoundException(string path)
    : DbUpCliException($"File is not found: {path}")
{
    public string Path { get; } = path;
}