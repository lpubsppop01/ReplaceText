using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lpubsppop01.ReplaceText
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
                Pattern = tokens[1],
                Replacement = tokens[2],
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
    }
}
