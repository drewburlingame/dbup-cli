﻿using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.EnvironmentExtensionTests;

[TestClass]
public class GetFilePathTests
{
    private readonly TestHost host = new();
    private static readonly string AbsolutePath = new TestHost().EnsureTempDbUpYmlFileExists();
    
    public static IEnumerable<object[]> FilePaths
    {
        get
        {
            yield return ["OnlyFileName", "dbup.yml", AbsolutePath];
            yield return ["AbsolutePath", AbsolutePath, AbsolutePath];
            yield return ["RelativePath", Path.Combine(".", "dbup.yml"), AbsolutePath];
            yield return ["NonExistingPath", "missing.yml", ProjectPaths.GetTempPath("missing.yml")];
        }
    }
    
    [TestMethod]
    public void ConfirmFileExists()
    {
        if (File.Exists(AbsolutePath)) return;
        Assert.Fail($"Missing file: {AbsolutePath}");
    }

    [DataTestMethod]
    [DynamicData(nameof(FilePaths))]
    public void ShouldReturnAValidFileName_When_(string type, string path, string expected)
    {
        var configPath = host.Environment.GetFilePath(path);
        configPath.HasValue.Should().BeTrue();
        configPath.GetValueOrThrow().Should().Be(expected);
    }


    public static IEnumerable<object[]> FilesByExistence
    {
        get
        {
            yield return ["dbup.yml", true];
            yield return ["missing.yml", false];
        }
    }

    [DataTestMethod]
    [DynamicData(nameof(FilesByExistence))]
    public void ShouldReturnNone_When_ExistingOrNonExistingDoesNotMatchExpectation(string filename, bool exists)
    {
        var configPath = host.Environment.GetFilePath(filename, fileShouldExist: !exists);
        configPath.HasValue.Should().BeFalse();
        configPath.GetErrorOrThrow().Should()
            .Be(exists 
                ? $"File already exists: {filename}" 
                : $"File is not found: {filename}");
    }
    
    [DataTestMethod]
    [DynamicData(nameof(FilesByExistence))]
    public void ShouldReturnFilePath_When_ExistingOrNonExistingMatchesExpectation(string filename, bool exists)
    {
        var configPath = host.Environment.GetFilePath(filename, fileShouldExist: exists);
        configPath.HasValue.Should().BeTrue();
    }
}