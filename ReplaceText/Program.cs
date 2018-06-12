using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lpubsppop01.ReplaceText
{
    class Program
    {
        static void Main(string[] args)
        {
            var ops = ArgumentParser.Parse(Environment.GetCommandLineArgs());
            if (ops == null) return;

            var replacer = new TextReplacer(ops.Value.Item1);
            replacer.Replace(ops.Value.paths, ops.Value.kind);
        }

    }
}
