using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DbUp.Cli.Configuration;

public static class ConfigLoader
{
    public static Migration LoadMigration(string configFilePath, IEnvironment environment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configFilePath);
        ArgumentNullException.ThrowIfNull(environment);
        
        var yml = environment.ReadFile(configFilePath);

        if (yml is null)
            throw new ConfigFileNotFoundException(configFilePath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        Migration migration;
        try
        {
            yml = StringUtils.ExpandEnvironmentVariables(yml);
            migration = deserializer.Deserialize<ConfigFile>(yml).DbUp;
        }
        catch (YamlException e)
        {
            throw new ConfigParsingException(e.Message, e);
        }

        Validator.ValidateObject(migration, new ValidationContext(migration));

        if (migration.Version != "1")
        {
            throw new UnsupportedConfigFileVersionException(migration.Version);
        }

        if (migration.Scripts.Count == 0)
        {
            migration.Scripts.Add(ScriptBatch.Default);
        }

        ValidateVarNames(migration.Vars);

        NormalizeScriptFolders(configFilePath, migration.Scripts);

        return migration;
    }

    private static void ValidateVarNames(Dictionary<string, string>? vars)
    {
        if (vars is null) return;

        Regex exp = new("^[a-z0-9_-]+$", RegexOptions.IgnoreCase);
        var invalidNames = vars.Keys.Where(n => !exp.IsMatch(n)).ToList();
        if (invalidNames.Count > 0)
        {
            throw new InvalidVarNamesException(invalidNames);
        }
    }

    private static void NormalizeScriptFolders(string configFilePath, IList<ScriptBatch> scripts)
    {
        foreach (var script in scripts)
        {
            var folder = ScriptProviderHelper.GetFolder(Path.Combine(configFilePath, ".."), script.Folder);
            script.Folder = new DirectoryInfo(folder).FullName;
        }
    }

    public static string GetDefaultConfigFile()
    {
        using var resourceStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(Constants.Default.ConfigFileResourceName);
        if (resourceStream is null)
            throw new Exception($"Missing default embedded resource: {Constants.Default.ConfigFileResourceName}");

        using var reader = new StreamReader(resourceStream);
        return reader.ReadToEnd();
    }
}