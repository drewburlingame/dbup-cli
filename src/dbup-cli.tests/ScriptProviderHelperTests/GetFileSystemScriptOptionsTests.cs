using DbUp.Cli.Tests.TestInfrastructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbUp.Cli.Tests.ScriptProviderHelperTests;

[TestClass]
public class GetFileSystemScriptOptionsTests
{
    [TestMethod]
    public void WhenOptionIsSpecified_ShouldReturnValid_UseOnlyFilenameForScriptName_Option()
    {
        var namingOptions = new NamingOptions(useOnlyFileName: true, false, null);
        var options = GetFileSystemScriptOptions(namingOptions);
        options.UseOnlyFilenameForScriptName.Should().BeTrue();
    }

    [TestMethod]
    public void WhenOptionIsSpecified_ShouldReturnValid_PrefixScriptNameWithBaseFolderName_Option()
    {
        var namingOptions = new NamingOptions(false, includeBaseFolderName: true, null);
        var options = GetFileSystemScriptOptions(namingOptions);
        options.PrefixScriptNameWithBaseFolderName.Should().BeTrue();
    }

    [TestMethod]
    public void WhenOptionIsSpecified_ShouldReturnValid_Prefix_Option()
    {
        var namingOptions = new NamingOptions(false, false, "customprefix");
        var options = GetFileSystemScriptOptions(namingOptions);
        options.Prefix.Should().Be("customprefix");
    }
    
    [TestMethod]
    public void ShouldSetIncludeSubDirectoriesToFalse_IfSubFoldersIsSetToFalse()
    {
        var options = GetFileSystemScriptOptions(NamingOptions.Default, subFolders: false);
        options.IncludeSubDirectories.Should().BeFalse();
    }

    [TestMethod]
    public void ShouldSetIncludeSubDirectoriesToTrue_IfSubFoldersIsSetToTrue()
    {
        var options = GetFileSystemScriptOptions(NamingOptions.Default, subFolders: true);
        options.IncludeSubDirectories.Should().BeTrue();
    }

    private static CustomFileSystemScriptOptions GetFileSystemScriptOptions(NamingOptions namingOptions, bool subFolders = true)
    {
        var batch = new ScriptBatch("", true, subFolders: subFolders, 5, Constants.Default.Encoding);
        return ScriptProviderHelper.GetFileSystemScriptOptions(batch, namingOptions).GetValueOrThrow();
    }
}