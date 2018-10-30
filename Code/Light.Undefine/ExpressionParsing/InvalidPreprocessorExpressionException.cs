using System.Runtime.Serialization;

namespace Light.Undefine.ExpressionParsing
{
    public class InvalidPreprocessorExpressionException : InvalidPreprocessorDirectiveException
    {
        public InvalidPreprocessorExpressionException(string message) : base(message) { }

        protected InvalidPreprocessorExpressionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}