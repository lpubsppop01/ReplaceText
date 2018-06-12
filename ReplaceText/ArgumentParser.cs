using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lpubsppop01.ReplaceText
{
    static class ArgumentParser
    {
        public static ((string from, string to)[], string[] paths,  TextReplacerActionKind kind)? Parse(string[] commandLineArgs)
        {
            var programName = commandLineArgs[0];
            var args = commandLineArgs.Skip(1).ToArray();
            if (!args.Any())
            {
                ShowUsage(programName);
                return null;
            }
            var remainedArgs = args.ToList();
            var ops = new List<(string from, string to)>();
            for (int i = 0; i < remainedArgs.Count;) {
                var op = ParseOperationText(remainedArgs[i]);
                if (op != null)
                {
                    ops.Add(op.Value);
                    remainedArgs.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
            if (!ops.Any())
            {
                ShowUsage(programName);
                return null;
            }
            TextReplacerActionKind action = TextReplacerActionKind.StandardOutput;
            for (int i = 0; i<remainedArgs.Count;)
            {
                if(remainedArgs[i] == "-replace")
                {
                    action = TextReplacerActionKind.Replace;
                    remainedArgs.RemoveAt(i);
                } else if (remainedArgs[i] == "-generate") {
                    action = TextReplacerActionKind.Genearte;
                    remainedArgs.RemoveAt(i);
                }else
                {
                    ++i;
                }
            }
            return (ops.ToArray(), remainedArgs.ToArray(), action);
        }

        static void ShowUsage(string programName)
        {
            Console.WriteLine("Usage: {0} s/FROM/TO/g... FILE|DIRECTORY... [-replace|-generate]");
        }

        static (string from, string to)? ParseOperationText(string opText)
        {
            var tokens = SplitOperationText(opText).ToArray();
            if (tokens.Length != 4) return null;
            if (tokens[0] != "s") return null;
            if (tokens[3] != "g") return null;
            return (tokens[1], tokens[2]);
        }

        static IEnumerable<string> SplitOperationText(string opText)
        {
            var buf = new StringBuilder();
            for (int i = 0; i < opText.Length; ++i)
            {
                if (opText[i] == '/' && (i == 0 || opText[i - 1] != '\\'))
                {
                    yield return buf.ToString();
                    buf.Clear();
                }
                else
                {
                    buf.Append(opText[i]);
                }
            }
            yield return buf.ToString();
        }
    }
}
