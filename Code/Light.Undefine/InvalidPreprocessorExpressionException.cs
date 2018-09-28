using System;
using System.Runtime.Serialization;

namespace Light.Undefine
{
    public class InvalidPreprocessorExpressionException : Exception
    {
        public InvalidPreprocessorExpressionException(string message) : base(message) { }

        protected InvalidPreprocessorExpressionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}