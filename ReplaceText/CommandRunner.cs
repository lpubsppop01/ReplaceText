using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lpubsppop01.ReplaceText
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
        bool messageIsDisabled;

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

        public void Run(Stream input, Stream output)
        {
            string tempFilePath = Path.GetTempFileName();
            messageIsDisabled = true;
            try
            {
                using (var tempOutput = new FileStream(tempFilePath, FileMode.Open))
                {
                    input.CopyTo(tempOutput);
                }

                var pathTree = new PathTree();
                pathTree.TryAdd(tempFilePath, out string errorMessage);
                Run(pathTree, CommandRunnerActionKind.Replace);

                using (var tempInput = new FileStream(tempFilePath, FileMode.Open))
                {
                    tempInput.CopyTo(output);
                }
            }
            finally
            {
                messageIsDisabled = false;
                File.Delete(tempFilePath);
            }
        }

        void Console_WriteLine(string format, params object[] args)
        {
            if (messageIsDisabled) return;
            Console.WriteLine(format, args);
        }

        void ReplaceName(CommandRunnerActionKind actionKind, PathTreeNode node)
        {
            string resultName = commands.Aggregate(node.Name, (n, c) => Regex.Replace(n, c.Pattern, c.Replacement));
            if (resultName == node.Name) return;

            string kindLabel = node.IsDirectory ? "Directory name" : "Filename";
            string prevPath = node.Path;
            node.Name = resultName;
            Console_WriteLine("{0}:", kindLabel);
            Console_WriteLine("  From: {0}", prevPath);
            Console_WriteLine("  To  : {0}", node.Path);
            if (actionKind == CommandRunnerActionKind.Replace)
            {
                if (node.IsDirectory)
                {
                    Directory.Move(prevPath, node.Path);
                }
                else
                {
                    File.Move(prevPath, node.Path);
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
            var srcInfo = new TextFileInfo(srcPath);
            if (!srcInfo.IsValid) return;
            var lines = srcInfo.ReadAllLines();
            var destLines = new List<string>();
            for (int i = 0; i < lines.Count; ++i)
            {
                string resultLine = commands.Aggregate(lines[i], (l, c) => Regex.Replace(l, c.Pattern, c.Replacement));
                destLines.Add(resultLine);
                if (resultLine == lines[i]) continue;

                if (!replaced)
                {
                    Console_WriteLine("Content of \"{0}\":", node.Name);
                    replaced = true;
                }
                Console_WriteLine("  {0:0000}: {1}", i + 1, resultLine);
            }
            if (actionKind == CommandRunnerActionKind.Replace && replaced ||
                actionKind == CommandRunnerActionKind.Genearte)
            {
                using (var writer = new StreamWriter(node.Path, /* append: */ false, srcInfo.Encoding))
                {
                    for (int i = 0; i < destLines.Count; ++i)
                    {
                        writer.Write(destLines[i] + ((i != destLines.Count - 1) ? srcInfo.NewLine : ""));
                    }
                }
            }
        }
    }
}
