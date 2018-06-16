﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace lpubsppop01.ReplaceText.Tests
{
    public class CommandRunner_RunShould
    {
        string TestDataPath;
        string WorkDataPath;
        string Ja_UTF8_CRLF_TxtPath;
        string Ja_UTF8_CRLF_EndsWithEmptyLine_TxtPath;
        string Ja_UTF8WithBom_CRLF_TxtPath;
        string Ja_SJIS_CRLF_TxtPath;
        string Ja_EUCJP_LF_TxtPath;
        List<string> AllExistingPaths;

        string NotExistingPath;

        public CommandRunner_RunShould()
        {
            string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestDataPath = Path.Combine(here, @"TestData");
            WorkDataPath = Path.Combine(here, @"WorkData");
            Ja_UTF8_CRLF_TxtPath = Path.Combine(here, @"WorkData\ja_utf8_crlf.txt");
            Ja_UTF8_CRLF_EndsWithEmptyLine_TxtPath = Path.Combine(here, @"WorkData\ja_utf8_crlf_endswithemptyline.txt");
            Ja_UTF8WithBom_CRLF_TxtPath = Path.Combine(here, @"WorkData\ja_utf8withbom_crlf.txt");
            Ja_SJIS_CRLF_TxtPath = Path.Combine(here, @"WorkData\ja_sjis_crlf.txt");
            Ja_EUCJP_LF_TxtPath = Path.Combine(here, @"WorkData\ja_eucjp_lf.txt");
            AllExistingPaths = new List<string>
            {
                TestDataPath, Ja_UTF8_CRLF_TxtPath, Ja_UTF8_CRLF_EndsWithEmptyLine_TxtPath, Ja_UTF8WithBom_CRLF_TxtPath
            };
        }

        void RefreshWorkData()
        {
            var p = new Process();
            p.StartInfo.FileName = "robocopy";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = @"/mir " + TestDataPath + " " + WorkDataPath;
            p.Start();
            p.WaitForExit();
        }

        [Fact]
        public void KeepCharactorEncodingOnContentEdit()
        {
            RefreshWorkData();
            foreach (var path in new[] { Ja_UTF8_CRLF_TxtPath, Ja_UTF8WithBom_CRLF_TxtPath, Ja_SJIS_CRLF_TxtPath, Ja_EUCJP_LF_TxtPath })
            {
                (var srcEncoding, var srcNewLine) = EncodingDetector.Detect(path);
                var pathTree = new PathTree();
                pathTree.TryAdd(path, out string errorMessage);
                var commands = new List<Command>();
                if (Command.TryParse("s/ほげ/ホゲ/g", out var command)) commands.Add(command);
                var runner = new CommandRunner(commands);
                runner.Run(pathTree, CommandRunnerActionKind.Replace);
                (var encoding, var newLine) = EncodingDetector.Detect(path);
                Assert.Equal(srcEncoding, encoding);
                Assert.Equal(srcNewLine, newLine);
            }
        }
    }
}