using JetBrains.Annotations;

namespace DbUp.Cli.Configuration;

[UsedImplicitly]
public class Migration
{
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    public string Version { get; private set; }
    public Provider Provider { get; private set; }
    // ReSharper enable UnusedAutoPropertyAccessor.Local
    public string ConnectionString { get; private set; }
    public int ConnectionTimeoutSec { get; private set; } = 30;
    public bool DisableVars { get; private set; } = false;
    public Transaction Transaction { get; private set; } = Transaction.None;
    public Journal JournalTo { get; private set; } = Journal.Default;
    public NamingOptions Naming { get; private set; } = NamingOptions.Default;
    public List<ScriptBatch> Scripts { get; set; } = [];

    public Dictionary<string, string> Vars { get; set; } = new();

    internal void ExpandVariables()
    {
        ConnectionString = StringUtils.ExpandEnvironmentVariables(ConnectionString ?? "");
        Scripts.ForEach(x => x.Folder = StringUtils.ExpandEnvironmentVariables(x.Folder ?? ""));

        var dic = new Dictionary<string, string>();
        foreach (var item in Vars)
        {
            dic.Add(item.Key, StringUtils.ExpandEnvironmentVariables(item.Value ?? ""));
        }

        Vars = dic;
    }
}