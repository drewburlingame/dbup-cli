namespace DbUp.Cli;

public record Error(string Message)
{
    public static Error Create(string template, params object[] args) => new(string.Format(template, args));
}