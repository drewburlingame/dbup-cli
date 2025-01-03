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
        host.Environment.GetFilePath(path).Should().Be(expected);
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
        var ex = Assert.ThrowsAny<DbUpCliException>(() => host.Environment.GetFilePath(filename, fileShouldExist: !exists));
        if (exists)
        {
            ex.Should().BeOfType<FileAlreadyExistsException>();
            ex.Message.Should().StartWith("File already exists:");
            ex.Message.Should().EndWith(filename);
        }
        else
        {
            ex.Should().BeOfType<FileNotFoundException>();
            ex.Message.Should().StartWith("File is not found:");
            ex.Message.Should().EndWith(filename);
        }
    }
    
    [Theory]
    [MemberData(nameof(FilesByExistence))]
    public void ShouldReturnFilePath_When_ExistingOrNonExistingMatchesExpectation(string filename, bool exists)
    {
        var configPath = host.Environment.GetFilePath(filename, fileShouldExist: exists);
        configPath.Should().NotBeNull();
    }
}