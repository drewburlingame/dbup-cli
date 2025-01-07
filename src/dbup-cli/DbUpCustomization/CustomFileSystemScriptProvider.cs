using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.ScriptProviders;

namespace DbUp.Cli.DbUpCustomization;

internal class CustomFileSystemScriptProvider : IScriptProvider
{
    private readonly string directoryPath;
    private readonly CustomFileSystemScriptOptions options;
    private readonly FileSystemScriptProvider scriptProvider;

    /// <summary>
    /// </summary>
    /// <param name="directoryPath">Path to SQL upgrade scripts</param>
    /// <param name="options">Different options for the file system script provider</param>
    /// <param name="sqlScriptOptions">The sql script options</param>        
    public CustomFileSystemScriptProvider(string directoryPath, CustomFileSystemScriptOptions options, SqlScriptOptions sqlScriptOptions)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        if (sqlScriptOptions == null)
            throw new ArgumentNullException(nameof(sqlScriptOptions));
        this.directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));

        scriptProvider = new FileSystemScriptProvider(directoryPath, options, sqlScriptOptions);
    }

    /// <summary>
    /// Gets all scripts that should be executed.
    /// </summary>
    public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
    {
        return scriptProvider.GetScripts(connectionManager).Select(x => new SqlScript(GetScriptName(directoryPath, x.Name), x.Contents, x.SqlScriptOptions));
    }

    private string GetScriptName(string basePath, string filename)
    {
        var prefixedFilename = filename;
        if (options.PrefixScriptNameWithBaseFolderName)
        {
            var dir = new DirectoryInfo(basePath);
            prefixedFilename = $"{dir.Name}.{filename}";
        }

        var prefix = options.Prefix?.Trim();
        if ( !string.IsNullOrEmpty(prefix) )
        {
            prefixedFilename = prefix + prefixedFilename;
        }

        return prefixedFilename;
    }
}