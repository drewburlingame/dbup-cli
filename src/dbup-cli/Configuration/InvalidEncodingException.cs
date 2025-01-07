namespace DbUp.Cli.Configuration;

public class InvalidEncodingException(string folder, Exception innerException) : 
    DbUpCliException($"Invalid encoding for scripts' folder '{folder}'", innerException)
{
}