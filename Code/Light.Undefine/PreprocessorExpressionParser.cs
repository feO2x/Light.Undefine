using System;

namespace Light.Undefine
{
    public static class PreprocessorExpressionParser
    {
        public static PreprocessorExpression Parse(ReadOnlyMemory<char> expression, PreprocessorTokenList.Builder tokenListBuilder = null)
        {
            tokenListBuilder = tokenListBuilder ?? PreprocessorTokenList.Builder.CreateDefault();
            var tokens = PreprocessorExpressionTokenizer.CreateTokens(expression, tokenListBuilder);
            return CreateExpressionTreeRecursively(tokens);
        }

        private static PreprocessorExpression CreateExpressionTreeRecursively(in PreprocessorTokenList tokens)
        {
            if (tokens.Count == 1)
            {
                var token = tokens[0];
                if (token.Type != PreprocessorTokenType.Symbol)
                    throw new InvalidPreprocessorExpressionException("A single token must always be a Symbol.");
                return new SymbolExpression(token.SymbolText);
            }

            if (tokens.Count == 2)
            {
                var first = tokens[0];
                var second = tokens[1];
                if (first.Type != PreprocessorTokenType.NotOperator || second.Type != PreprocessorTokenType.Symbol)
                    throw new InvalidPreprocessorExpressionException("Two tokens always have to be a Not Operator and a Symbol.");
                return new NotExpression(new SymbolExpression(second.SymbolText));
            }

            throw new NotImplementedException();
        }
    }
}