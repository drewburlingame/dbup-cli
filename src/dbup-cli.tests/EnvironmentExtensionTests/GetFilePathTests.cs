using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.EnvironmentExtensionTests;

[TestClass]
public class GetFilePathTests
{
    private readonly TestHost host = new();
    private readonly string tempDbupYmlPath = ProjectPaths.GetTempPath("dbup.yml");

    [TestMethod]
    public void ShouldReturnFileFromTheCurrentDirectory_IfOnlyAFilenameSpecified()
    {
        var configPath = host.Environment.GetFilePath("dbup.yml");
        configPath.HasValue.Should().BeTrue();
        configPath.GetValueOrThrow().Should().Be(tempDbupYmlPath);
    }

    [TestMethod]
    public void ShouldReturnAValidFileName_IfAnAbsolutePathSpecified()
    {
        var configPath = host.Environment.GetFilePath(tempDbupYmlPath);
        configPath.HasValue.Should().BeTrue();
        configPath.GetValueOrThrow().Should().Be(tempDbupYmlPath);
    }

    [TestMethod]
    public void ShouldReturnAValidFileName_IfARelativePathSpecified()
    {
        var configPath = host.Environment.GetFilePath(Path.Combine(".", "dbup.yml"));
        configPath.HasValue.Should().BeTrue();
        configPath.GetValueOrThrow().Should().Be(tempDbupYmlPath);
    }

    [TestMethod]
    public void ShouldReturnAValidFileName_IfAFileDoesNotExist()
    {
        var configPath = host.Environment.GetFilePath("missing_dbup.yml");
        configPath.HasValue.Should().BeTrue();
        configPath.GetValueOrThrow().Should().Be(ProjectPaths.GetTempPath("missing_dbup.yml"));
    }

    [TestMethod]
    public void ShouldReturnNone_IfAFileShouldExistButDoesNot()
    {
        var configPath = host.Environment.GetFilePath("missing_dbup.yml", fileShouldExist: true);
        configPath.HasValue.Should().BeFalse();
        configPath.GetErrorOrThrow().Should().Be("File is not found: missing_dbup.yml");
    }

    [TestMethod]
    public void ShouldReturnNone_IfAFileShouldNotExistButDoes()
    {
        var configPath = host.Environment.GetFilePath("dbup.yml", fileShouldExist: false);
        if (configPath.HasValue)
        {
            var fileExists = host.Environment.FileExists("dbup.yml");
            Assert.Fail($"HasValue should be false. fileExists:{fileExists} configPath:{configPath.GetValueOrNull()}");
        }
        configPath.HasValue.Should().BeFalse();
        configPath.GetErrorOrThrow().Should().Be("File already exists: dbup.yml");
    }
}