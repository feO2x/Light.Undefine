using System;

// ReSharper disable CommentTypo

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Provides static methods to parse a preprocessor expression.
    /// </summary>
    public static class PreprocessorExpressionParser
    {
        /// <summary>
        /// Parses the specified source code to an preprocessor expression tree.
        /// </summary>
        /// <param name="expression">The source code that that should be parsed. It must only contain the expression (without #if or #elif keyword).</param>
        /// <param name="tokenListBuilder">The builder that is used to assemble and validate tokens of the preprocessor expression (optional).</param>
        /// <returns>The top expression of the parsed tree.</returns>
        public static PreprocessorExpression Parse(string expression, PreprocessorExpressionTokenList.Builder tokenListBuilder = null) => Parse(expression.AsSpan());

        /// <summary>
        /// Parses the specified source code to an preprocessor expression tree.
        /// </summary>
        /// <param name="expression">The source code that that should be parsed. It must only contain the expression (without #if or #elif keyword).</param>
        /// <param name="tokenListBuilder">The builder that is used to assemble and validate tokens of the preprocessor expression (optional).</param>
        /// <returns>The top expression of the parsed tree.</returns>
        public static PreprocessorExpression Parse(in ReadOnlySpan<char> expression, PreprocessorExpressionTokenList.Builder tokenListBuilder = null)
        {
            tokenListBuilder = tokenListBuilder ?? PreprocessorExpressionTokenList.Builder.CreateDefault();
            var tokens = PreprocessorExpressionTokenizer.CreateTokens(expression, tokenListBuilder);
            return CreateExpressionTreeRecursively(tokens, expression);
        }

        private static PreprocessorExpression CreateExpressionTreeRecursively(in PreprocessorExpressionTokenList tokens, in ReadOnlySpan<char> expression)
        {
            if (tokens.Count < 4)
            {
                if (tokens.Count == 1)
                {
                    var token = tokens[0];
                    if (token.Type != PreprocessorExpressionTokenType.Symbol)
                        ThrowInvalidExpression(expression);
                    return new SymbolExpression(token.SymbolText);
                }

                if (tokens.Count == 2)
                {
                    var first = tokens[0];
                    var second = tokens[1];
                    if (first.Type != PreprocessorExpressionTokenType.NotOperator || second.Type != PreprocessorExpressionTokenType.Symbol)
                        ThrowInvalidExpression(expression);
                    return new NotExpression(new SymbolExpression(second.SymbolText));
                }


                var left = tokens[0];
                var middle = tokens[1];
                var right = tokens[2];

                if (left.Type == PreprocessorExpressionTokenType.Symbol &&
                    (middle.Type == PreprocessorExpressionTokenType.OrOperator || middle.Type == PreprocessorExpressionTokenType.AndOperator) &&
                    right.Type == PreprocessorExpressionTokenType.Symbol)
                {
                    var leftExpression = new SymbolExpression(left.SymbolText);
                    var rightExpression = new SymbolExpression(right.SymbolText);
                    return middle.Type == PreprocessorExpressionTokenType.OrOperator ? (PreprocessorExpression) new OrExpression(leftExpression, rightExpression) : new AndExpression(leftExpression, rightExpression);
                }

                if (left.Type == PreprocessorExpressionTokenType.OpenBracket &&
                    middle.Type == PreprocessorExpressionTokenType.Symbol &&
                    right.Type == PreprocessorExpressionTokenType.CloseBracket)
                    return new SymbolExpression(middle.SymbolText);

                ThrowInvalidExpression(expression);
            }
            
            var analysisResult = tokens.AnalyzeComplexExpression();
            if (analysisResult.CanOuterBracketsBeIgnored)
                return CreateExpressionTreeRecursively(tokens.Slice(1, tokens.Count - 1), expression);

            if (analysisResult.TopLevelOperatorIndex == -1)
                ThrowInvalidExpression(expression);

            if (analysisResult.TopLevelOperator.Type == PreprocessorExpressionTokenType.OrOperator)
                return new OrExpression(
                    CreateExpressionTreeRecursively(tokens.Slice(0, analysisResult.TopLevelOperatorIndex), expression),
                    CreateExpressionTreeRecursively(tokens.Slice(analysisResult.TopLevelOperatorIndex + 1), expression));
            if (analysisResult.TopLevelOperator.Type == PreprocessorExpressionTokenType.AndOperator)
                return new AndExpression(
                    CreateExpressionTreeRecursively(tokens.Slice(0, analysisResult.TopLevelOperatorIndex), expression),
                    CreateExpressionTreeRecursively(tokens.Slice(analysisResult.TopLevelOperatorIndex + 1), expression));

            return new NotExpression(CreateExpressionTreeRecursively(tokens.Slice(analysisResult.TopLevelOperatorIndex + 1), expression));
        }

        private static void ThrowInvalidExpression(in ReadOnlySpan<char> expression) => 
            throw new InvalidPreprocessorExpressionException($"\"{expression.ToString()}\" cannot be parsed to a valid preprocessor expression.");
    }
}