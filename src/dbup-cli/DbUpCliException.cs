namespace DbUp.Cli;

public abstract class DbUpCliException : Exception
{
    protected DbUpCliException(string message) : base(message)
    {
    }

    protected DbUpCliException(string message, Exception innerException) : base(message, innerException)
    {
    }
}