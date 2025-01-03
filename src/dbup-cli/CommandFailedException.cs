namespace DbUp.Cli;

public class CommandFailedException(string message) : DbUpCliException(message);