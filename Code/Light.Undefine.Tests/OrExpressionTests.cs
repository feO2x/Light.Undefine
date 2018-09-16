using FluentAssertions;
using Xunit;

namespace Light.Undefine.Tests
{
    public static class OrExpressionTests
    {
        [Fact]
        public static void DerivesFromBinaryExpression() => typeof(OrExpression).Should().BeDerivedFrom<BinaryExpression>();

        [Theory]
        [MemberData(nameof(AtLeastOneSymbolDefinedData))]
        public static void AtLeastOneSymbolDefined(string[] definedSymbols) =>
            new OrExpression(new SymbolExpression("RELEASE"), new SymbolExpression("DEBUG")).Evaluate(definedSymbols).Should().BeTrue();

        public static readonly TheoryData<string[]> AtLeastOneSymbolDefinedData =
            new TheoryData<string[]>
            {
                new[] { "RELEASE" },
                new[] { "DEBUG", "CUSTOM" },
                new[] { "RELEASE", "DEBUG", "CUSTOM" }
            };

        [Theory]
        [MemberData(nameof(NoSymbolDefinedData))]
        public static void NoSymbolDefined(string[] definedSymbols) => 
            new OrExpression(new SymbolExpression("RELEASE"), new SymbolExpression("STAGING")).Evaluate(definedSymbols);

        public static readonly TheoryData<string[]> NoSymbolDefinedData =
            new TheoryData<string[]>
            {
                new[] { "DEBUG" },
                new[] { "CUSTOM", "DEBUG" }
            };

    }
}