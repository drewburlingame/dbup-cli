namespace DbUp.Cli;

public class FailedToConnectException(string message) : DbUpCliException(message);