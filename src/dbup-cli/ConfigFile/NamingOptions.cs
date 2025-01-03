﻿namespace DbUp.Cli;

public class NamingOptions
{
    public bool UseOnlyFileName { get; private set; }
    public bool IncludeBaseFolderName { get; private set; }
    public string Prefix { get; private set; }

    public NamingOptions(bool useOnlyFileName, bool includeBaseFolderName, string prefix)
    {
        UseOnlyFileName = useOnlyFileName;
        IncludeBaseFolderName = includeBaseFolderName;
        Prefix = prefix;
    }

    public NamingOptions()
    {
    }

    public static NamingOptions Default => new();
}