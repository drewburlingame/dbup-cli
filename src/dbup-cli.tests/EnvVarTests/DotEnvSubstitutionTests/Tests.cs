using DbUp.Cli.Configuration;
using FluentAssertions;

namespace DbUp.Cli.Tests.EnvVarTests.DotEnvSubstitutionTests;

public class Tests
{
    private static readonly string EnvVarsYmlPath = Caller.ConfigFile("env-vars.yml");
    private static readonly string DotEnvCurrentFolder = Caller.ConfigFile("DotEnv-CurrentFolder");
    
    [Fact]
    public void LoadMigration_ShouldSubstituteEnvVars_ToConnectionString()
    {
        const string connstr = "connection string";
        Environment.SetEnvironmentVariable(nameof(connstr), connstr);

        var migration = LoadMigration(EnvVarsYmlPath);
        migration.ConnectionString.Should().Be(connstr);
    }

    [Fact]
    public void LoadMigration_ShouldSubstituteEnvVars_ToFolders()
    {
        const string folder = "folder_name";
        Environment.SetEnvironmentVariable(nameof(folder), folder);

        var migration = LoadMigration(EnvVarsYmlPath);
        migration.Scripts[0].Folder.Should().EndWith(folder);
    }

    [Fact]
    public void LoadMigration_ShouldSubstituteEnvVars_ToVarValues()
    {
        const string var1 = "variable_value";
        Environment.SetEnvironmentVariable(nameof(var1), var1);

        var migration = LoadMigration(EnvVarsYmlPath);
        migration.Vars["Var1"].Should().Be(var1);
    }

    [Fact]
    public void LoadEnvironmentVariables_ShouldLoadDotEnv_FromCurrentFolder()
    {
        var dotEnvVarsPath = Caller.ConfigFile("dotenv-vars.yml");
        LoadEnvironmentVariables(DotEnvCurrentFolder, dotEnvVarsPath, []);

        var migration = LoadMigration(dotEnvVarsPath, DotEnvCurrentFolder);
        migration.Vars["VarA"].Should().Be("va1");
    }

    [Fact]
    public void LoadEnvironmentVariables_ShouldLoadDotEnv_FromConfigFileFolder()
    {
        var dotEnvVarsPath = Caller.ConfigFile("dotenv-vars.yml");
        LoadEnvironmentVariables(DotEnvCurrentFolder, dotEnvVarsPath, []);

        var migration = LoadMigration(dotEnvVarsPath, DotEnvCurrentFolder);
        migration.Vars["VarB"].Should().Be("vb2");
    }

    [Fact]
    public void LoadEnvironmentVariables_ShouldLoadVars_FromSpecifiedFiles()
    {
        var dotEnvVarsPath = Caller.ConfigFile("dotenv-vars.yml");
        LoadEnvironmentVariables(DotEnvCurrentFolder,
            dotEnvVarsPath,
            [
                Path.Combine("..", "varC.env"), // relative path
                Caller.ConfigFile("varD.env")
            ]);

        var migration = LoadMigration(dotEnvVarsPath, DotEnvCurrentFolder);
        migration.Vars["VarC"].Should().Be("vc3");
        migration.Vars["VarD"].Should().Be("vd3");
    }

    [Fact]
    public void LoadEnvironmentVariables_ShouldProperlyOverrideLoadedVariables()
    {
        // Loading order should be:
        // .env in the current folder
        // .env in the config file folder
        // all specified files in order of their appearance

        /*
         * The folder Config/DotEnv-VarsOverride contains prepared files to conduct this test. The idea is that each of the files contains
         * the same variables varA - varD, but each one overrides the different variables with the different values.
         *
         * The order of the files should be:
         *     1. CurrentFolder/.env
         *     2. ConfigFolder/.env
         *     3. file3.env
         *     4. file4.env
         *
         * The values of the variables are (dependent of a file):
         *
         *          1       2       3       4             The result
         *  varA   va1      -       -       -                 va1
         *  varB   vb1     vb2      -       -                 vb2
         *  varC   vc1     vc2     vc3      -                 vc3
         *  varD   vd1     vd2     vd3     vd4                vd4
         *
         *  The numbers in the headers of the rows are the file numbers as mentioned before. A dash means that the variable is absent in a file.
         *  The last column contains the right values of the variables after the overriding if it was correctly performed.
         */

        var currentDirectory = Caller.ConfigFile(["DotEnv-VarsOverride", "CurrentFolder"]);

        var dotEnvVarsPath = Caller.ConfigFile(["DotEnv-VarsOverride", "ConfigFolder", "dotenv-vars.yml"]);
        LoadEnvironmentVariables(currentDirectory,
            dotEnvVarsPath,
            [
                Path.Combine("..", "file3.env"),
                Path.Combine("..", "file4.env")
            ]);

        var migration = LoadMigration(dotEnvVarsPath, currentDirectory);
        migration.Vars["VarA"].Should().Be("va1");
        migration.Vars["VarB"].Should().Be("vb2");
        migration.Vars["VarC"].Should().Be("vc3");
        migration.Vars["VarD"].Should().Be("vd4");
    }

    private static Migration LoadMigration(string configPath, string currentDirectory = null)
    {
        return ConfigLoader.LoadMigration(configPath, new TestEnvironment(currentDirectory ?? Caller.Directory()));
    }
    
    private static void LoadEnvironmentVariables(string currentDirectory, string dotEnvVarsPath,
        List<string> envFiles)
    {
        var env = new TestEnvironment(currentDirectory);
        env.LoadDotEnvFiles(dotEnvVarsPath, envFiles);
    }
}