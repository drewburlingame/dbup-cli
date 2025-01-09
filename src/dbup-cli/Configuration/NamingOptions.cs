using System.Diagnostics.CodeAnalysis;

namespace DbUp.Cli.Configuration;

public class NamingOptions
{
    public static NamingOptions Default => new();
    
    public bool UseOnlyFileName { get; private set; }
    public bool IncludeBaseFolderName { get; private set; }
    public string? Prefix { get; private set; }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Yaml Serializer")]
    public NamingOptions()
    {
    }

    public NamingOptions(bool useOnlyFileName, bool includeBaseFolderName, string prefix)
    {
        UseOnlyFileName = useOnlyFileName;
        IncludeBaseFolderName = includeBaseFolderName;
        Prefix = prefix;
    }
}