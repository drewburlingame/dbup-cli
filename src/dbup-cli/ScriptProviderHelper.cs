﻿using System.Text;
using System.Text.RegularExpressions;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Support;

namespace DbUp.Cli;

public static class ScriptProviderHelper
{
    public static string GetFolder(string basePath, string path) =>
        string.IsNullOrWhiteSpace(basePath)
            ? throw new ArgumentException("param can't be a null or whitespace", nameof(basePath))
            : string.IsNullOrWhiteSpace(path)
                ? basePath
                : Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(basePath, path);

    public static SqlScriptOptions GetSqlScriptOptions(ScriptBatch batch) =>
        new()
        {
            ScriptType = batch.RunAlways ? ScriptType.RunAlways : ScriptType.RunOnce,
            RunGroupOrder = batch.Order
        };

    public static CustomFileSystemScriptOptions GetFileSystemScriptOptions(ScriptBatch batch, NamingOptions naming)
    {
        ArgumentNullException.ThrowIfNull(batch);
        if (batch.Encoding == null)
            throw new ArgumentNullException(nameof(batch.Encoding), "Encoding can't be null");

        Encoding encoding = null;
        try
        {
            encoding = Encoding.GetEncoding(batch.Encoding);
        }
        catch
        {
        }
        
        if(encoding == null)
        {
            try
            {
                encoding = CodePagesEncodingProvider.Instance.GetEncoding(batch.Encoding);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidEncodingException(batch.Folder, ex);
            }
        }

        return new CustomFileSystemScriptOptions
        {
            IncludeSubDirectories = batch.SubFolders,
            Encoding = encoding,
            Filter = CreateFilter(batch.Filter, batch.MatchFullPath),
            UseOnlyFilenameForScriptName = naming.UseOnlyFileName,
            PrefixScriptNameWithBaseFolderName = naming.IncludeBaseFolderName,
            Prefix = naming.Prefix
        };
    }

    public static Func<string, bool> CreateFilter(string filterString, bool matchFullPath = false)
    {
        if( string.IsNullOrWhiteSpace(filterString))
        {
            return null;
        }

        filterString = filterString.Trim();

        if (filterString.StartsWith("/", StringComparison.Ordinal) && filterString.EndsWith("/", StringComparison.Ordinal) && filterString.Length >= 2)
        {
            // This is a regular expression

            if(filterString.Length == 2)
            {
                // equals to empty filter
                return s => true;
            }

            // We cannot use Trim('/') because we need to trim only one symbol on either side, but preserve all other symbols. 
            // For example, '//script//' -> should be '/script/', not 'script'
            filterString = filterString.Substring(1);
            filterString = filterString.Substring(0, filterString.Length - 1);

            filterString = $"^{filterString}$";
        }
        else
        {
            filterString = WildCardToRegular(filterString);
        }

        var regex = new Regex(filterString, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        return fullFilePath =>
        {
            var filename = matchFullPath ? fullFilePath : new FileInfo(fullFilePath).Name;
            var res = regex.IsMatch(filename);

            return res;
        };
    }

    private static string WildCardToRegular(string value)
    {
        return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
    }

    public static UpgradeEngineBuilder SelectScripts(this UpgradeEngineBuilder builder, IList<ScriptBatch> scripts, NamingOptions naming)
    {
        ArgumentNullException.ThrowIfNull(scripts);

        if (scripts.Count == 0)
        {
            throw new MissingScriptException();
        }

        foreach (var script in scripts)
        {
            if (!Directory.Exists(script.Folder))
            {
                throw new FolderNotFoundException(script.Folder);
            }
        }

        foreach (var script in scripts)
        {
            builder = builder.AddScripts(script, naming ?? NamingOptions.Default);
        }

        return builder;
    }

    private static UpgradeEngineBuilder AddScripts(this UpgradeEngineBuilder builder, ScriptBatch script, NamingOptions naming)
    {
        var options = GetFileSystemScriptOptions(script, naming);
        return builder.WithScripts(
            new CustomFileSystemScriptProvider(
                script.Folder,
                options,
                GetSqlScriptOptions(script)));
    }
}