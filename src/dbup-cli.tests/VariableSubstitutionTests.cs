using DbUp.Cli.Tests.TestInfrastructure;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optional;

namespace DbUp.Cli.Tests;

[TestClass]
public class VariableSubstitutionTests
{
    private readonly TestHost host = new();

    private string GetConfigPath(string name) => ProjectPaths.GetConfigPath(name);

    [TestMethod]
    public void LoadMigration_ShouldLoadVariablesFromConfig()
    {
        var migrationOrNone = ConfigLoader.LoadMigration(GetConfigPath("vars.yml").Some<string, Error>());

        migrationOrNone.Match(
            some: migration =>
            {
                migration.Vars.Should().HaveCount(3);
                migration.Vars.Should().ContainKey("Var1");
                migration.Vars.Should().ContainKey("Var2");
                migration.Vars.Should().ContainKey("Var_3-1");

                migration.Vars["Var1"].Should().Be("Var1Value");
                migration.Vars["Var2"].Should().Be("Var2Value");
                migration.Vars["Var_3-1"].Should().Be("Var3 Value");
            },
            none: err => Assert.Fail(err.Message));
    }

    [TestMethod]
    public void LoadMigration_ShouldReturnAnError_IfVarNameContainsInvalidChars()
    {
        /* According to https://dbup.readthedocs.io/en/latest/more-info/variable-substitution/:
         *
         * Variables can only contain letters, digits, _ and -.
         */
        var migrationOrNone = ConfigLoader.LoadMigration(GetConfigPath("invalid-vars.yml").Some<string, Error>());

        migrationOrNone.MatchSome(
            migration =>
            {
                Assert.Fail("LoadMigration should fail if a var name contains one of the invalid chars");
            });
    }

    [TestMethod]
    public void LoadMigration_ShouldSubstituteVariablesToScript()
    {
        var env = A.Fake<IEnvironment>();
        A.CallTo(() => env.GetCurrentDirectory()).Returns(@"c:\test");
        A.CallTo(() => env.FileExists(A<string>.Ignored)).ReturnsLazily(x => File.Exists(x.Arguments[0] as string));

        host.ToolEngine
            .Run("upgrade", GetConfigPath("vars.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().Contain("print 'Var1Value'");
        host.Logger.Log.Should().Contain("print 'Var2Value'");
        host.Logger.Log.Should().Contain("print 'Var3 Value'");
    }

    [TestMethod]
    public void LoadMigration_ShouldNotSubstituteVariablesToScript()
    {
        host.ToolEngine
            .Run("upgrade", GetConfigPath("disable-vars.yml"))
            .ShouldSucceed();

        host.Logger.Log.Should().Contain("print '$Var1$'");
        host.Logger.Log.Should().Contain("print '$Var2$'");
        host.Logger.Log.Should().Contain("print '$Var_3-1$'");
    }
}