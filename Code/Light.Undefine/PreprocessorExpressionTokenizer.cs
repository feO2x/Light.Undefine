using System;
using Light.GuardClauses;

namespace Light.Undefine
{
    public static class PreprocessorExpressionTokenizer
    {
        private const char AndOperatorCharacter = '&';
        private const char OrOperatorCharacter = '|';
        private const char NotOperatorCharacter = '!';
        private const char OpenBracketCharacter = '(';
        private const char CloseBracketCharacter = ')';
        private const char Underscore = '_';

        public static PreprocessorTokenList CreateTokens(ReadOnlyMemory<char> expression, PreprocessorTokenList.Builder tokenListBuilder) =>
            new InternalTokenizer(expression, tokenListBuilder).CreateTokens();

        private static string ToOperatorSymbol(this char character) => new string(character, 2);

        private struct InternalTokenizer
        {
            private readonly ReadOnlyMemory<char> _expression;
            private readonly PreprocessorTokenList.Builder _tokenListBuilder;
            private int _currentIndex;

            public InternalTokenizer(ReadOnlyMemory<char> expression, PreprocessorTokenList.Builder tokenListBuilder)
            {
                _expression = expression;
                _tokenListBuilder = tokenListBuilder.MustNotBeNull(nameof(tokenListBuilder));
                _currentIndex = 0;
            }

            public PreprocessorTokenList CreateTokens()
            {
                var span = _expression.Span;

                while (AdvanceToNextNonWhiteSpaceCharacter(span))
                {
                    var currentCharacter = span[_currentIndex];
                    if (currentCharacter == AndOperatorCharacter)
                        ExpectTwoCharacterOperator(AndOperatorCharacter, span, PreprocessorTokenType.AndOperator);
                    else if (currentCharacter == OrOperatorCharacter)
                        ExpectTwoCharacterOperator(OrOperatorCharacter, span, PreprocessorTokenType.OrOperator);
                    else if (currentCharacter == NotOperatorCharacter)
                        AddSingleCharacterToken(span, PreprocessorTokenType.NotOperator);
                    else if (currentCharacter == OpenBracketCharacter)
                        AddSingleCharacterToken(span, PreprocessorTokenType.OpenBracket);
                    else if (currentCharacter == CloseBracketCharacter)
                        AddSingleCharacterToken(span, PreprocessorTokenType.CloseBracket);
                    else if (char.IsLetter(currentCharacter) || currentCharacter == Underscore)
                        ExpectSymbol(span);
                }

                if (!_tokenListBuilder.TryBuild(out var tokenList, out var errorMessage))
                    ThrowErrorFromBuilder(span, errorMessage);
                return tokenList;
            }

            private void ExpectSymbol(in ReadOnlySpan<char> span)
            {
                var startIndex = _currentIndex;
                while (++_currentIndex < span.Length)
                {
                    var currentCharacter = span[_currentIndex];
                    if (char.IsWhiteSpace(currentCharacter) || 
                        currentCharacter == OrOperatorCharacter || 
                        currentCharacter == AndOperatorCharacter || 
                        currentCharacter == CloseBracketCharacter)
                        break;
                    if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == Underscore)
                        continue;

                    throw new InvalidPreprocessorExpressionException($"The expression \"{span.ToString()}\" contains an invalid Symbol character at position {_currentIndex}.");
                }

                if (!_tokenListBuilder.TryAdd(new PreprocessorToken(PreprocessorTokenType.Symbol, span.Slice(startIndex, _currentIndex - startIndex).ToString()), out var errorMessage))
                    ThrowErrorFromBuilder(span, errorMessage);
            }

            private void ExpectTwoCharacterOperator(char operatorCharacter, in ReadOnlySpan<char> span, PreprocessorTokenType tokenType)
            {
                if (span.Length - _currentIndex - 1 < 2)
                    throw new InvalidPreprocessorExpressionException($"The expression \"{span.ToString()}\" contains an invalid use of the {operatorCharacter.ToOperatorSymbol()} operator at index {_currentIndex}.");

                if (span[++_currentIndex] != operatorCharacter)
                    throw new InvalidPreprocessorExpressionException($"The expression \"{span.ToString()}\" contains no second operator symbol for {operatorCharacter.ToOperatorSymbol()} at index {_currentIndex}.");

                if (!_tokenListBuilder.TryAdd(new PreprocessorToken(tokenType), out var errorMessage))
                    ThrowErrorFromBuilder(span, errorMessage);
                ++_currentIndex;
            }

            private void AddSingleCharacterToken(in ReadOnlySpan<char> span, PreprocessorTokenType tokenType)
            {
                if (!_tokenListBuilder.TryAdd(new PreprocessorToken(tokenType), out var errorMessage))
                    ThrowErrorFromBuilder(span, errorMessage);
                ++_currentIndex;
            }

            private bool AdvanceToNextNonWhiteSpaceCharacter(in ReadOnlySpan<char> span)
            {
                if (_currentIndex >= span.Length)
                    return false;

                while (char.IsWhiteSpace(span[_currentIndex]))
                {
                    if (_currentIndex < span.Length - 1)
                        ++_currentIndex;
                    else
                        return false;
                }

                return true;
            }

            private static void ThrowErrorFromBuilder(in ReadOnlySpan<char> span, string errorMessage) =>
                throw new InvalidPreprocessorExpressionException($"The expression \"{span.ToString()}\" is erroneous. {errorMessage}");
        }
    }
}