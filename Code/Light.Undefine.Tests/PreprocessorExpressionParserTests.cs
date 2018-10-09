using System;
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

        private static void MustBeNotExpressionWithSymbol(this PreprocessorExpression expression, string symbol) =>
            expression.MustBeOfType<NotExpression>().Expression.MustBeSymbolExpression(symbol);

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

        [Theory]
        [InlineData("(SILVERLIGHT)", "SILVERLIGHT")]
        [InlineData("( RELEASE )", "RELEASE")]
        [InlineData(" ( NET47 )", "NET47")]
        [InlineData(" ( NET45)", "NET45")]
        [InlineData("(NET35_CF )", "NET35_CF")]
        public static void ParseSymbolInBrackets(string expressionSource, string expectedSymbol) =>
            PreprocessorExpressionParser.Parse(expressionSource).MustBeSymbolExpression(expectedSymbol);

        [Theory]
        [MemberData(nameof(ComplexExpressions))]
        public static void ParseComplexExpression(string expressionSource, Action<PreprocessorExpression> validateExpression) =>
            validateExpression(PreprocessorExpressionParser.Parse(expressionSource));

        public static readonly TheoryData<string, Action<PreprocessorExpression>> ComplexExpressions =
            new TheoryData<string, Action<PreprocessorExpression>>
            {
                {
                    "(DEBUG || STAGING) && (NETSTANDARD2_0 || NET45)",
                    expression =>
                    {
                        var topLevelAndExpression = expression.MustBeOfType<AndExpression>();
                        var leftOrExpression = topLevelAndExpression.Left.MustBeOfType<OrExpression>();
                        var rightOrExpression = topLevelAndExpression.Right.MustBeOfType<OrExpression>();
                        leftOrExpression.Left.MustBeSymbolExpression("DEBUG");
                        leftOrExpression.Right.MustBeSymbolExpression("STAGING");
                        rightOrExpression.Left.MustBeSymbolExpression("NETSTANDARD2_0");
                        rightOrExpression.Right.MustBeSymbolExpression("NET45");
                    }
                },
                {
                    "!(NETSTANDARD || NET45)",
                    expression =>
                    {
                        var topLevelNotExpression = expression.MustBeOfType<NotExpression>();
                        var innerOrExpression = topLevelNotExpression.Expression.MustBeOfType<OrExpression>();
                        innerOrExpression.Left.MustBeSymbolExpression("NETSTANDARD");
                        innerOrExpression.Right.MustBeSymbolExpression("NET45");
                    }
                },
                {
                    "NETCOREAPP && DEBUG && TRACE",
                    expression =>
                    {
                        var topLevelAndExpression = expression.MustBeOfType<AndExpression>();
                        topLevelAndExpression.Left.MustBeSymbolExpression("NETCOREAPP");
                        var innerAndExpression = topLevelAndExpression.Right.MustBeOfType<AndExpression>();
                        innerAndExpression.Left.MustBeSymbolExpression("DEBUG");
                        innerAndExpression.Right.MustBeSymbolExpression("TRACE");
                    }
                },
                {
                    "NETSTANDARD2_0 || NETSTANDARD1_0 || NET45",
                    expression =>
                    {
                        var topLevelOrExpression = expression.MustBeOfType<OrExpression>();
                        topLevelOrExpression.Left.MustBeSymbolExpression("NETSTANDARD2_0");
                        var innerOrExpression = topLevelOrExpression.Right.MustBeOfType<OrExpression>();
                        innerOrExpression.Left.MustBeSymbolExpression("NETSTANDARD1_0");
                        innerOrExpression.Right.MustBeSymbolExpression("NET45");
                    }
                },
                {
                    "(!NET35 && !NET35_CF) && DEBUG",
                    expression =>
                    {
                        var andExpression = expression.MustBeOfType<AndExpression>();
                        andExpression.Right.MustBeSymbolExpression("DEBUG");
                        var orExpression = andExpression.Left.MustBeOfType<AndExpression>();
                        orExpression.Left.MustBeNotExpressionWithSymbol("NET35");
                        orExpression.Right.MustBeNotExpressionWithSymbol("NET35_CF");
                    }
                },
                {
                    "(!DEBUG) && (!STAGING)",
                    expression =>
                    {
                        var andExpression = expression.MustBeOfType<AndExpression>();
                        andExpression.Left.MustBeNotExpressionWithSymbol("DEBUG");
                        andExpression.Right.MustBeNotExpressionWithSymbol("STAGING");
                    }
                },
                {
                    "(!RELEASE || (STAGING && !DEVELOPMENT))",
                    expression =>
                    {
                        var orExpression = expression.MustBeOfType<OrExpression>();
                        orExpression.Left.MustBeNotExpressionWithSymbol("RELEASE");
                        var andExpression = orExpression.Right.MustBeOfType<AndExpression>();
                        andExpression.Left.MustBeSymbolExpression("STAGING");
                        andExpression.Right.MustBeNotExpressionWithSymbol("DEVELOPMENT");
                    }
                },
                {
                    "((!RELEASE && !STAGING) || DEVELOPMENT)",
                    expression =>
                    {
                        var orExpression = expression.MustBeOfType<OrExpression>();
                        orExpression.Right.MustBeSymbolExpression("DEVELOPMENT");
                        var andExpression = orExpression.Left.MustBeOfType<AndExpression>();
                        andExpression.Left.MustBeNotExpressionWithSymbol("RELEASE");
                        andExpression.Right.MustBeNotExpressionWithSymbol("STAGING");
                    }
                },
                {
                    "(((!DEBUG)))",
                    expression => expression.MustBeNotExpressionWithSymbol("DEBUG")
                },
                {
                    "((((NETSTANDARD))))",
                    expression => expression.MustBeSymbolExpression("NETSTANDARD")
                },
                {
                    "NETSTANDARD || ((NET45))",
                    expression =>
                    {
                        var orExpression = expression.MustBeOfType<OrExpression>();
                        orExpression.Left.MustBeSymbolExpression("NETSTANDARD");
                        orExpression.Right.MustBeSymbolExpression("NET45");
                    }
                }
            };
    }
}