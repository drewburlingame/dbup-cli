using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests;

public class VariableSubstitutionTests
{
    private readonly TestHost host = new();

    private string GetConfigPath(string name) => ProjectPaths.GetConfigPath(name);

    [Fact]
    public void LoadMigration_ShouldLoadVariablesFromConfig()
    {
        var migration = ConfigLoader.LoadMigration(GetConfigPath("vars.yml"), host.Environment);

        migration.Vars.Should().HaveCount(3);
        migration.Vars.Should().ContainKey("Var1");
        migration.Vars.Should().ContainKey("Var2");
        migration.Vars.Should().ContainKey("Var_3-1");

        migration.Vars["Var1"].Should().Be("Var1Value");
        migration.Vars["Var2"].Should().Be("Var2Value");
        migration.Vars["Var_3-1"].Should().Be("Var3 Value");
    }

    [Fact]
    public void LoadMigration_ShouldReturnAnError_IfVarNameContainsInvalidChars()
    {
        /* According to https://dbup.readthedocs.io/en/latest/more-info/variable-substitution/:
         *
         * Variables can only contain letters, digits, _ and -.
         */
        var ex = Assert.Throws<InvalidVarNamesException>(() => 
            ConfigLoader.LoadMigration(GetConfigPath("invalid-vars.yml"), host.Environment));

        ex.Message.Should().Contain("Invalid-Var-Name$");
    }

    [Fact]
    public void LoadMigration_ShouldSubstituteVariablesToScript()
    {
        host.ToolEngine
            .Run("upgrade", GetConfigPath("vars.yml"))
            .ShouldSucceed();

        host.Logger.SummaryText().Should().Contain("print 'Var1Value'");
        host.Logger.SummaryText().Should().Contain("print 'Var2Value'");
        host.Logger.SummaryText().Should().Contain("print 'Var3 Value'");
    }

    [Fact]
    public void LoadMigration_ShouldNotSubstituteVariablesToScript()
    {
        host.ToolEngine
            .Run("upgrade", GetConfigPath("disable-vars.yml"))
            .ShouldSucceed();

        host.Logger.SummaryText().Should().Contain("print '$Var1$'");
        host.Logger.SummaryText().Should().Contain("print '$Var2$'");
        host.Logger.SummaryText().Should().Contain("print '$Var_3-1$'");
    }
}