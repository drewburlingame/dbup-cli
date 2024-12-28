using DbUp.Cli.Tests.TestInfrastructure;
using DbUp.Engine.Transactions;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace DbUp.Cli.Tests;

[TestClass]
public class EnvVariableSubstitutionTests
{
    private static readonly string EnvVarsYmlPath = ProjectPaths.GetConfigPath("env-vars.yml");
    private static readonly string DotEnvCurrentFolder = ProjectPaths.GetConfigPath("DotEnv-CurrentFolder");
    private readonly CaptureLogsLogger Logger;
    private readonly DelegateConnectionFactory testConnectionFactory;
    private readonly RecordingDbConnection recordingConnection;

    public EnvVariableSubstitutionTests()
    {
        Logger = new CaptureLogsLogger();
        recordingConnection = new RecordingDbConnection(Logger, "SchemaVersions");
        testConnectionFactory = new DelegateConnectionFactory(_ => recordingConnection);
    }

    [TestMethod]
    public void LoadMigration_ShouldSubstituteEnvVars_ToConnectionString()
    {
        const string connstr = "connection string";
        Environment.SetEnvironmentVariable(nameof(connstr), connstr);

        var migrationOrNone = ConfigLoader.LoadMigration(EnvVarsYmlPath.Some<string, Error>());

        migrationOrNone.Match(
            some: migration =>
            {
                migration.ConnectionString.Should().Be(connstr);
            },
            none: err => Assert.Fail(err.Message));
    }

    [TestMethod]
    public void LoadMigration_ShouldSubstituteEnvVars_ToFolders()
    {
        const string folder = "folder_name";
        Environment.SetEnvironmentVariable(nameof(folder), folder);

        var migrationOrNone = ConfigLoader.LoadMigration(EnvVarsYmlPath.Some<string, Error>());

        migrationOrNone.Match(
            some: migration =>
            {
                migration.Scripts[0].Folder.Should().EndWith(folder);
            },
            none: err => Assert.Fail(err.Message));
    }

    [TestMethod]
    public void LoadMigration_ShouldSubstituteEnvVars_ToVarValues()
    {
        const string var1 = "variable_value";
        Environment.SetEnvironmentVariable(nameof(var1), var1);

        var migrationOrNone = ConfigLoader.LoadMigration(EnvVarsYmlPath.Some<string, Error>());

        migrationOrNone.Match(
            some: migration =>
            {
                migration.Vars["Var1"].Should().Be(var1);
            },
            none: err => Assert.Fail(err.Message));
    }

    [TestMethod]
    public void LoadEnvironmentVariables_ShouldLoadDotEnv_FromCurrentFolder()
    {
        const string varA = "va1";

        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(DotEnvCurrentFolder);
        A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => File.Exists(x.Arguments[0] as string));
            
        var dotEnvVarsPath = ProjectPaths.GetConfigPath("dotenv-vars.yml");
            
        ConfigurationHelper.LoadEnvironmentVariables(env, dotEnvVarsPath, new List<string>())
            .MatchNone(error => Assert.Fail(error.Message));

        var migrationOrNone = ConfigLoader.LoadMigration(dotEnvVarsPath.Some<string, Error>());

        migrationOrNone.Match(
            some: migration =>
            {
                migration.Vars["VarA"].Should().Be(varA);
            },
            none: err => Assert.Fail(err.Message));
    }

    [TestMethod]
    public void LoadEnvironmentVariables_ShouldLoadDotEnv_FromConfigFileFolder()
    {
        const string varB = "vb2";

        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(DotEnvCurrentFolder);
        A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => File.Exists(x.Arguments[0] as string));

        var dotEnvVarsPath = ProjectPaths.GetConfigPath("dotenv-vars.yml");
            
        ConfigurationHelper.LoadEnvironmentVariables(env, dotEnvVarsPath, new List<string>())
            .MatchNone(error => Assert.Fail(error.Message));

        var migrationOrNone = ConfigLoader.LoadMigration(dotEnvVarsPath.Some<string, Error>());

        migrationOrNone.Match(
            some: migration =>
            {
                migration.Vars["VarB"].Should().Be(varB);
            },
            none: err => Assert.Fail(err.Message));
    }

    [TestMethod]
    public void LoadEnvironmentVariables_ShouldLoadVars_FromSpecifiedFiles()
    {
        const string varC = "vc3";
        const string varD = "vd3";

        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(DotEnvCurrentFolder);
        A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => File.Exists(x.Arguments[0] as string));

        var dotEnvVarsPath = ProjectPaths.GetConfigPath("dotenv-vars.yml");
            
        ConfigurationHelper.LoadEnvironmentVariables(env, dotEnvVarsPath, new List<string>
            {
                Path.Combine("..", "varC.env"),   // relative path
                ProjectPaths.GetConfigPath("varD.env")   // absolute path
            })
            .MatchNone(error => Assert.Fail(error.Message));

        var migrationOrNone = ConfigLoader.LoadMigration(dotEnvVarsPath.Some<string, Error>());

        migrationOrNone.Match(
            some: migration =>
            {
                migration.Vars["VarC"].Should().Be(varC);
                migration.Vars["VarD"].Should().Be(varD);
            },
            none: err => Assert.Fail(err.Message));
    }

    [TestMethod]
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

        const string varA = "va1";
        const string varB = "vb2";
        const string varC = "vc3";
        const string varD = "vd4";

        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(ProjectPaths.GetConfigPath("DotEnv-VarsOverride", "CurrentFolder"));
        A.CallTo(() => env.FileExists("")).WithAnyArguments().ReturnsLazily(x => File.Exists(x.Arguments[0] as string));
            
        var dotEnvVarsPath = ProjectPaths.GetConfigPath("DotEnv-VarsOverride", "ConfigFolder", "dotenv-vars.yml");
            
        ConfigurationHelper.LoadEnvironmentVariables(env, dotEnvVarsPath, new List<string>
            {
                Path.Combine("..", "file3.env"),
                Path.Combine("..", "file4.env")
            })
            .MatchNone(error => Assert.Fail(error.Message));

        var migrationOrNone = ConfigLoader.LoadMigration(dotEnvVarsPath.Some<string, Error>());

        migrationOrNone.Match(
            some: migration =>
            {
                migration.Vars["VarA"].Should().Be(varA);
                migration.Vars["VarB"].Should().Be(varB);
                migration.Vars["VarC"].Should().Be(varC);
                migration.Vars["VarD"].Should().Be(varD);
            },
            none: err => Assert.Fail(err.Message));
    }
}