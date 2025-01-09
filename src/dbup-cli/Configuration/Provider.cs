// ReSharper disable InconsistentNaming
// won't change for backward compatibility
namespace DbUp.Cli.Configuration;

public enum Provider
{
    UnsupportedProvider,
    SqlServer,
    PostgreSQL,
    MySQL,
    AzureSql
}