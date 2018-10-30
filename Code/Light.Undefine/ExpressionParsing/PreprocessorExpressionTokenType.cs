using System;

namespace Light.Undefine.ExpressionParsing
{
    public enum PreprocessorExpressionTokenType
    {
        Symbol = 1,
        NotOperator,
        AndOperator,
        OrOperator,
        OpenBracket,
        CloseBracket
    }

    public static class PreprocessorTokenTypeConstants
    {
        

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