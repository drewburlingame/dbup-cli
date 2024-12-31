using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace DbUp.Cli.Tests.ConfigLoaderTests;

[TestClass]
public class LoadMigrationTests
{
    private const string MinYaml =
        """
        dbUp:
          version: 1
          provider: sqlserver
          connectionString: (localdb)\dbup;Initial Catalog=DbUpTest;Integrated Security=True
        """;

    private readonly TestHost host = new();

    [TestMethod]
    public void DefaultConfigFile_ShouldLoadItWithoutErrors()
    {
        var configFilePath = host.Environment.WriteFileInMem(ToolEngine.GetDefaultConfigFile()); 
        LoadMigration(configFilePath);
    }

    [TestMethod]
    public void WhenVersionOfConfigFileNotEqualsTo1_0_ShouldFail()
    {
        var error = LoadMigrationError(host.Environment.WriteFileInMem(
            """
            dbUp:
              version: 2
            """));
        error.Should().Be("The only supported version of a config file is '1'");
    }

    [TestMethod]
    public void MinVersionOfYml_ShouldSet_ValidProviderAndConnectionString_And_DefaultParameters()
    {
        var path = host.Environment.WriteFileInMem(MinYaml);
        var migration = LoadMigration(path);
        
        migration.Provider.Should().Be(Provider.SqlServer);
        migration.ConnectionString.Should().Be(@"(localdb)\dbup;Initial Catalog=DbUpTest;Integrated Security=True");

        migration.Transaction.Should().Be(Transaction.None);
        migration.ConnectionTimeoutSec.Should().Be(30);
        migration.DisableVars.Should().Be(false);

        migration.Scripts.Should().HaveCount(1);
        migration.Scripts[0].Encoding.Should().Be(Constants.Default.Encoding);
        migration.Scripts[0].Folder.Should().Be(Path.GetDirectoryName(path));
        migration.Scripts[0].Order.Should().Be(Constants.Default.Order);
        migration.Scripts[0].RunAlways.Should().BeFalse();
        migration.Scripts[0].SubFolders.Should().BeFalse();

        migration.Naming.UseOnlyFileName.Should().BeFalse();
        migration.Naming.IncludeBaseFolderName.Should().BeFalse();
        migration.Naming.Prefix.Should().BeNullOrEmpty();
    }

    [TestMethod]
    public void ShouldLoadAValidTimeout()
    {
        var migration = LoadMigrationFromYaml($"""
                                               {MinYaml}
                                                 connectionTimeoutSec: 45
                                               """);
        migration.ConnectionTimeoutSec.Should().Be(45);
    }

    [TestMethod]
    public void ShouldSetValidTransactionOptions()
    {
        var migration = LoadMigrationFromYaml($"""
                                               {MinYaml}
                                                 transaction: PerScript
                                               """);
        migration.Transaction.Should().Be(Transaction.PerScript);
    }

