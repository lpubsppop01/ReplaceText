using Lpubsppop01.ReplaceText;
using System;
using Xunit;

namespace Lpubsppop01.ReplaceText.Tests
{
    public class Command_TryParseShould
    {
        [Fact]
        public void ReturnTrueGivenSubstituteGlobalCommand()
        {
            Command c1, c2;
            Assert.True(Command.TryParse("s/hoge/piyo/g", out c1) && c1.Pattern == "hoge" && c1.Replacement == "piyo");
            Assert.True(Command.TryParse(@"s/\\/\//g", out c2) && c2.Pattern == @"\\" && c2.Replacement == "/");
        }

        [Fact]
        public void ConvertSedRegexToDotNetRegex()
        {
            Assert.True(Command.TryParse(@"s/ho\([gG][eE]\)/\1ge/g", out var command));
            Assert.Equal(@"ho([gG][eE])", command.Pattern);
            Assert.Equal(@"$1ge", command.Replacement);
        }
    }
}
