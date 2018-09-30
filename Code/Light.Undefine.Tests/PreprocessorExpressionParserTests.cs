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

        [Theory]
        [InlineData("( RELEASE )", "RELEASE")]
        [InlineData(" ( NET47 )", "NET47")]
        [InlineData(" ( NET45)", "NET45")]
        [InlineData("(NET35_CF )", "NET35_CF")]
        public static void ParseSymbolInBracketsWithWhiteSpace(string expressionSource, string expectedSymbol) => 
            PreprocessorExpressionParser.Parse(expressionSource).MustBeSymbolExpression(expectedSymbol);

        [Fact]
        public static void ParseOrConcatenationWithThreeSymbols()
        {
            var expression = PreprocessorExpressionParser.Parse("NETSTANDARD2_0 || NETSTANDARD1_0 || NET45");

            var topLevelOrExpression = expression.MustBeOfType<OrExpression>();
            topLevelOrExpression.Left.MustBeSymbolExpression("NETSTANDARD2_0");
            var innerOrExpression = topLevelOrExpression.Right.MustBeOfType<OrExpression>();
            innerOrExpression.Left.MustBeSymbolExpression("NETSTANDARD1_0");
            innerOrExpression.Right.MustBeSymbolExpression("NET45");
        }

        [Fact]
        public static void ParseAndConcatenationWithThreeSymbols()
        {
            var expression = PreprocessorExpressionParser.Parse("NETCOREAPP && DEBUG && TRACE");

            var topLevelAndExpression = expression.MustBeOfType<AndExpression>();
            topLevelAndExpression.Left.MustBeSymbolExpression("NETCOREAPP");
            var innerAndExpression = topLevelAndExpression.Right.MustBeOfType<AndExpression>();
            innerAndExpression.Left.MustBeSymbolExpression("DEBUG");
            innerAndExpression.Right.MustBeSymbolExpression("TRACE");
        }

        [Fact]
        public static void ParseTopLevelNotExpression()
        {
            var expression = PreprocessorExpressionParser.Parse("!(NETSTANDARD || NET45)");

            var topLevelNotExpression = expression.MustBeOfType<NotExpression>();
            var innerOrExpression = topLevelNotExpression.Expression.MustBeOfType<OrExpression>();
            innerOrExpression.Left.MustBeSymbolExpression("NETSTANDARD");
            innerOrExpression.Right.MustBeSymbolExpression("NET45");
        }

    }
}