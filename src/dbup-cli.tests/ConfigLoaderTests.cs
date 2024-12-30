using DbUp.Cli.Tests.TestInfrastructure;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace DbUp.Cli.Tests;

[TestClass]
public class ConfigLoaderTests
{
    private readonly TestHost host = new();
    private readonly string tempDbupYmlPath = ProjectPaths.GetTempPath("dbup.yml");
    private readonly string tempScriptsPath = ProjectPaths.GetTempPath("scripts");

    [TestMethod]
    public void LoadMigration_MinVersionOfYml_ShouldSetTheValidDefaultParameters()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("min.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            x.Transaction.Should().Be(Transaction.None);
            x.ConnectionTimeoutSec.Should().Be(30);
            x.DisableVars.Should().Be(false);

            x.Scripts.Should().HaveCount(1);
            x.Scripts[0].Encoding.Should().Be(Constants.Default.Encoding);
            x.Scripts[0].Folder.Should().Be(new FileInfo(ProjectPaths.GetConfigPath("min.yml")).Directory.FullName);
            x.Scripts[0].Order.Should().Be(Constants.Default.Order);
            x.Scripts[0].RunAlways.Should().BeFalse();
            x.Scripts[0].SubFolders.Should().BeFalse();

            x.Naming.UseOnlyFileName.Should().BeFalse();
            x.Naming.IncludeBaseFolderName.Should().BeFalse();
            x.Naming.Prefix.Should().BeNullOrEmpty();
        });
    }

    [TestMethod]
    public void LoadMigration_WhenLoadADefaultConfigFile_ShouldLoadItWithoutErrors()
    {
        var configFile = ToolEngine.GetDefaultConfigFile();
        var configFilePath = Path.GetTempFileName();
        File.WriteAllText(configFilePath, configFile);

        try
        {
            var migration = ConfigLoader.LoadMigration(configFilePath.Some<string, Error>());

            migration.MatchNone(err =>
            {
                Assert.Fail(err.Message);
            });
        }
        finally
        {
            File.Delete(configFilePath);
        }
    }

    [TestMethod]
    public void LoadMigration_ShouldLoadAValidTimeout()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("timeout.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            x.ConnectionTimeoutSec.Should().Be(45);
        });
    }

    [TestMethod]
    public void LoadMigration_WhenVersionOfConfigFileNotEqualsTo1_0_ShouldFail()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("wrongversion.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            Assert.Fail("At the time the '1' version only of config file should be supported");
        });
    }

    [TestMethod]
    public void LoadMigration_MinVersionOfYml_ShouldSetValidProviderAndConnectionString()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("min.yml").Some<string, Error>());

        migration.MatchSome(x => x.Provider.Should().Be(Provider.SqlServer));
        migration.MatchSome(x => x.ConnectionString.Should().Be(@"(localdb)\dbup;Initial Catalog=DbUpTest;Integrated Security=True"));
    }

    [TestMethod]
    public void LoadMigration_ShouldSetValidTransactionOptions()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("tran.yml").Some<string, Error>());

        migration.MatchSome(x => x.Transaction.Should().Be(Transaction.PerScript));
    }

    [TestMethod]
    public void LoadMigration_ShouldSetValidScriptOptions()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("script.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            x.Scripts.Should().HaveCount(2);
            x.Scripts[0].Folder.Should().Be( Path.Combine(new FileInfo(ProjectPaths.GetConfigPath("script.yml")).Directory.FullName, "upgrades"));
            x.Scripts[1].Folder.Should().Be(Path.Combine(new FileInfo(ProjectPaths.GetConfigPath("script.yml")).Directory.FullName, "views"));

            x.Scripts[0].SubFolders.Should().BeTrue();
            x.Scripts[1].SubFolders.Should().BeTrue();

            x.Scripts[0].Order.Should().Be(1);
            x.Scripts[1].Order.Should().Be(2);

            x.Scripts[0].RunAlways.Should().BeFalse();
            x.Scripts[1].RunAlways.Should().BeTrue();
        });
    }

    [TestMethod]
    public void LoadMigration_ShouldNotThrow_InCaseOfSyntacticError()
    {
        Action a = () => ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("syntax-error.yml").Some<string, Error>());

        a.Should().NotThrow();
    }

    [TestMethod]
    public void LoadMigration_ShouldReturnNoneWithError_InCaseOfSyntacticError()
    {
        var migrationOrNone = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("syntax-error.yml").Some<string, Error>());

        migrationOrNone.Match(
            some: m => Assert.Fail("Migration should not be loaded in case of syntactic error"),
            none: e => e.Should().NotBeNull());
    }

    [TestMethod]
    public void GetConfigFilePath_ShouldReturnFileFromTheCurrentDirectory_IfOnlyAFilenameSpecified()
    {
        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(ProjectPaths.TempDir);
        A.CallTo(() => env.FileExists(tempDbupYmlPath)).Returns(true);

        var configPath = ConfigLoader.GetFilePath(env, "dbup.yml");
        configPath.HasValue.Should().BeTrue();

        configPath.MatchSome(x => x.Should().Be(tempDbupYmlPath));
    }

    [TestMethod]
    public void GetConfigFilePath_ShouldReturnNone_IfAFileNotExists()
    {
        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(ProjectPaths.TempDir);
        A.CallTo(() => env.FileExists(tempDbupYmlPath)).Returns(false);

        var configPath = ConfigLoader.GetFilePath(env, "dbup.yml");
        configPath.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void GetConfigFilePath_ShouldReturnAValidFileName_IfARelativePathSpecified()
    {
        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(tempScriptsPath);
        A.CallTo(() => env.FileExists(tempDbupYmlPath)).Returns(true);

        var configPath = ConfigLoader.GetFilePath(env, Path.Combine("..", "dbup.yml"));
        configPath.HasValue.Should().BeTrue();

        configPath.MatchSome(x => x.Should().Be(tempDbupYmlPath));
    }

    [TestMethod]
    public void GetConfigFilePath_ShouldReturnAValidFileName_IfAnAbsolutePathSpecified()
    {
        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(ProjectPaths.TempDir);
        A.CallTo(() => env.FileExists(tempDbupYmlPath)).Returns(true);

        var configPath = ConfigLoader.GetFilePath(env, tempDbupYmlPath);
        configPath.HasValue.Should().BeTrue();

        configPath.MatchSome(x => x.Should().Be(tempDbupYmlPath));
    }

    [TestMethod]
    public void LoadMigration_ShouldNotThrow_IfNoVarsPresent()
    {
        Action a = () => ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("no-vars.yml").Some<string, Error>());

        a.Should().NotThrow();
    }

    [TestMethod]
    public void LoadMigration_ShouldNotThrow_IfNoScriptsPresent()
    {
        Action a = () => ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("no-scripts.yml").Some<string, Error>());

        a.Should().NotThrow();
    }

    [TestMethod]
    public void LoadMigration_ShouldRespectScriptEncoding()
    {
        host.ToolEngine
            .Run("upgrade", ProjectPaths.GetConfigPath("encoding.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().Contain("print 'Превед, медвед'");
    }

    [TestMethod]
    public void LoadMigration_ShouldSetValidNamingOptions()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("naming.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            x.Naming.UseOnlyFileName.Should().BeTrue();
            x.Naming.IncludeBaseFolderName.Should().BeTrue();
            x.Naming.Prefix.Should().Be("scriptpreffix");
        });
    }

    [TestMethod]
    public void LoadMigration_ShouldSetValidJournalToOptions()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("journalTo.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            x.JournalTo.Should().NotBeNull();
            x.JournalTo.Schema.Should().Be("test-schema");
            x.JournalTo.Table.Should().Be("test-table");
        });
    }

    [TestMethod]
    public void LoadMigration_ShouldSetValidJournalToNull()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("journalTo-null.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            x.JournalTo.Should().BeNull();
        });
    }

    [TestMethod]
    public void LoadMigration_ShouldPrintReadableError_WhenProviderIsInvalid()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("invalid-provider.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            Assert.Fail("Invalid provider in the configuration file should not be parsed");
        });
    }

    [TestMethod]
    public void LoadMigration_ShouldPrintReadableError_WhenTransactionIsInvalid()
    {
        var migration = ConfigLoader.LoadMigration(ProjectPaths.GetConfigPath("invalid-transaction.yml").Some<string, Error>());

        migration.MatchSome(x =>
        {
            Assert.Fail("Invalid transaction in the configuration file should not be parsed");
        });
    }
}