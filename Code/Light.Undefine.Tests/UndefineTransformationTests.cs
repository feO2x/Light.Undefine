using System;
using FluentAssertions;
using Xunit;

namespace Light.Undefine.Tests
{
    public sealed class UndefineTransformationTests
    {
        private readonly UndefineTransformation _transformation = new UndefineTransformation();

        [Fact]
        public void UndefineSimpleIfDirective()
        {
            const string source = @"#if NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif";

            var result = _transformation.Undefine(source.AsSpan(), "NETSTANDARD2_0").ToString();

            result.Should().Be("        [MethodImpl(MethodImplOptions.AggressiveInlining)]" + Environment.NewLine);
        }
    }
}