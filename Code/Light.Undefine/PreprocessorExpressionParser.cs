using System;

namespace Light.Undefine
{
    public static class PreprocessorExpressionParser
    {
        public static PreprocessorExpression Parse(string expression, PreprocessorTokenList.Builder tokenListBuilder = null) => Parse(expression.AsMemory());

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

            if (tokens.Count == 3)
            {
                var left = tokens[0];
                var middle = tokens[1];
                var right = tokens[2];

                if (left.Type == PreprocessorTokenType.Symbol &&
                    (middle.Type == PreprocessorTokenType.OrOperator || middle.Type == PreprocessorTokenType.AndOperator) &&
                    right.Type == PreprocessorTokenType.Symbol)
                {
                    var leftExpression = new SymbolExpression(left.SymbolText);
                    var rightExpression = new SymbolExpression(right.SymbolText);
                    return middle.Type == PreprocessorTokenType.OrOperator ? (PreprocessorExpression) new OrExpression(leftExpression, rightExpression) : new AndExpression(leftExpression, rightExpression);
                }

                if (left.Type == PreprocessorTokenType.OpenBracket &&
                    middle.Type == PreprocessorTokenType.Symbol &&
                    right.Type == PreprocessorTokenType.CloseBracket)
                    return new SymbolExpression(middle.SymbolText);
            }


            var (topLevelOperator, operatorIndex) = tokens.FindTopLevelOperator();
            if (operatorIndex == -1)
                throw new InvalidPreprocessorExpressionException("Could not find operator.");

            if (topLevelOperator.Type == PreprocessorTokenType.OrOperator)
                return new OrExpression(
                    CreateExpressionTreeRecursively(tokens.Slice(0, operatorIndex)), 
                    CreateExpressionTreeRecursively(tokens.Slice(operatorIndex + 1)));
            if (topLevelOperator.Type == PreprocessorTokenType.AndOperator)
                return new AndExpression(
                    CreateExpressionTreeRecursively(tokens.Slice(0, operatorIndex)), 
                    CreateExpressionTreeRecursively(tokens.Slice(operatorIndex + 1)));

            return new NotExpression(CreateExpressionTreeRecursively(tokens.Slice(operatorIndex + 1)));
        }
    }
}