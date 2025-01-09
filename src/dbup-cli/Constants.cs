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
        public static readonly int Order = 100;
    }
}