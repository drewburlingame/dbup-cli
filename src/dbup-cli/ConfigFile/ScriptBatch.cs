﻿namespace DbUp.Cli;

public class ScriptBatch
{
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

    public string Folder { get; set; }
    public bool RunAlways { get; private set; }
    public bool SubFolders { get; private set; }
    public int Order { get; private set; } = Constants.Default.Order;   // Default value in DbUp
    public string Encoding { get; set; } = Constants.Default.Encoding;
    public string Filter { get; private set; }
    public bool MatchFullPath { get; private set; }

    public static ScriptBatch Default => new();
}