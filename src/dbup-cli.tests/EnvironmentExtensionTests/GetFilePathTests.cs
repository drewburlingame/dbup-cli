using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;

namespace DbUp.Cli.Tests.EnvironmentExtensionTests;

public class GetFilePathTests
{
    private readonly TestHost host = new();
    private static readonly string AbsolutePath = new TestHost().EnsureTempDbUpYmlFileExists();

    public static TheoryData<string, string, string> FilePaths => new()
    {
        {"OnlyFileName", "dbup.yml", AbsolutePath},
        {"AbsolutePath", AbsolutePath, AbsolutePath},
        {"RelativePath", Path.Combine(".", "dbup.yml"), AbsolutePath},
        {"NonExistingPath", "missing.yml", ProjectPaths.GetTempPath("missing.yml")}
    };
    
    [Fact]
    public void ConfirmFileExists()
    {
        if (File.Exists(AbsolutePath)) return;
        Assert.Fail($"Missing file: {AbsolutePath}");
    }

    [Theory]
    [MemberData(nameof(FilePaths))]
    public void ShouldReturnAValidFileName_When_(string _, string path, string expected)
    {
        var configPath = host.Environment.GetFilePath(path);
        configPath.HasValue.Should().BeTrue();
        configPath.GetValueOrThrow().Should().Be(expected);
    }

    public static TheoryData<string, bool> FilesByExistence => new()
    {
        {"dbup.yml", true},
        {"missing.yml", false}
    };

    [Theory]
    [MemberData(nameof(FilesByExistence))]
    public void ShouldReturnNone_When_ExistingOrNonExistingDoesNotMatchExpectation(string filename, bool exists)
    {
        var configPath = host.Environment.GetFilePath(filename, fileShouldExist: !exists);
        configPath.HasValue.Should().BeFalse();
        configPath.GetErrorOrThrow().Should()
            .Be(exists 
                ? $"File already exists: {filename}" 
                : $"File is not found: {filename}");
    }
    
    [Theory]
    [MemberData(nameof(FilesByExistence))]
    public void ShouldReturnFilePath_When_ExistingOrNonExistingMatchesExpectation(string filename, bool exists)
    {
        var configPath = host.Environment.GetFilePath(filename, fileShouldExist: exists);
        configPath.HasValue.Should().BeTrue();
    }
}