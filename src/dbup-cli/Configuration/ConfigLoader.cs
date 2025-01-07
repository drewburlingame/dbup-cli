using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DbUp.Cli.Configuration;

public static class ConfigLoader
{
    // TODO: environment should eventually be required. Using optional param for now as incremental step to keep tests passing during refactor.
    // will require using IEnvironment to get environment variables, with a bonus that we can isolate changes within tests
     public static Migration LoadMigration(string configFilePath, IEnvironment environment = null)
     {
         var path = configFilePath;
         var yml = environment is null
             ? File.ReadAllText(path, Encoding.UTF8)
             : environment.ReadFile(path);

         if (yml is null)
             throw new ConfigFileNotFoundException(configFilePath);
         
         var deserializer = new DeserializerBuilder()
             .WithNamingConvention(CamelCaseNamingConvention.Instance)
             .Build();
         
         Migration migration;
         try
         {
             migration = deserializer.Deserialize<ConfigFile>(yml).DbUp;
         }
         catch (YamlException e)
         {
             throw new ConfigParsingException(e.Message, e);
         }

         if (migration.Version != "1")
         {
             throw new UnsupportedConfigFileVersionException(migration.Version);
         }

         migration.Scripts ??= new List<ScriptBatch>();

         if (migration.Scripts.Count == 0)
         {
             migration.Scripts.Add(ScriptBatch.Default);
         }

         migration.Vars ??= new Dictionary<string, string>();

         ValidateVarNames(migration.Vars);

         migration.ExpandVariables();

         NormalizeScriptFolders(path, migration.Scripts);

         return migration;
     }


     private static void ValidateVarNames(Dictionary<string, string> vars)
    {
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
            var dir = new DirectoryInfo(folder);
            folder = dir.FullName;

            script.Folder = folder;
        }
    }
    
    public static string GetDefaultConfigFile()
    {
        using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Constants.Default.ConfigFileResourceName);
        if (resourceStream is null) 
            throw new Exception($"Missing default embedded resource: {Constants.Default.ConfigFileResourceName}");
        
        using var reader = new StreamReader(resourceStream);
        return reader.ReadToEnd();
    }
}