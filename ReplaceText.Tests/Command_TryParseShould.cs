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
            Assert.True(Command.TryParse("s/hoge/piyo/g", out var command1));
            Assert.True(Command.TryParse(@"s/\\/\//g", out var command2));
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
