﻿using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DbUp.Cli
{
    public static class ConfigLoader
    {
        public static Option<string, Error> GetConfigFilePath(IEnvironment environment, string configFilePath, bool fileShouldExist = true)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));
            if (string.IsNullOrWhiteSpace(configFilePath))
                throw new ArgumentException("Parameter can't be null or white space", nameof(configFilePath));

            return new FileInfo(Path.IsPathFullyQualified(configFilePath)
                ? configFilePath
                : Path.Combine(environment.GetCurrentDirectory(), configFilePath)
            ).FullName.SomeWhen<string, Error>(x =>
                !fileShouldExist || (fileShouldExist && environment.FileExists(x)),
                Error.Create(Constants.ConsoleMessages.FileNotFound, configFilePath)
            );
        }

        public static Option<Migration, Error> LoadMigration(Option<string, Error> configFilePath) =>
            configFilePath.Match(
                some: path =>
                {
                    var input = new StringReader(File.ReadAllText(path, Encoding.UTF8));

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(new CamelCaseNamingConvention())
                        .Build();

                    Migration migration = null;

                    try
                    {
                        migration = deserializer.Deserialize<ConfigFile>(input).DbUp;
                    }
                    catch (SyntaxErrorException ex)
                    {
                        return Option.None<Migration, Error>(Error.Create(Constants.ConsoleMessages.ParsingError, ex.Message));
                    }

                    if (migration.Scripts.Count == 0)
                    {
                        migration.Scripts.Add(ScriptBatch.Default);
                    }

                    // TODO: all script folders should exist
                    NormalizeScriptFolders(path, migration.Scripts);

                    if( !ValidateVarNames(migration.Vars, out var errMessage) )
                    {
                        return Option.None<Migration, Error>(Error.Create(errMessage));
                    }

                    return migration.Some<Migration, Error>();
                },
                none: error => Option.None<Migration, Error>(error));

        readonly static Regex exp = new Regex("^[a-z0-9_-]+$", RegexOptions.IgnoreCase);

        static bool ValidateVarNames(Dictionary<string, string> vars, out string errMessage)
        {
            errMessage = null;
            foreach (var n in vars.Keys)
            {
                if( !exp.IsMatch(n) )
                {
                    if( errMessage == null )
                    {
                        errMessage = "Found one or more variables with an invalid name. Variable name should contain only letters, digits, - and _.";
                        errMessage += Environment.NewLine;
                        errMessage += "Check these names:";
                    }
                    errMessage += "    " + n;
                }
            }

            return errMessage == null;
        }

        private static void NormalizeScriptFolders(string configFilePath, IList<ScriptBatch> scripts)
        {
            // TODO: Check whether the folder exists
            foreach (var script in scripts)
            {
                var folder = ScriptProviderHelper.GetFolder(Path.Combine(configFilePath, ".."), script.Folder);
                var dir = new DirectoryInfo(folder);
                folder = dir.FullName;

                script.Folder = folder;
            }
        }
    }
}
