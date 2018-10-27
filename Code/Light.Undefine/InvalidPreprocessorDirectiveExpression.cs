using System;
using System.Runtime.Serialization;

namespace Light.Undefine
{
    public class InvalidPreprocessorDirectiveExpression : Exception
    {
        public InvalidPreprocessorDirectiveExpression(string message) : base(message) { }

        protected InvalidPreprocessorDirectiveExpression(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}