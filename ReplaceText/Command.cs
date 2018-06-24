using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lpubsppop01.ReplaceText
{
    enum CommandKind
    {
        Substitute
    }

    [Flags]
    enum CommandFlags
    {
        None = 0,
        Global = 1 << 0,
    }

    class Command
    {
        public CommandKind Kind { get; set; }
        public string Pattern { get; set; }
        public string Replacement { get; set; }
        public CommandFlags Flags { get; set; }

        public static bool TryParse(string text, out Command command)
        {
            command = null;
            var tokens = SplitBySeprator(text).ToArray();
            if (tokens.Length != 4) return false;
            if (tokens[0] != "s") return false;
            if (tokens[3] != "g") return false;
            command = new Command
            {
                Kind = CommandKind.Substitute,
                Pattern = ConvertSedPatternToDotNetPattern(tokens[1]),
                Replacement = ConvertSedReplacementToDotNetReplacement(tokens[2]),
                Flags = CommandFlags.Global
            };
            return true;
        }

        static IEnumerable<string> SplitBySeprator(string text)
        {
            var buf = new StringBuilder();
            for (int i = 0; i < text.Length; ++i)
            {
                if (text[i] == '/' && (i == 0 || text[i - 1] != '\\'))
                {
                    yield return buf.ToString();
                    buf.Clear();
                }
                else
                {
                    buf.Append(text[i]);
                }
            }
            yield return buf.ToString();
        }

        static string ConvertSedPatternToDotNetPattern(string pattern)
        {
            var dest = new List<char>();
            bool backslash = false;
            foreach (var c in pattern)
            {
                if (c == '\\')
                {
                    backslash = !backslash;
                }
                else if (c == '(' || c == ')')
                {
                    if (backslash)
                    {
                        backslash = false;
                        dest.RemoveAt(dest.Count - 1);
                    }
                    else
                    {
                        dest.Add('\\');
                    }
                }
                dest.Add(c);
            }
            return new string(dest.ToArray());
        }

        static string ConvertSedReplacementToDotNetReplacement(string replacement)
        {
            var dest = new List<char>();
            bool backslash = false;
            foreach (var c in replacement)
            {
                if (c == '\\')
                {
                    backslash = !backslash;
                }
                else if (c == '$')
                {
                    dest.Add('\\');
                }
                else if (c >= '0' && c <= '9')
                {
                    if (backslash)
                    {
                        backslash = false;
                        dest.RemoveAt(dest.Count - 1);
                        dest.Add('$');
                    }
                }
                dest.Add(c);
            }
            return new string(dest.ToArray());
        }
    }
}
