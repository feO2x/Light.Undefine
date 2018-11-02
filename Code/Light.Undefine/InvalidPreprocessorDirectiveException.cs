using System;
using System.Runtime.Serialization;

namespace Light.Undefine
{
    /// <summary>
    /// Represents the exception that is thrown when the <see cref="UndefineTransformation"/> cannot parse a line of code that represents a preprocessor directive correctly.
    /// </summary>
    public class InvalidPreprocessorDirectiveException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="InvalidPreprocessorDirectiveException"/>.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public InvalidPreprocessorDirectiveException(string message) : base(message) { }

        /// <inheritdoc />
        protected InvalidPreprocessorDirectiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}