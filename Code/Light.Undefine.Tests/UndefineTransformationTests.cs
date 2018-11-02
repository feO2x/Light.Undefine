using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Light.Undefine.Tests
{
    public static class UndefineTransformationTests
    {
        private static readonly UndefineTransformation TestTarget = new UndefineTransformation();

        [Theory]
        [MemberData(nameof(UndefineData))]
        public static void Undefine(string validSourceCode, IEnumerable<string> definedSymbols, string expectedResult) =>
            TestTarget.Undefine(validSourceCode, definedSymbols).ToString().Should().Be(expectedResult);

        public static readonly TheoryData<string, IEnumerable<string>, string> UndefineData =
            new TheoryData<string, IEnumerable<string>, string>
            {
                // Simple if directive
                {
                    @"#if NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif",
                    new[] { "NETSTANDARD2_0" },
                    "        [MethodImpl(MethodImplOptions.AggressiveInlining)]".AppendNewLine()
                },

                // If directive with complex or expression
                {
                    @"#if NETSTANDARD2_0 || NETSTANDARD1_0 || NET45 || SILVERLIGHT
using System.Runtime.CompilerServices;
#endif",
                    new[] { "SILVERLIGHT" },
                    "using System.Runtime.CompilerServices;".AppendNewLine()
                },

                // If directive that is not true
                {
                    @"#if NETSTANDARD1_0
using Light.GuardClauses.FrameworkExtensions;
#endif",
                    new[] { "NETSTANDARD2_0" },
                    string.Empty
                },

                // If-else directive, if wins
                {
                    @"#if !NETSTANDARD1_0
                    FlagsPattern |= EnumConstantsArray[i].ToUInt64(null);
#else
                    FlagsPattern |= Convert.ToUInt64(EnumConstantsArray[i]);
#endif",
                    new[] { "NET45", "NETSTANDARD2_0" },
                    "                    FlagsPattern |= EnumConstantsArray[i].ToUInt64(null);".AppendNewLine()
                },

                // If-else directive, else wins
                {
                    @"#if !NETSTANDARD1_0
            var fields = typeof(T).GetFields();
#else
            var fields = typeof(T).GetTypeInfo().DeclaredFields.AsArray();
#endif",
                    new[] { "NETSTANDARD1_0" },
                    "            var fields = typeof(T).GetTypeInfo().DeclaredFields.AsArray();".AppendNewLine()
                }
            };

        private static string AppendNewLine(this string input) => input + Environment.NewLine;
    }
}