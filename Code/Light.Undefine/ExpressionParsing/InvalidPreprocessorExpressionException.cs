using System.Runtime.Serialization;

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// This exception represents an erroneous preprocessor expression that cannot be parsed correctly.
    /// </summary>
    public class InvalidPreprocessorExpressionException : InvalidPreprocessorDirectiveException
    {
        /// <summary>
        /// Initializes a new instance of <see cref="InvalidPreprocessorExpressionException"/>.
        /// </summary>
        /// <param name="message">The message of this exception.</param>
        public InvalidPreprocessorExpressionException(string message) : base(message) { }

        protected InvalidPreprocessorExpressionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}