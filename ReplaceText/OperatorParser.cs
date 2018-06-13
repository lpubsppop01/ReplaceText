using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lpubsppop01.ReplaceText
{
    enum OperatorKind
    {
        Substitute
    }

    [Flags]
    enum OperatorFlags
    {
        None = 0,
        Global = 1 << 0,
    }

    class Operator
    {
        public OperatorKind Kind { get; set; }
        public string SearchPattern { get; set; }
        public string Replacement { get; set; }
        public OperatorFlags Flags { get; set; }
    }

    static class OperatorParser
    {
        public static bool TryParse(string opText, out Operator op)
        {
            op = null;
            var tokens = SplitBySeprator(opText).ToArray();
            if (tokens.Length != 4) return false;
            if (tokens[0] != "s") return false;
            if (tokens[3] != "g") return false;
            op = new Operator
            {
                Kind = OperatorKind.Substitute,
                SearchPattern = tokens[1],
                Replacement = tokens[2],
                Flags = OperatorFlags.Global
            };
            return true;
        }

        static IEnumerable<string> SplitBySeprator(string opText)
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
