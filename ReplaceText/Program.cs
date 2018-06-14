using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("lpubsppop01.ReplaceText.Tests")]

namespace lpubsppop01.ReplaceText
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = "ReplaceText";
            app.Description = "A CLI tool to replace part of file contents, file names and directory names.";
            app.HelpOption("-h|--help");
            var replaceOption = app.Option("-r|--replace", "Replace matched part", CommandOptionType.NoValue);
            var generateOption = app.Option("-g|--generate", "Generate new files that matched part is replaced", CommandOptionType.NoValue);
            var commandOrPathArg = app.Argument("command_or_path", "sed like command / target file or directory path", multipleValues: true);
            app.OnExecute(() =>
            {
                var commands = new List<Command>();
                var paths = new List<string>();
                var actionKind = TextReplacerActionKind.StandardOutput;
                commandOrPathArg.Values.ForEach((value) =>
                {
                    if (CommandParser.TryParse(value, out var command))
                    {
                        commands.Add(command);
                    }
                    else if (File.Exists(value) || Directory.Exists(value))
                    {
                        paths.Add(value);
                    }
                    else
                    {
                        Console.WriteLine("Error: Neither command nor valid path: " + value);
                    }
                });
                if (replaceOption.HasValue())
                {
                    actionKind = TextReplacerActionKind.Replace;
                }
                if (generateOption.HasValue())
                {
                    actionKind = TextReplacerActionKind.Genearte;
                }
                if (!commands.Any() || !paths.Any())
                {
                    Console.Write("Error: Command and path are required at least each one.");
                    app.ShowHelp();
                    return 1;
                }
                var replacer = new TextReplacer(commands);
                replacer.Replace(paths, actionKind);
                return 0;
            });
            app.Execute(args);
        }
    }
}
