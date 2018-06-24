using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Lpubsppop01.ReplaceText.Tests")]

namespace Lpubsppop01.ReplaceText
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
                var pathTree = new PathTree();
                var actionKind = CommandRunnerActionKind.None;
                commandOrPathArg.Values.ForEach((value) =>
                {
                    if (Command.TryParse(value, out var command))
                    {
                        commands.Add(command);
                    }
                    else if (!pathTree.TryAdd(value, out string errorMessage))
                    {
                        Console.WriteLine(errorMessage);
                    }
                });
                if (replaceOption.HasValue())
                {
                    actionKind = CommandRunnerActionKind.Replace;
                }
                if (generateOption.HasValue())
                {
                    actionKind = CommandRunnerActionKind.Genearte;
                }
                if (!commands.Any())
                {
                    Console.WriteLine("Error: Command is required at least one.");
                    app.ShowHelp();
                    return 1;
                }
                var runner = new CommandRunner(commands);
                if (pathTree.HasValue)
                {
                    runner.Run(pathTree, actionKind);
                }
                else
                {
                    using (var input = Console.OpenStandardInput())
                    {
                        using (var output = Console.OpenStandardOutput())
                        {
                            runner.Run(input, output);
                        }
                    }
                }
                return 0;
            });
            app.Execute(args);
        }
    }
}
