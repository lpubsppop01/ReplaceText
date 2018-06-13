using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace lpubsppop01.ReplaceText
{
    enum TextReplacerActionKind
    {
        StandardOutput,
        Replace,
        Genearte
    }

    class TextReplacer
    {
        IList<Operator> ops;

        public TextReplacer(IList<Operator> ops)
        {
            this.ops = ops;
        }

        public void Replace(IList<string> paths, TextReplacerActionKind actionKind)
        {
            var root = BuildPathTree(paths);
            Replace(root, actionKind);
        }

        class PathTreeNode
        {
            public string Name = "";
            public bool IsTarget;
            public bool IsDirectory;
            public Dictionary<string, PathTreeNode> Children = new Dictionary<string, PathTreeNode>();
            public PathTreeNode Parent;

            public string Path
            {
                get
                {
                    var buf = new List<string>();
                    var curr = this;
                    while (curr != null && curr.Name != "")
                    {
                        buf.Insert(0, curr.Name);
                        curr = curr.Parent;
                    }
                    return string.Join(System.IO.Path.DirectorySeparatorChar, buf);
                }
            }

            public string OriginalPath;
        }

        static PathTreeNode BuildPathTree(IList<string> paths)
        {
            var root = new PathTreeNode();
            foreach (string path in paths)
            {
                bool isDirectory = Directory.Exists(path);
                bool isFile = File.Exists(path);
                if (!isDirectory && !isFile)
                {
                    Console.WriteLine("Not found: {0}", path);
                    continue;
                }
                var tokens = path.Split(Path.DirectorySeparatorChar);
                var curr = root;
                while (tokens.Any())
                {
                    var token = tokens.First();
                    tokens = tokens.Skip(1).ToArray();
                    if (token == "") continue;
                    PathTreeNode child;
                    if (!curr.Children.TryGetValue(token, out child))
                    {
                        curr.Children[token] = child = new PathTreeNode { Name = token };
                        child.Parent = curr;
                        child.OriginalPath = child.Path;
                    }
                    curr = child;
                    if (tokens.Any())
                    {
                        curr.IsDirectory = true;
                    }
                    else
                    {
                        curr.IsTarget = true;
                    }
                }
                if (isDirectory)
                {
                    curr.IsDirectory = true;
                    BuildPathTree_(curr, path);
                }
            }
            return root;
        }

        static void BuildPathTree_(PathTreeNode directoryNode, string directoryPath)
        {
            foreach (var childPath in Directory.EnumerateFiles(directoryPath))
            {
                string name = Path.GetFileName(childPath);
                PathTreeNode child;
                if (!directoryNode.Children.TryGetValue(name, out child))
                {
                    directoryNode.Children[name] = child = new PathTreeNode { Name = name };
                    child.Parent = directoryNode;
                    child.OriginalPath = child.Path;
                }
                child.IsTarget = true;
            }
            foreach (var childPath in Directory.EnumerateDirectories(directoryPath))
            {
                string name = Path.GetFileName(childPath);
                PathTreeNode child;
                if (!directoryNode.Children.TryGetValue(name, out child))
                {
                    directoryNode.Children[name] = child = new PathTreeNode { Name = name, IsDirectory = true };
                    child.Parent = directoryNode;
                    child.OriginalPath = child.Path;
                }
                child.IsTarget = true;
                BuildPathTree_(child, childPath);
            }
        }

        void Replace(PathTreeNode node, TextReplacerActionKind actionKind)
        {
            if (node.Name == "")
            {
                foreach (var child in node.Children.Values) Replace(child, actionKind);
            }
            else
            {
                if (node.IsTarget)
                {
                    {
                        string result = node.Name;
                        foreach (var op in ops)
                        {
                            result = Regex.Replace(result, op.SearchPattern, op.Replacement);
                        }
                        if (result != node.Name)
                        {
                            string kindLabel = node.IsDirectory ? "Directory name" : "Filename";
                            string origPath = node.Path;
                            node.Name = result;
                            Console.WriteLine("{0}:", kindLabel);
                            Console.WriteLine("  From: {0}", origPath);
                            Console.WriteLine("  To  : {0}", node.Path);
                            if (actionKind == TextReplacerActionKind.Replace)
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
                            else if (actionKind == TextReplacerActionKind.Genearte)
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
                    }
                    if (!node.IsDirectory)
                    {
                        bool replaced = false;
                        string srcPath = actionKind == TextReplacerActionKind.Replace ? node.Path : node.OriginalPath;
                        var lines = File.ReadAllLines(srcPath);
                        var destLines = new List<string>();
                        for (int i = 0; i < lines.Length; ++i)
                        {
                            string result = lines[i];
                            foreach (var op in ops)
                            {
                                result = Regex.Replace(result, op.SearchPattern, op.Replacement);
                            }
                            if (result != lines[i])
                            {
                                if (!replaced)
                                {
                                    Console.WriteLine("Content of \"{0}\":", node.Name);
                                    replaced = true;
                                }
                                Console.WriteLine("  {0:0000}: {1}", i + 1, result);
                            }
                            destLines.Add(result);
                        }
                        if (actionKind == TextReplacerActionKind.Replace && replaced ||
                            actionKind == TextReplacerActionKind.Genearte)
                        {
                            using (var writer = new StreamWriter(node.Path, /* append: */ false, new UTF8Encoding(false)))
                            {
                                foreach (var l in destLines)
                                {
                                    writer.WriteLine(l);
                                }
                            }
                        }
                    }
                }
                foreach (var child in node.Children.Values) Replace(child, actionKind);
            }
        }
    }
}
