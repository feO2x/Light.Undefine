using FluentAssertions;
using Light.GuardClauses;
using Xunit;

namespace Light.Undefine.Tests
{
    public static class PreprocessorExpressionParserTests
    {
        [Theory]
        [MemberData(nameof(ValidSymbols))]
        public static void ParseSymbol(string symbol) => 
            PreprocessorExpressionParser.Parse(symbol).MustBeSymbolExpression(symbol);

        [Theory]
        [MemberData(nameof(ValidSymbols))]
        public static void ParseNotExpression(string symbol)
        {
            var expressionSource = $"!{symbol}";

            var actualExpression = PreprocessorExpressionParser.Parse(expressionSource);

            var notExpression = actualExpression.MustBeOfType<NotExpression>();
            notExpression.Expression.MustBeSymbolExpression(symbol);
        }

        private static void MustBeSymbolExpression(this PreprocessorExpression expression, string symbol) =>
            expression.MustBeOfType<SymbolExpression>().Name.Should().Be(symbol);

        public static readonly TheoryData<string> ValidSymbols =
            new TheoryData<string>
            {
                "NETSTANDARD2_0",
                "NET45",
                "DEBUG",
                "RELEASE"
            };

        [Fact]
        public static void ParseOrExpression()
        {
            const string expressionSource = "DEBUG || RELEASE";

            var actualExpression = PreprocessorExpressionParser.Parse(expressionSource);

            var orExpression = actualExpression.MustBeOfType<OrExpression>();
            orExpression.Left.MustBeSymbolExpression("DEBUG");
            orExpression.Right.MustBeSymbolExpression("RELEASE");
        }

        [Fact]
        public static void ParseAndExpression()
        {
            const string expressionSource = "DEBUG && NETSTANDARD";

            var actualExpression = PreprocessorExpressionParser.Parse(expressionSource);

            var andExpression = actualExpression.MustBeOfType<AndExpression>();
            andExpression.Left.MustBeSymbolExpression("DEBUG");
            andExpression.Right.MustBeSymbolExpression("NETSTANDARD");
        }

        [Fact]
        public static void ParseSymbolWithBracketsAroundIt()
        {
            const string expressionSource = "(SILVERLIGHT)"; // <- funny!

            PreprocessorExpressionParser.Parse(expressionSource).MustBeSymbolExpression("SILVERLIGHT");
        }
    }
}