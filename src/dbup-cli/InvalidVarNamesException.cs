namespace DbUp.Cli;

public class InvalidVarNamesException(List<string> names)
    : DbUpCliException("Found one or more variables with an invalid name. " +
                       "Variable name should contain only letters, digits, - and _." +
                       Environment.NewLine + "Check these names: " + string.Join(", ", names))
{
    public List<string> Names { get; } = names;
}