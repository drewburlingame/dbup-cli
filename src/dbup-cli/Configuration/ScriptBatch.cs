using System.Diagnostics.CodeAnalysis;

namespace DbUp.Cli.Configuration;

public class ScriptBatch
{
    internal static ScriptBatch Default => new();

    public string Folder { get; set; }
    public bool RunAlways { get; private set; }
    public bool SubFolders { get; private set; }
    public int Order { get; private set; } = Constants.Default.Order;   // Default value in DbUp
    public string Encoding { get; set; } = Constants.Default.Encoding;
    
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    public string Filter { get; private set; }
    public bool MatchFullPath { get; private set; }
    // ReSharper enable UnusedAutoPropertyAccessor.Local
    
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Yaml Serializer")]
    public ScriptBatch()
    {
    }

    public ScriptBatch(string folder, bool runAlways, bool subFolders, int order, string encoding)
    {
        Folder = folder;
        RunAlways = runAlways;
        SubFolders = subFolders;
        Order = order;
        Encoding = encoding;
    }
}