using System;

namespace Light.Undefine
{
    public enum PreprocessorTokenType
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
        public const string NotOperator = "!";
        public const string AndOperator = "&&";
        public const string OrOperator = "||";
        public const string OpenBracket = "(";
        public const string CloseBracket = ")";

        public static string GetStringRepresentationOfOperatorOrBracket(this PreprocessorTokenType preprocessorTokenType)
        {
            switch (preprocessorTokenType)
            {
                case PreprocessorTokenType.NotOperator:
                    return NotOperator;
                case PreprocessorTokenType.AndOperator:
                    return AndOperator;
                case PreprocessorTokenType.OrOperator:
                    return OrOperator;
                case PreprocessorTokenType.OpenBracket:
                    return OpenBracket;
                case PreprocessorTokenType.CloseBracket:
                    return CloseBracket;
                default:
                    throw new ArgumentOutOfRangeException(nameof(preprocessorTokenType), "Cannot convert Symbol or unknown token type to a constant.");
            }
        }
    }
}