using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace lpubsppop01.ReplaceText
{
    class PathTreeNode
    {
        public PathTreeNode(string name, PathTreeNode parent, bool isDirectory)
        {
            Name = name;
            IsDirectory = isDirectory;
            Parent = parent;
            OriginalPath = Path;
        }

        public string Name { get; set; }
        public bool IsTarget { get; set; }
        public bool IsDirectory { get; private set; }
        public Dictionary<string, PathTreeNode> Children { get; private set; } = new Dictionary<string, PathTreeNode>();
        public PathTreeNode Parent { get; private set; }
        public string OriginalPath { get; private set; }

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
    }

    class PathTree
    {
        public PathTreeNode Root { get; private set; } = new PathTreeNode("", null, isDirectory: true);

        public bool HasValue
        {
            get { return Root.Children.Any(); }
        }

        public bool TryAdd(string path, out string errorMessage)
        {
            bool directoryExists = Directory.Exists(path);
            bool fileExists = File.Exists(path);
            if (!directoryExists && !fileExists)
            {
                errorMessage = "Not found: " + path;
                return false;
            }
            string absPath = Path.GetFullPath(path);
            var tokens = absPath.Split(Path.DirectorySeparatorChar);
            var curr = Root;
            while (tokens.Any())
            {
                var token = tokens.First();
                tokens = tokens.Skip(1).ToArray();
                if (token == "") continue;
                bool childIsLeaf = !tokens.Any();
                PathTreeNode child;
                if (!curr.Children.TryGetValue(token, out child))
                {
                    curr.Children[token] = child = new PathTreeNode(token, curr, isDirectory: !childIsLeaf || directoryExists);
                }
                curr = child;
                curr.IsTarget |= childIsLeaf;
            }
            if (curr.IsDirectory)
            {
                TraverseDirectory(curr, absPath);
            }
            errorMessage = "";
            return true;
        }

        static void TraverseDirectory(PathTreeNode directoryNode, string directoryPath)
        {
            void onChild(string childPath, bool childIsDirectory)
            {
                string childName = Path.GetFileName(childPath);
                PathTreeNode childNode;
                if (!directoryNode.Children.TryGetValue(childName, out childNode))
                {
                    directoryNode.Children[childName] = childNode = new PathTreeNode(childName, directoryNode, childIsDirectory);
                }
                childNode.IsTarget = true;
                if (childIsDirectory)
                {
                    TraverseDirectory(childNode, childPath);
                }
            }

            foreach (var childPath in Directory.EnumerateFiles(directoryPath))
            {
                onChild(childPath, childIsDirectory: false);
            }
            foreach (var childPath in Directory.EnumerateDirectories(directoryPath))
            {
                onChild(childPath, childIsDirectory: true);
            }
        }

        public PathTreeNode FindNode(string path)
        {
            string absPath = Path.GetFullPath(path);
            var tokens = absPath.Split(Path.DirectorySeparatorChar);
            var curr = Root;
            while (tokens.Any())
            {
                var token = tokens.First();
                tokens = tokens.Skip(1).ToArray();
                if (token == "") continue;
                PathTreeNode child;
                if (!curr.Children.TryGetValue(token, out child)) return null;
                curr = child;
            }
            return curr;
        }

        public void Traverse(Action<PathTreeNode> onNode)
        {
            Traverse(Root, onNode);
        }

        void Traverse(PathTreeNode node, Action<PathTreeNode> onNode)
        {
            foreach (var childNode in node.Children.Values)
            {
                onNode(childNode);
                Traverse(childNode, onNode);
            }
        }
    }
}
