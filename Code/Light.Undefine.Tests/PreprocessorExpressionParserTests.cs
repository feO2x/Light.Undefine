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
            PreprocessorExpressionParser.Parse(symbol.AsMemory()).MustBeSymbolExpression(symbol);

        [Theory]
        [MemberData(nameof(ValidSymbols))]
        public static void ParseNotSymbolExpressions(string symbol)
        {
            var expression = $"!{symbol}";

            var actualExpression = PreprocessorExpressionParser.Parse(expression.AsMemory());

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
    }
}