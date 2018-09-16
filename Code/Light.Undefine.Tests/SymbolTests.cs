using FluentAssertions;
using Xunit;

namespace Light.Undefine.Tests
{
    public static class SymbolTests
    {
        [Fact]
        public static void SymbolDerivesFromPreprocessorExpression() => typeof(SymbolExpression).Should().BeDerivedFrom<PreprocessorExpression>();

        [Theory]
        [MemberData(nameof(SymbolIsDefinedData))]
        public static void SymbolIsDefined(string[] predefinedSymbols) =>
            new SymbolExpression("DEBUG").Evaluate(predefinedSymbols).Should().BeTrue();

        public static readonly TheoryData<string[]> SymbolIsDefinedData =
            new TheoryData<string[]>
            {
                new[] { "DEBUG" },
                new[] { "DEBUG", "TRACE" },
                new[] { "CUSTOM", "TRACE", "DEBUG" }
            };

        [Theory]
        [MemberData(nameof(SymbolIsNotDefinedData))]
        public static void SymbolIsNotDefined(string[] predefinedSymbols) =>
            new SymbolExpression("RELEASE").Evaluate(predefinedSymbols).Should().BeFalse();

        public static readonly TheoryData<string[]> SymbolIsNotDefinedData =
            new TheoryData<string[]>
            {
                new[] { "DEBUG" },
                new[] { "DEBUG", "TRACE" }
            };
    }
}