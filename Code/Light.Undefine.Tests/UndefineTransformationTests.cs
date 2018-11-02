using System;
using System.Collections.Generic;
using FluentAssertions;
using Light.GuardClauses;
using Xunit;
using Xunit.Abstractions;

namespace Light.Undefine.Tests
{
    public static class UndefineTransformationTests
    {
        private static readonly UndefineTransformation Transformation = new UndefineTransformation();

        [Theory]
        [MemberData(nameof(UndefineData))]
        public static void Undefine(string validSourceCode, IEnumerable<string> definedSymbols, string expectedResult) =>
            Transformation.Undefine(validSourceCode, definedSymbols).ToString().Should().Be(expectedResult);

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
                },

                // ReSharper disable CommentTypo
                // ReSharper disable StringLiteralTypo

                // If-elif-else directive, if wins
                {
                    @"#if NET45 || NETSTANDARD2_0
            typeof(T).GetCustomAttribute(Types.FlagsAttributeType) != null;
#elif NETSTANDARD1_0
            typeof(T).GetTypeInfo().GetCustomAttribute(Types.FlagsAttributeType) != null;
#else
            typeof(T).GetCustomAttributes(Types.FlagsAttributeType, false).FirstOrDefault() != null;
#endif",
                    new[] { "NETSTANDARD2_0" },
                    "            typeof(T).GetCustomAttribute(Types.FlagsAttributeType) != null;".AppendNewLine()
                },

                // If-elif-else directive, elif wins
                {
                    @"#if NET45 || NETSTANDARD2_0
            typeof(T).GetCustomAttribute(Types.FlagsAttributeType) != null;
#elif NETSTANDARD1_0
            typeof(T).GetTypeInfo().GetCustomAttribute(Types.FlagsAttributeType) != null;
#else
            typeof(T).GetCustomAttributes(Types.FlagsAttributeType, false).FirstOrDefault() != null;
#endif",
                    new[] { "NETSTANDARD1_0" },
                    "            typeof(T).GetTypeInfo().GetCustomAttribute(Types.FlagsAttributeType) != null;".AppendNewLine()
                },

                // If-elif-else directive, else wins
                {
                    @"#if NET45 || NETSTANDARD2_0
            typeof(T).GetCustomAttribute(Types.FlagsAttributeType) != null;
#elif NETSTANDARD1_0
            typeof(T).GetTypeInfo().GetCustomAttribute(Types.FlagsAttributeType) != null;
#else
            typeof(T).GetCustomAttributes(Types.FlagsAttributeType, false).FirstOrDefault() != null;
#endif",
                    new[] { "SOMETHING_ELSE" },
                    "            typeof(T).GetCustomAttributes(Types.FlagsAttributeType, false).FirstOrDefault() != null;".AppendNewLine()
                },


                // Subsequent preprocessor directives test 1
                {
                    @"        /// <summary>
        /// Ensures that the specified object reference is not null, or otherwise throws an <see cref=""ArgumentNullException"" />.
        /// </summary>
        /// <param name=""parameter"">The object reference to be checked.</param>
        /// <param name=""parameterName"">The name of the parameter (optional).</param>
        /// <param name=""message"">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref=""ArgumentNullException"">Thrown when <paramref name=""parameter"" /> is null.</exception>
#if (NETSTANDARD2_0 || NETSTANDARD1_0 || NET45 || SILVERLIGHT)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [ContractAnnotation(""parameter:null => halt; parameter:notnull => notnull"")]
        public static T MustNotBeNull<T>(this T parameter, string parameterName = null, string message = null) where T : class
        {
            if (parameter == null)
                Throw.ArgumentNull(parameterName, message);
            return parameter;
        }

        /// <summary>
        /// Ensures that the specified object reference is not null, or otherwise throws your custom exception.
        /// </summary>
        /// <param name=""parameter"">The reference to be checked.</param>
        /// <param name=""exceptionFactory"">The delegate that creates your custom exception.</param>
        /// <exception cref=""Exception"">Your custom exception thrown when <paramref name=""parameter"" /> is null.</exception>
#if (NETSTANDARD2_0 || NETSTANDARD1_0 || NET45 || SILVERLIGHT)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [ContractAnnotation(""parameter:null => halt; parameter:notnull => notnull; exceptionFactory:null => halt"")]
        public static T MustNotBeNull<T>(this T parameter, Func<Exception> exceptionFactory) where T : class
        {
            if (parameter == null)
                Throw.CustomException(exceptionFactory);
            return parameter;
        }",
                    new[] { "NETSTANDARD2_0" },
                    @"        /// <summary>
        /// Ensures that the specified object reference is not null, or otherwise throws an <see cref=""ArgumentNullException"" />.
        /// </summary>
        /// <param name=""parameter"">The object reference to be checked.</param>
        /// <param name=""parameterName"">The name of the parameter (optional).</param>
        /// <param name=""message"">The message that will be passed to the resulting exception (optional).</param>
        /// <exception cref=""ArgumentNullException"">Thrown when <paramref name=""parameter"" /> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation(""parameter:null => halt; parameter:notnull => notnull"")]
        public static T MustNotBeNull<T>(this T parameter, string parameterName = null, string message = null) where T : class
        {
            if (parameter == null)
                Throw.ArgumentNull(parameterName, message);
            return parameter;
        }

        /// <summary>
        /// Ensures that the specified object reference is not null, or otherwise throws your custom exception.
        /// </summary>
        /// <param name=""parameter"">The reference to be checked.</param>
        /// <param name=""exceptionFactory"">The delegate that creates your custom exception.</param>
        /// <exception cref=""Exception"">Your custom exception thrown when <paramref name=""parameter"" /> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation(""parameter:null => halt; parameter:notnull => notnull; exceptionFactory:null => halt"")]
        public static T MustNotBeNull<T>(this T parameter, Func<Exception> exceptionFactory) where T : class
        {
            if (parameter == null)
                Throw.CustomException(exceptionFactory);
            return parameter;
        }"
                },

