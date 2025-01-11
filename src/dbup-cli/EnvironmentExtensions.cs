using DbUp.Cli.Configuration;
using DotNetEnv;
using FileNotFoundException = DbUp.Cli.Configuration.FileNotFoundException;

namespace DbUp.Cli;

public static class EnvironmentExtensions
{
    public static string GetFilePath(this IEnvironment environment, string configFilePath, bool? fileShouldExist = null)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentException.ThrowIfNullOrWhiteSpace(configFilePath);

        var fullPath = new FileInfo(Path.IsPathRooted(configFilePath)
            ? configFilePath
            : Path.Combine(environment.CurrentDirectory, configFilePath)
        ).FullName;

        if (fileShouldExist.HasValue && environment.FileExists(fullPath) != fileShouldExist.Value)
        {
            if (fileShouldExist.Value)
                throw new FileNotFoundException(fullPath);
            throw new FileAlreadyExistsException(fullPath);
        }
        
        // consider returning the fullPath. Is there a reason not to?
        return fullPath;
    }
    
    public static void LoadDotEnvFiles(this IEnvironment environment, string configFilePath, IEnumerable<string> extraFiles)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentException.ThrowIfNullOrWhiteSpace(configFilePath);
        
        LoadDefaultsFrom(environment.CurrentDirectory);
        
        // .env file next to a dbup.yml
        var configFileDirectory = new FileInfo(configFilePath).DirectoryName!;
        if (configFileDirectory != environment.CurrentDirectory)
        {
            LoadDefaultsFrom(configFileDirectory);
        }

        foreach (var file in extraFiles)
        {
            Load(environment.GetFilePath(file, fileShouldExist: true));
        }

        return;

        void LoadDefaultsFrom(string directory)
        {
            LoadIfExists(directory, Constants.Default.DotEnvFileName);
            LoadIfExists(directory, Constants.Default.DotEnvLocalFileName);
        }
        
        void LoadIfExists(string folder, string file)
        {
            // TODO: load from stream instead of file to enable in-mem support
            var path = Path.Combine(folder, file);
            if (environment.FileExists(path))
            {
                Load(path);
            }
        }

        void Load(string envFile) => environment.LoadDotEnv(envFile);
    }
}