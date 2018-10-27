using System.Runtime.Serialization;

namespace Light.Undefine
{
    public class InvalidPreprocessorExpressionException : InvalidPreprocessorDirectiveExpression
    {
        public InvalidPreprocessorExpressionException(string message) : base(message) { }

        protected InvalidPreprocessorExpressionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}