﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Lpubsppop01.ReplaceText.Tests
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

        public CommandRunner_RunShould()
        {
            string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestDataPath = Path.Combine(here, "TestData");
            WorkDataPath = Path.Combine(here, "WorkData");
            Ja_UTF8_CRLF_TxtPath = Path.Combine(here, "WorkData/ja_utf8_crlf.txt");
            Ja_UTF8_CRLF_EndsWithEmptyLine_TxtPath = Path.Combine(here, "WorkData/ja_utf8_crlf_endswithemptyline.txt");
            Ja_UTF8WithBom_CRLF_TxtPath = Path.Combine(here, "WorkData/ja_utf8withbom_crlf.txt");
            Ja_SJIS_CRLF_TxtPath = Path.Combine(here, "WorkData/ja_sjis_crlf.txt");
            Ja_EUCJP_LF_TxtPath = Path.Combine(here, "WorkData/ja_eucjp_lf.txt");
        }

        void RefreshWorkData()
        {
            var p = new Process();
            p.StartInfo.CreateNoWindow = true;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                p.StartInfo.FileName = "robocopy";
                p.StartInfo.Arguments = @"/mir " + TestDataPath + " " + WorkDataPath;
            }
            else
            {
                if (Directory.Exists(WorkDataPath)) Directory.Delete(WorkDataPath, recursive: true);
                p.StartInfo.FileName = "cp"; 
                p.StartInfo.Arguments = "-a " + TestDataPath + " " + WorkDataPath; 
            }
            p.Start();
            p.WaitForExit();
        }

        [Fact]
        public void ReplaceFileContentAndName()
        {
            RefreshWorkData();
            string path = Path.Combine(WorkDataPath, "Hoge/Hoge.txt");
            var pathTree = new PathTree();
            pathTree.TryAdd(path, out string errorMessage);
            var commands = new List<Command>();
            if (Command.TryParse("s/Hoge/Piyo/g", out var command)) commands.Add(command);
            var runner = new CommandRunner(commands);
            runner.Run(pathTree, CommandRunnerActionKind.Replace);
            Assert.True(Directory.Exists(Path.Combine(WorkDataPath, "Hoge")));
            Assert.False(Directory.Exists(Path.Combine(WorkDataPath, "Piyo")));
            Assert.False(File.Exists(Path.Combine(WorkDataPath, "Hoge/Hoge.txt")));
            Assert.Equal("Piyo", File.ReadAllText(Path.Combine(WorkDataPath, "Hoge/Piyo.txt")));
        }

        [Fact]
        public void ReplaceDirectoryNameAndDescendant()
        {
            RefreshWorkData();
            string path = Path.Combine(WorkDataPath, "Hoge");
            var pathTree = new PathTree();
            pathTree.TryAdd(path, out string errorMessage);
            var commands = new List<Command>();
            if (Command.TryParse("s/Hoge/Piyo/g", out var command)) commands.Add(command);
            var runner = new CommandRunner(commands);
            runner.Run(pathTree, CommandRunnerActionKind.Replace);
            Assert.False(Directory.Exists(Path.Combine(WorkDataPath, "Hoge")));
            Assert.True(Directory.Exists(Path.Combine(WorkDataPath, "Piyo")));
            Assert.False(File.Exists(Path.Combine(WorkDataPath, "Piyo/Hoge.txt")));
            Assert.Equal("Piyo", File.ReadAllText(Path.Combine(WorkDataPath, "Piyo/Piyo.txt")));
        }

        [Fact]
        public void GenerateFile()
        {
            RefreshWorkData();
            string path = Path.Combine(WorkDataPath, "Hoge/Hoge.txt");
            var pathTree = new PathTree();
            pathTree.TryAdd(path, out string errorMessage);
            var commands = new List<Command>();
            if (Command.TryParse("s/Hoge/Piyo/g", out var command)) commands.Add(command);
            var runner = new CommandRunner(commands);
            runner.Run(pathTree, CommandRunnerActionKind.Genearte);
            Assert.True(Directory.Exists(Path.Combine(WorkDataPath, "Hoge")));
            Assert.False(Directory.Exists(Path.Combine(WorkDataPath, "Piyo")));
            Assert.Equal("Hoge", File.ReadAllText(Path.Combine(WorkDataPath, "Hoge/Hoge.txt")));
            Assert.Equal("Piyo", File.ReadAllText(Path.Combine(WorkDataPath, "Hoge/Piyo.txt")));
        }

        [Fact]
        public void GenerateDirectoryAndDescendant()
        {
            RefreshWorkData();
            string path = Path.Combine(WorkDataPath, "Hoge");
            var pathTree = new PathTree();
            pathTree.TryAdd(path, out string errorMessage);
            var commands = new List<Command>();
            if (Command.TryParse("s/Hoge/Piyo/g", out var command)) commands.Add(command);
            var runner = new CommandRunner(commands);
            runner.Run(pathTree, CommandRunnerActionKind.Genearte);
            Assert.True(Directory.Exists(Path.Combine(WorkDataPath, "Hoge")));
            Assert.True(Directory.Exists(Path.Combine(WorkDataPath, "Piyo")));
            Assert.False(File.Exists(Path.Combine(WorkDataPath, "Hoge/Piyo.txt")));
            Assert.False(File.Exists(Path.Combine(WorkDataPath, "Piyo/Hoge.txt")));
            Assert.Equal("Hoge", File.ReadAllText(Path.Combine(WorkDataPath, "Hoge/Hoge.txt")));
            Assert.Equal("Piyo", File.ReadAllText(Path.Combine(WorkDataPath, "Piyo/Piyo.txt")));
        }

        [Fact]
        public void KeepCharactorEncodingOnContentEdit()
        {
            RefreshWorkData();
            foreach (var path in new[] { Ja_UTF8_CRLF_TxtPath, Ja_UTF8WithBom_CRLF_TxtPath, Ja_SJIS_CRLF_TxtPath, Ja_EUCJP_LF_TxtPath })
            {
                var srcInfo = new TextFileInfo(path);
                var pathTree = new PathTree();
                pathTree.TryAdd(path, out string errorMessage);
                var commands = new List<Command>();
                if (Command.TryParse("s/ほげ/ホゲ/g", out var command)) commands.Add(command);
                var runner = new CommandRunner(commands);
                runner.Run(pathTree, CommandRunnerActionKind.Replace);
                var editedInfo = new TextFileInfo(path);
                Assert.Equal(srcInfo.Encoding, editedInfo.Encoding);
                Assert.Equal(srcInfo.NewLine, editedInfo.NewLine);
            }
        }

        [Fact]
        public void KeepLastEmptyLineOnContentEdit()
        {
            RefreshWorkData();
            foreach (var path in new[] { Ja_UTF8_CRLF_TxtPath, Ja_UTF8_CRLF_EndsWithEmptyLine_TxtPath })
            {
                var fileInfo = new TextFileInfo(path);
                int srcLineCount = fileInfo.ReadAllLines().Count;
                var pathTree = new PathTree();
                pathTree.TryAdd(path, out string errorMessage);
                var commands = new List<Command>();
                if (Command.TryParse("s/ほげ/ホゲ/g", out var command)) commands.Add(command);
                var runner = new CommandRunner(commands);
                runner.Run(pathTree, CommandRunnerActionKind.Replace);
                int lineCount = fileInfo.ReadAllLines().Count;
                Assert.Equal(srcLineCount, lineCount);
            }
        }

        [Fact]
        public void WorkWithSedRegex()
        {
            RefreshWorkData();
            string path = Ja_UTF8_CRLF_TxtPath;
            var pathTree = new PathTree();
            pathTree.TryAdd(path, out string errorMessage);
            var commands = new List<Command>();
            if (Command.TryParse(@"s/ほ\(げ\)/ホ\1/g", out var command)) commands.Add(command);
            var runner = new CommandRunner(commands);
            runner.Run(pathTree, CommandRunnerActionKind.Replace);
            var fileInfo = new TextFileInfo(path);
            var lines = fileInfo.ReadAllLines();
            Assert.Contains(lines, l => l.Contains("ホげ"));
        }

        [Fact]
        public void WorkAsStreamFilter()
        {
            RefreshWorkData();
            var commands = new List<Command>();
            if (Command.TryParse("s/ほげ/ホゲ/g", out var command)) commands.Add(command);
            var runner = new CommandRunner(commands);
            string tempFilePath = Path.GetTempFileName();
            try
            {
                using (var output = new FileStream(tempFilePath, FileMode.Open))
                {
                    using (var input = new FileStream(Ja_UTF8_CRLF_TxtPath, FileMode.Open))
                    {
                        runner.Run(input, output);
                    }
                }
                Assert.Equal(File.ReadAllText(Ja_UTF8_CRLF_TxtPath).Replace("ほげ", "ホゲ"), File.ReadAllText(tempFilePath));
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