    [TestMethod]
    public void ShouldSetValidScriptOptions()
    {
        var path = host.Environment.WriteFileInMem($"""
                                                    {MinYaml}
                                                      scripts:
                                                        - folder: upgrades
                                                          subFolders: yes
                                                          order: 1
                                                          runAlways: no
                                                        - folder: views
                                                          subFolders: yes
                                                          order: 2
                                                          runAlways: yes
                                                    """);
        var x = LoadMigration(path);

        x.Scripts.Should().HaveCount(2);
        x.Scripts[0].Folder.Should().Be( Path.Combine(Path.GetDirectoryName(path)!, "upgrades"));
        x.Scripts[1].Folder.Should().Be(Path.Combine(Path.GetDirectoryName(path)!, "views"));

        x.Scripts[0].SubFolders.Should().BeTrue();
        x.Scripts[1].SubFolders.Should().BeTrue();

        x.Scripts[0].Order.Should().Be(1);
        x.Scripts[1].Order.Should().Be(2);

        x.Scripts[0].RunAlways.Should().BeFalse();
        x.Scripts[1].RunAlways.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldReturnNoneWithError_InCaseOfSyntacticError()
    {
        var error = LoadMigrationErrorFromYaml("""
                                                     dbUp:
                                                       version: 1
                                                       provider: sqlserver
                                                       connectionString: %ERR%
                                                     """);

        error.Should().Be("Configuration file error: While scanning for the next token, found character that cannot start any token.");
    }

    [TestMethod]
    public void ShouldNotThrow_IfNoVarsPresent()
    {
        var migration = LoadMigrationFromYaml($"""
                                               {MinYaml}
                                                 scripts:
                                                   -  folder: Vars
                                                 vars:
                                               """);
        migration.Vars.Should().BeEmpty();
    }

    [TestMethod]
    public void ShouldNotThrow_IfNoScriptsPresent()
    {
        // I'm not clear on what this it testing, but keeping it around since I'm likely missing something.
        LoadMigrationFromYaml($"""
                               {MinYaml}
                                 scripts:
                                   -  folder: NoScripts
                               """);
    }

    [TestMethod]
    public void ShouldSetValidNamingOptions()
    {
        var migration = LoadMigrationFromYaml($"""
                                               {MinYaml}
                                                 naming:
                                                  useOnlyFileName: yes
                                                  includeBaseFolderName: yes
                                                  prefix: scriptpreffix
                                               """);
        migration.Naming.UseOnlyFileName.Should().BeTrue();
        migration.Naming.IncludeBaseFolderName.Should().BeTrue();
        migration.Naming.Prefix.Should().Be("scriptpreffix");
    }

    [TestMethod]
    public void ShouldSetValidJournalToOptions()
    {
        var migration = LoadMigrationFromYaml($"""
                                               {MinYaml}
                                                 journalTo:
                                                   schema: "test-schema"
                                                   table: "test-table"
                                               """);
        migration.JournalTo.Should().NotBeNull();
        migration.JournalTo.Schema.Should().Be("test-schema");
        migration.JournalTo.Table.Should().Be("test-table");
    }

    [TestMethod]
    public void ShouldSetValidJournalToNull()
    {
        var migration = LoadMigrationFromYaml($"""
                                               {MinYaml}
                                                 journalTo: null
                                               """);
        migration.JournalTo.Should().BeNull();
    }

    [TestMethod]
    public void ShouldPrintReadableError_WhenProviderIsInvalid()
    {
        var error = LoadMigrationErrorFromYaml("""
                                               dbUp:
                                                 version: 1
                                                 provider: postgre1
                                               """);

        error.Should().Be("Configuration file error: Requested value 'postgre1' was not found. Exception during deserialization");
    }

    [TestMethod]
    public void ShouldPrintReadableError_WhenTransactionIsInvalid()
    {
        var error = LoadMigrationErrorFromYaml($"""
                                                {MinYaml}
                                                  transaction: WrongTransaction
                                                """);

        error.Should().Be("Configuration file error: Requested value 'WrongTransaction' was not found. Exception during deserialization");
    }

    private Migration LoadMigrationFromYaml(string yaml) => 
        LoadMigration(host.Environment.WriteFileInMem(yaml));

    private string LoadMigrationErrorFromYaml(string yaml) => 
        LoadMigrationError(host.Environment.WriteFileInMem(yaml));

    private Migration LoadMigration(string path)
    {
        var migrationOption = ConfigLoader.LoadMigration(path.Some<string, Error>(), host.Environment);
        if (migrationOption.HasValue)
        {
            return migrationOption.ValueOr(() => null);
        }
        
        migrationOption.MatchNone(err =>
        {
            Assert.Fail(err.Message);
        });
        return null;
    }
    
    private string LoadMigrationError(string path)
    {
        var migrationOption = ConfigLoader.LoadMigration(path.Some<string, Error>(), host.Environment);
        if (migrationOption.HasValue)
        {
            Assert.Fail("expected LoadMigration to fail");
        }

        return migrationOption.GetErrorOrThrow();
    }
}