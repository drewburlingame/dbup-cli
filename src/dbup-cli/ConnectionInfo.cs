namespace DbUp.Cli;

public record ConnectionInfo(string ConnectionString, int ConnectionTimeoutSec)
{
    public TimeSpan Timeout = TimeSpan.FromSeconds(ConnectionTimeoutSec);
}