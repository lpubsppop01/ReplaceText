using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace lpubsppop01.ReplaceText
{
    enum CommandRunnerActionKind
    {
        None,
        Replace,
        Genearte
    }

    class CommandRunner
    {
        IList<Command> commands;

        public CommandRunner(IList<Command> commands)
        {
            this.commands = commands;
        }

        public void Run(PathTree pathTree, CommandRunnerActionKind actionKind)
        {
            pathTree.Traverse((node) =>
            {
                if (!node.IsTarget) return;
                ReplaceName(actionKind, node);
                ReplaceContent(actionKind, node);
            });
        }

        void ReplaceName(CommandRunnerActionKind actionKind, PathTreeNode node)
        {
            string resultName = commands.Aggregate(node.Name, (n, c) => Regex.Replace(n, c.Pattern, c.Replacement));
            if (resultName == node.Name) return;

            string kindLabel = node.IsDirectory ? "Directory name" : "Filename";
            string prevPath = node.Path;
            node.Name = resultName;
            Console.WriteLine("{0}:", kindLabel);
            Console.WriteLine("  From: {0}", prevPath);
            Console.WriteLine("  To  : {0}", node.Path);
            if (actionKind == CommandRunnerActionKind.Replace)
            {
                if (node.IsDirectory)
                {
                    Directory.Move(node.OriginalPath, node.Path);
                }
                else
                {
                    File.Move(node.OriginalPath, node.Path);
                }
            }
            else if (actionKind == CommandRunnerActionKind.Genearte)
            {
                if (node.IsDirectory)
                {
                    Directory.CreateDirectory(node.Path);
                }
                else
                {
                    File.Copy(node.OriginalPath, node.Path);
                }
            }
        }

        void ReplaceContent(CommandRunnerActionKind actionKind, PathTreeNode node)
        {
            if (node.IsDirectory) return;

            bool replaced = false;
            string srcPath = actionKind == CommandRunnerActionKind.Replace ? node.Path : node.OriginalPath;
            (var srcEncoding, var srcNewLine) = EncodingDetector.Detect(srcPath);
            if (srcEncoding == null || srcNewLine == null) return;
            var lines = MyFileReader.ReadAllLines(srcPath, srcEncoding, srcNewLine);
            var destLines = new List<string>();
            for (int i = 0; i < lines.Count; ++i)
            {
                string resultLine = commands.Aggregate(lines[i], (l, c) => Regex.Replace(l, c.Pattern, c.Replacement));
                destLines.Add(resultLine);
                if (resultLine == lines[i]) continue;

                if (!replaced)
                {
                    Console.WriteLine("Content of \"{0}\":", node.Name);
                    replaced = true;
                }
                Console.WriteLine("  {0:0000}: {1}", i + 1, resultLine);
            }
            if (actionKind == CommandRunnerActionKind.Replace && replaced ||
                actionKind == CommandRunnerActionKind.Genearte)
            {
                using (var writer = new StreamWriter(node.Path, /* append: */ false, srcEncoding))
                {
                    for (int i = 0; i < destLines.Count; ++i)
                    {
                        writer.Write(destLines[i] + ((i != destLines.Count - 1) ? srcNewLine : ""));
                    }
                }
            }
        }
    }
}
