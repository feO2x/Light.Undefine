using System;

namespace Light.Undefine
{
    public static class PreprocessorExpressionParser
    {
        public static PreprocessorExpression Parse(ReadOnlySpan<char> expression)
        {
            if (expression[0] == Operators.Not)
                return new NotExpression(new SymbolExpression(expression.Slice(1).ToString()));
            return new SymbolExpression(expression.ToString());
        }
    }
}