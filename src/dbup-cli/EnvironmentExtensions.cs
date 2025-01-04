using DotNetEnv;

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
    
    public static void LoadEnvironmentVariables(this IEnvironment environment, string configFilePath, IEnumerable<string> envFiles)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentException.ThrowIfNullOrWhiteSpace(configFilePath);
        
        // .env file  in a current folder
        environment.Load(environment.CurrentDirectory, Constants.Default.DotEnvFileName);
        // .env.local file  in a current folder
        environment.Load(environment.CurrentDirectory, Constants.Default.DotEnvLocalFileName);

        var configFileDirectoryName = new FileInfo(configFilePath).DirectoryName!;
        
        // .env file next to a dbup.yml
        environment.Load(configFileDirectoryName, Constants.Default.DotEnvFileName);
        // .env.local file next to a dbup.yml
        environment.Load(configFileDirectoryName, Constants.Default.DotEnvLocalFileName);

        if (envFiles != null)
        {
            foreach (var file in envFiles)
            {
                Env.Load(environment.GetFilePath(file, fileShouldExist: true));
            }
        }
    }

    private static void Load(this IEnvironment environment, string folder, string file)
    {
        // TODO: load from stream instead of file to enable in-mem support
        var defaultEnvFile = Path.Combine(folder, file);
        if (environment.FileExists(defaultEnvFile))
        {
            Env.Load(defaultEnvFile);
        }
    }
}