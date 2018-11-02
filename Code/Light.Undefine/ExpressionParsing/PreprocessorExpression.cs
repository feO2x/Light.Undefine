using System.Collections.Generic;

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Represents the abstraction of a preprocessor expression that can be evaluated.
    /// </summary>
    public abstract class PreprocessorExpression
    {
        /// <summary>
        /// Checks if this expression evaluates to true with the given defined preprocessor symbols. The check
        /// for symbols is case-sensitive.
        /// </summary>
        public abstract bool Evaluate(IEnumerable<string> definedSymbols);
    }
}