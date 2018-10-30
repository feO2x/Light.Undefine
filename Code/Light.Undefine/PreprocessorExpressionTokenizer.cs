﻿using System;

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

        public static PreprocessorExpressionTokenList CreateTokens(in ReadOnlySpan<char> expression, PreprocessorExpressionTokenList.Builder tokenListBuilder) =>
            new InternalTokenizer(expression, tokenListBuilder).CreateTokens();

        private static string ToOperatorSymbol(this char character) => new string(character, 2);

        private ref struct InternalTokenizer
        {
            private readonly ReadOnlySpan<char> _expression;
            private readonly PreprocessorExpressionTokenList.Builder _tokenListBuilder;
            private int _currentIndex;

            public InternalTokenizer(ReadOnlySpan<char> expression, PreprocessorExpressionTokenList.Builder tokenListBuilder)
            {
                _tokenListBuilder = tokenListBuilder;
                _expression = expression;
                _currentIndex = 0;
            }

            public PreprocessorExpressionTokenList CreateTokens()
            {
                while (AdvanceToNextNonWhiteSpaceCharacter())
                {
                    var currentCharacter = _expression[_currentIndex];
                    if (currentCharacter == AndOperatorCharacter)
                        ExpectTwoCharacterOperator(AndOperatorCharacter, PreprocessorTokenType.AndOperator);
                    else if (currentCharacter == OrOperatorCharacter)
                        ExpectTwoCharacterOperator(OrOperatorCharacter, PreprocessorTokenType.OrOperator);
                    else if (currentCharacter == NotOperatorCharacter)
                        AddSingleCharacterToken(PreprocessorTokenType.NotOperator);
                    else if (currentCharacter == OpenBracketCharacter)
                        AddSingleCharacterToken(PreprocessorTokenType.OpenBracket);
                    else if (currentCharacter == CloseBracketCharacter)
                        AddSingleCharacterToken(PreprocessorTokenType.CloseBracket);
                    else if (char.IsLetter(currentCharacter) || currentCharacter == Underscore)
                        ExpectSymbol();
                }

                if (!_tokenListBuilder.TryBuild(out var tokenList, out var errorMessage))
                    ThrowErrorFromBuilder(_expression, errorMessage);
                return tokenList;
            }

            private void ExpectSymbol()
            {
                var startIndex = _currentIndex;
                while (++_currentIndex < _expression.Length)
                {
                    var currentCharacter = _expression[_currentIndex];
                    if (char.IsWhiteSpace(currentCharacter) ||
                        currentCharacter == OrOperatorCharacter ||
                        currentCharacter == AndOperatorCharacter ||
                        currentCharacter == CloseBracketCharacter)
                        break;
                    if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == Underscore)
                        continue;

                    throw new InvalidPreprocessorExpressionException($"The expression \"{_expression.ToString()}\" contains an invalid Symbol character at position {_currentIndex}.");
                }

                if (!_tokenListBuilder.TryAdd(new PreprocessorExpressionToken(PreprocessorTokenType.Symbol, _expression.Slice(startIndex, _currentIndex - startIndex).ToString()), out var errorMessage))
                    ThrowErrorFromBuilder(_expression, errorMessage);
            }

            private void ExpectTwoCharacterOperator(char operatorCharacter, PreprocessorTokenType tokenType)
            {
                if (_expression.Length - _currentIndex - 1 < 2)
                    throw new InvalidPreprocessorExpressionException($"The expression \"{_expression.ToString()}\" contains an invalid use of the {operatorCharacter.ToOperatorSymbol()} operator at index {_currentIndex}.");

                if (_expression[++_currentIndex] != operatorCharacter)
                    throw new InvalidPreprocessorExpressionException($"The expression \"{_expression.ToString()}\" contains no second operator symbol for {operatorCharacter.ToOperatorSymbol()} at index {_currentIndex}.");

                if (!_tokenListBuilder.TryAdd(new PreprocessorExpressionToken(tokenType), out var errorMessage))
                    ThrowErrorFromBuilder(_expression, errorMessage);
                ++_currentIndex;
            }

            private void AddSingleCharacterToken(PreprocessorTokenType tokenType)
            {
                if (!_tokenListBuilder.TryAdd(new PreprocessorExpressionToken(tokenType), out var errorMessage))
                    ThrowErrorFromBuilder(_expression, errorMessage);
                ++_currentIndex;
            }

            private bool AdvanceToNextNonWhiteSpaceCharacter()
            {
                if (_currentIndex >= _expression.Length)
                    return false;

                while (char.IsWhiteSpace(_expression[_currentIndex]))
                {
                    if (_currentIndex < _expression.Length - 1)
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