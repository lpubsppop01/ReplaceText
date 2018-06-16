using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace lpubsppop01.ReplaceText
{
    static class MyFileReader
    {
        public static List<string> ReadAllLines(string path, Encoding encoding, string newLine)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(path, encoding))
            {
                lines.AddRange(reader.ReadToEnd().Split(newLine));
            }
            return lines;
        }
    }
}
