namespace DbUp.Cli;

public class MissingScriptException() : DbUpCliException("At least one script should be present")
{
}