                // Subsequent directives test 2
                {
                    @"using System;
#if (NETSTANDARD2_0 || NET45 || NET40 || NET35)
using System.Runtime.Serialization;
#endif

namespace Light.GuardClauses.Exceptions
{
    /// <summary>
    /// This exception indicates that an URI is absolute instead of relative.
    /// </summary>
#if (NETSTANDARD2_0 || NET45 || NET40)
    [Serializable]
#endif
    public class AbsoluteUriException : UriException
    {
        /// <summary>
        /// Creates a new instance of <see cref=""AbsoluteUriException"" />.
        /// </summary>
        /// <param name=""parameterName"">The name of the parameter (optional).</param>
        /// <param name=""message"">The message of the exception (optional).</param>
        public AbsoluteUriException(string parameterName = null, string message = null) : base(parameterName, message) { }

#if (NETSTANDARD2_0 || NET45 || NET40 || NET35)
        /// <inheritdoc />
        protected AbsoluteUriException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}",
                    new[] { "NET35" },
                    @"using System;
using System.Runtime.Serialization;

namespace Light.GuardClauses.Exceptions
{
    /// <summary>
    /// This exception indicates that an URI is absolute instead of relative.
    /// </summary>
    public class AbsoluteUriException : UriException
    {
        /// <summary>
        /// Creates a new instance of <see cref=""AbsoluteUriException"" />.
        /// </summary>
        /// <param name=""parameterName"">The name of the parameter (optional).</param>
        /// <param name=""message"">The message of the exception (optional).</param>
        public AbsoluteUriException(string parameterName = null, string message = null) : base(parameterName, message) { }

        /// <inheritdoc />
        protected AbsoluteUriException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}"
                },

                

                // Nested directive which evaluates to true
                {
                    @"#if NETSTANDARD
    Line 1
    #if NETSTANDARD2_0
    Line 2
    #endif
#endif",
                    new[] { "NETSTANDARD", "NETSTANDARD2_0" },
                    @"    Line 1
    Line 2".AppendNewLine()
                },

                // Nested directive which evaluates to false
                {
                    @"#if NETSTANDARD
    Line 1
    #if NETSTANDARD2_0
    Line 2
    #endif
#endif",
                    new[] { "NETSTANDARD", "NETSTANDARD1_0" },
                    "    Line 1".AppendNewLine()
                }
            };

        private static string AppendNewLine(this string input) => input + Environment.NewLine;

        public sealed class ErroneousCases
        {
            private static readonly UndefineTransformation UndefineTransformation = new UndefineTransformation();
            private readonly ITestOutputHelper _output;

            public ErroneousCases(ITestOutputHelper output) => _output = output.MustNotBeNull(nameof(output));

            [Theory]
            [InlineData("#i")]
            [InlineData("#if")]
            [InlineData("#if DEBUG")]
            [InlineData(@"#if DEBUG
#end")]
            [InlineData("#else")]
            [InlineData("#elif")]
            [InlineData("#if !DEBUG ||")]
            [InlineData(@"#if !DEBUG
                        #else
#elif FOO
#endif")]
            public void TransformationMustThrowExceptionWhenSourceCodeContainsInvalidDirectives(string invalidSourceCode) =>
                new Action(() => UndefineTransformation.Undefine(invalidSourceCode, "DEBUG")).Should().Throw<InvalidPreprocessorDirectiveException>().WriteExceptionTo(_output);
        }
    }
    // ReSharper restore CommentTypo
    // ReSharper restore StringLiteralTypo
}