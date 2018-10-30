using System;
using System.Runtime.Serialization;

namespace Light.Undefine
{
    public class InvalidPreprocessorDirectiveException : Exception
    {
        public InvalidPreprocessorDirectiveException(string message) : base(message) { }

        protected InvalidPreprocessorDirectiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}