namespace DbUp.Cli.Configuration;

public class FolderNotFoundException(string path)
    : DbUpCliException($"Folder is not found: {path}")
{
    public string Path { get; } = path;
}