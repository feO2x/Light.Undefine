using System;

namespace Light.Undefine
{
    public static class PreprocessorExpressionParser
    {
        public static PreprocessorExpression Parse(string expression, PreprocessorTokenList.Builder tokenListBuilder = null) => Parse(expression.AsMemory());

        public static PreprocessorExpression Parse(in ReadOnlyMemory<char> expression, PreprocessorTokenList.Builder tokenListBuilder = null)
        {
            tokenListBuilder = tokenListBuilder ?? PreprocessorTokenList.Builder.CreateDefault();
            var tokens = PreprocessorExpressionTokenizer.CreateTokens(expression, tokenListBuilder);
            return CreateExpressionTreeRecursively(tokens, expression);
        }

        private static PreprocessorExpression CreateExpressionTreeRecursively(in PreprocessorTokenList tokens, in ReadOnlyMemory<char> expression)
        {
            if (tokens.Count < 4)
            {
                if (tokens.Count == 1)
                {
                    var token = tokens[0];
                    if (token.Type != PreprocessorTokenType.Symbol)
                        ThrowInvalidExpression(expression);
                    return new SymbolExpression(token.SymbolText);
                }

                if (tokens.Count == 2)
                {
                    var first = tokens[0];
                    var second = tokens[1];
                    if (first.Type != PreprocessorTokenType.NotOperator || second.Type != PreprocessorTokenType.Symbol)
                        ThrowInvalidExpression(expression);
                    return new NotExpression(new SymbolExpression(second.SymbolText));
                }


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

                ThrowInvalidExpression(expression);
            }
            
            var analysisResult = tokens.AnalyzeComplexExpression();
            if (analysisResult.CanOuterBracketsBeIgnored)
                return CreateExpressionTreeRecursively(tokens.Slice(1, tokens.Count - 1), expression);

            if (analysisResult.TopLevelOperatorIndex == -1)
                ThrowInvalidExpression(expression);

            if (analysisResult.TopLevelOperator.Type == PreprocessorTokenType.OrOperator)
                return new OrExpression(
                    CreateExpressionTreeRecursively(tokens.Slice(0, analysisResult.TopLevelOperatorIndex), expression),
                    CreateExpressionTreeRecursively(tokens.Slice(analysisResult.TopLevelOperatorIndex + 1), expression));
            if (analysisResult.TopLevelOperator.Type == PreprocessorTokenType.AndOperator)
                return new AndExpression(
                    CreateExpressionTreeRecursively(tokens.Slice(0, analysisResult.TopLevelOperatorIndex), expression),
                    CreateExpressionTreeRecursively(tokens.Slice(analysisResult.TopLevelOperatorIndex + 1), expression));

            return new NotExpression(CreateExpressionTreeRecursively(tokens.Slice(analysisResult.TopLevelOperatorIndex + 1), expression));
        }

        private static void ThrowInvalidExpression(in ReadOnlyMemory<char> expression) => 
            throw new InvalidPreprocessorExpressionException($"\"{expression}\" cannot be parsed to a valid Preprocessor Expression.");
    }
}