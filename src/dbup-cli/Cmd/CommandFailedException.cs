namespace DbUp.Cli.Cmd;

internal class CommandFailedException(string message) : DbUpCliException(message);