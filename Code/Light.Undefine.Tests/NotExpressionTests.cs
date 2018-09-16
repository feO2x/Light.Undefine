using FluentAssertions;
using Xunit;

namespace Light.Undefine.Tests
{
    public static class NotExpressionTests
    {
        [Fact]
        public static void DerivesFromPreprocessorExpression() => typeof(NotExpression).Should().BeDerivedFrom<PreprocessorExpression>();

        [Theory]
        [MemberData(nameof(NotDefinedData))]
        public static void NotDefined(string[] definedSymbols) =>
            new NotExpression(new SymbolExpression("NETSTANDARD1_0")).Evaluate(definedSymbols).Should().BeTrue();

        public static readonly TheoryData<string[]> NotDefinedData =
            new TheoryData<string[]>
            {
                new[] { "DEBUG" },
                new[] { "NETSTANDARD2_0", "NET45", "NET40" }
            };

        [Theory]
        [MemberData(nameof(DefinedData))]
        public static void Defined(string[] definedSymbols) => 
            new NotExpression(new SymbolExpression("NET45")).Evaluate(definedSymbols).Should().BeFalse();

        public static readonly TheoryData<string[]> DefinedData =
            new TheoryData<string[]>
            {
                new[] { "NET45" },
                new[] { "NET45", "NETSTANDARD2_0" },
                new[] { "NETSTANDARD2_0", "NET45", "SL5" }
            };
    }
}