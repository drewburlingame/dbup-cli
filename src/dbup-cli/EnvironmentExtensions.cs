using DotNetEnv;
using Optional;

namespace DbUp.Cli;

public static class EnvironmentExtensions
{
    public static Option<string, Error> GetFilePath(this IEnvironment environment, string configFilePath, bool? fileShouldExist = null)
    {
        if (environment == null)
            throw new ArgumentNullException(nameof(environment));
        if (string.IsNullOrWhiteSpace(configFilePath))
            throw new ArgumentException("Parameter can't be null or white space", nameof(configFilePath));

        var fullPath = new FileInfo(Path.IsPathRooted(configFilePath)
            ? configFilePath
            : Path.Combine(environment.GetCurrentDirectory(), configFilePath)
        ).FullName;

        string error = !fileShouldExist.HasValue || environment.FileExists(fullPath) == fileShouldExist.Value
            ? null
            : fileShouldExist.Value
                ? Constants.ConsoleMessages.FileNotFound
                : Constants.ConsoleMessages.FileAlreadyExists;
        
        // consider returning the fullPath. Is there a reason not to?
        return fullPath.SomeWhen(x => error == null, () => Error.Create(error, configFilePath));
    }
    
    public static Option<bool, Error> LoadEnvironmentVariables(this IEnvironment environment, string configFilePath, IEnumerable<string> envFiles)
    {
        if (environment == null)
            throw new ArgumentNullException(nameof(environment));
        if (configFilePath == null)
            throw new ArgumentNullException(nameof(configFilePath));
        
        // .env file  in a current folder
        environment.Load(environment.GetCurrentDirectory(), Constants.Default.DotEnvFileName);
        // .env.local file  in a current folder
        environment.Load(environment.GetCurrentDirectory(), Constants.Default.DotEnvLocalFileName);

        var configFileDirectoryName = new FileInfo(configFilePath).DirectoryName!;
        
        // .env file next to a dbup.yml
        environment.Load(configFileDirectoryName, Constants.Default.DotEnvFileName);
        // .env.local file next to a dbup.yml
        environment.Load(configFileDirectoryName, Constants.Default.DotEnvLocalFileName);

        if (envFiles != null)
        {
            foreach (var file in envFiles)
            {
                Error error = null;
                environment.GetFilePath(file, fileShouldExist: true)
                    .Match(
                        some: path => Env.Load(path),
                        none: err => error = err);

                if (error != null)
                {
                    return Option.None<bool, Error>(error);
                }
            }
        }

        return true.Some<bool, Error>();
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