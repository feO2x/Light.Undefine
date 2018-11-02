using System;
using FluentAssertions.Specialized;
using Xunit.Abstractions;

namespace Light.Undefine.Tests
{
    public static class TestExtensions
    {
        public static void WriteExceptionTo<TException>(this ExceptionAssertions<TException> exceptionAssertion, ITestOutputHelper output) where TException : Exception =>
            output.WriteLine(exceptionAssertion.Which.ToString());
    }
}