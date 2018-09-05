using FluentAssertions;
using Xunit;

namespace Light.Undefine.Tests
{
    public static class AndExpressionTests
    {
        [Fact]
        public static void DerivesFromCompositeExpression() => typeof(AndExpression).Should().BeDerivedFrom<CompositeExpression>();

        [Theory]
        [MemberData(nameof(AllSymbolsDefinedData))]
        public static void AllSymbolsDefined(string[] definedSymbols) =>
            new AndExpression(new Symbol("DEBUG"), new Symbol("TRACE")).Evaluate(definedSymbols).Should().BeTrue();

        public static readonly TheoryData<string[]> AllSymbolsDefinedData =
            new TheoryData<string[]>
            {
                new[] { "DEBUG", "TRACE" },
                new[] { "DEBUG", "CUSTOM", "TRACE" }
            };

        [Theory]
        [MemberData(nameof(NotAllSymbolsDefinedData))]
        public static void NotAllSymbolsDefined(string[] definedSymbols) => 
            new AndExpression(new Symbol("DEBUG"), new Symbol("TRACE")).Evaluate(definedSymbols).Should().BeFalse();

        public static readonly TheoryData<string[]> NotAllSymbolsDefinedData =
            new TheoryData<string[]>
            {
                new[] { "DEBUG" },
                new[] { "TRACE" },
                new[] { "CUSTOM", "RELEASE" }
            };
    }
}