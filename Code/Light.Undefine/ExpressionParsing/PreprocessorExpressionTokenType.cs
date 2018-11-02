using System;

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// This enum contains the different types a preprocessor expression token can have.
    /// </summary>
    public enum PreprocessorExpressionTokenType
    {
        Symbol = 1,
        NotOperator,
        AndOperator,
        OrOperator,
        OpenBracket,
        CloseBracket
    }

    /// <summary>
    /// Provides extension methods for <see cref="PreprocessorExpressionTokenType"/>.
    /// </summary>
    public static class PreprocessorTokenTypeConstants
    {
        /// <summary>
        /// Returns the operator string representation for the specified <paramref name="preprocessorTokenType"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="preprocessorTokenType"/> is <see cref="PreprocessorExpressionTokenType.Symbol"/> or an unknown value.</exception>
        public static string GetStringRepresentationOfOperatorOrBracket(this PreprocessorExpressionTokenType preprocessorTokenType)
        {
            switch (preprocessorTokenType)
            {
                case PreprocessorExpressionTokenType.NotOperator:
                    return Operators.NotOperator;
                case PreprocessorExpressionTokenType.AndOperator:
                    return Operators.AndOperator;
                case PreprocessorExpressionTokenType.OrOperator:
                    return Operators.OrOperator;
                case PreprocessorExpressionTokenType.OpenBracket:
                    return Operators.OpenBracket;
                case PreprocessorExpressionTokenType.CloseBracket:
                    return Operators.CloseBracket;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preprocessorTokenType), "Cannot convert Symbol or unknown token type to a constant.");
            }
        }
    }
}