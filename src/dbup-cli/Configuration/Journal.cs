﻿using System.Diagnostics.CodeAnalysis;

namespace DbUp.Cli.Configuration;

public class Journal
{
    public string? Schema { get; private set; }
    public string? Table { get; private set; }

    public static Journal Default => new();
    public bool IsDefault => Schema == null && Table == null;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Yaml Serializer")]
    public Journal()
    {
    }

    public Journal(string schema, string table)
    {
        Schema = schema;
        Table = table;
    }
}