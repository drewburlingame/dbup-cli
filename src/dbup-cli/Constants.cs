namespace DbUp.Cli;

public static class Constants
{
    public static class Default
    {
        internal const string ConfigFileName = "dbup.yml";
        internal const string ConfigFileResourceName = "DbUp.Cli.DefaultOptions.dbup.yml";
        internal const string DotEnvFileName = ".env";
        internal const string DotEnvLocalFileName = ".env.local";
        public const string Encoding = "utf-8";
        public static int Order = 100;
    }

    internal static class ConsoleMessages
    {
        public static string InvalidEncoding => "Invalid encoding for scripts' folder '{0}': {1}";
    }
}

public class MissingScriptException() : DbUpCliException("At least one script should be present")
{
}

public class InvalidEncodingException(string folder, Exception innerException) : 
    DbUpCliException($"Invalid encoding for scripts' folder '{folder}'", innerException)
{
}