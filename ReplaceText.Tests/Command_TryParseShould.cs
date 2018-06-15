using lpubsppop01.ReplaceText;
using System;
using Xunit;

namespace lpubsppop01.ReplaceText.Tests
{
    public class Command_TryParseShould
    {
        [Fact]
        public void ReturnTrueGivenSubstituteGlobalCommand()
        {
            Assert.True(Command.TryParse("s/hoge/piyo/g", out var command));
        }
    }
}
