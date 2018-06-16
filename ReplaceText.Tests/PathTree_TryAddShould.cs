using lpubsppop01.ReplaceText;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace lpubsppop01.ReplaceText.Tests
{
    public class PathTree_TryAddShould
    {
        string TestDataPath;
        string JaUtf8CrlfTxtPath;
        string JaUtf8CrlfEndsWithEmptyLineTxtPath;
        string JaUtf8WithBomCrlfTxtPath;
        List<string> AllExistingPaths;

        string NotExistingPath;

        public PathTree_TryAddShould()
        {
            string here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            TestDataPath = Path.Combine(here, @"TestData");
            JaUtf8CrlfTxtPath = Path.Combine(here, @"TestData\ja_utf8_crlf.txt");
            JaUtf8CrlfEndsWithEmptyLineTxtPath = Path.Combine(here, @"TestData\ja_utf8_crlf_endswithemptyline.txt");
            JaUtf8WithBomCrlfTxtPath = Path.Combine(here, @"TestData\ja_utf8withbom_crlf.txt");
            AllExistingPaths = new List<string>
            {
                TestDataPath, JaUtf8CrlfTxtPath, JaUtf8CrlfEndsWithEmptyLineTxtPath, JaUtf8WithBomCrlfTxtPath
            };
            NotExistingPath = Path.Combine(here, @"TestData\not_existing.txt");
        }

        [Fact]
        public void SucceedGivenExistingFilePath()
        {
            var pathTree = new PathTree();
            Assert.True(pathTree.TryAdd(JaUtf8CrlfTxtPath, out string errorMessage));
            Assert.False(pathTree.FindNode(TestDataPath).IsTarget);
            Assert.True(pathTree.FindNode(JaUtf8CrlfTxtPath).IsTarget);
        }

        [Fact]
        public void SucceedGivenExistingDirectoryPath()
        {
            var pathTree = new PathTree();
            Assert.True(pathTree.TryAdd(TestDataPath, out string errorMessage));
            AllExistingPaths.ForEach(path => Assert.True(pathTree.FindNode(path).IsTarget));
        }

        [Fact]
        public void FailGivenNotExistingPath()
        {
            var pathTree = new PathTree();
            Assert.False(pathTree.TryAdd(NotExistingPath, out string errorMessage));
        }

        [Fact]
        public void MergePassedPaths()
        {
            string currentDirectoryBackup = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(TestDataPath);
            try
            {
                var pathTree = new PathTree();
                Assert.True(pathTree.TryAdd(@"Hoge\Hoge.txt", out string errorMessage1));
                Assert.True(pathTree.TryAdd(Path.Combine(TestDataPath, @"Hoge"), out string errorMessage2));
                Assert.Equal(pathTree.FindNode(@"Hoge\Hoge.txt"), pathTree.FindNode(Path.Combine(TestDataPath, @"Hoge\Hoge.txt")));
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectoryBackup);
            }
        }
    }
}
