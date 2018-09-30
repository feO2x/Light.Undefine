﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.Undefine
{
    public readonly struct PreprocessorTokenList : IReadOnlyList<PreprocessorToken>
    {
        private readonly PreprocessorToken[] _internalArray;
        public readonly int Count;
        private readonly int _from;

        private PreprocessorTokenList(PreprocessorToken[] internalArray, int from, int count)
        {
            _internalArray = internalArray;
            _from = from;
            Count = count;
        }

        public PreprocessorTokenList Slice(int from) => new PreprocessorTokenList(_internalArray, from, Count - from);

        public PreprocessorTokenList Slice(int from, int to) => new PreprocessorTokenList(_internalArray, from, to - from);

        public Enumerator GetEnumerator() => new Enumerator(_internalArray, _from, Count);

        public (PreprocessorToken, int) FindTopLevelOperator()
        {
            var topLevelOperator = default(PreprocessorToken);
            var operatorIndex = -1;
            var bracketLevel = 0;
            for (var i = _from; i < Count; ++i)
            {
                var currentToken = _internalArray[i];
                if (currentToken.Type == PreprocessorTokenType.OpenBracket)
                {
                    ++bracketLevel;
                    continue;
                }

                if (currentToken.Type == PreprocessorTokenType.CloseBracket)
                {
                    --bracketLevel;
                    continue;
                }

                if (currentToken.Type == PreprocessorTokenType.Symbol)
                    continue;

                if (currentToken.Type > topLevelOperator.Type)
                {
                    topLevelOperator = currentToken;
                    operatorIndex = i;
                }
            }

            return (topLevelOperator, operatorIndex);
        }

        IEnumerator<PreprocessorToken> IEnumerable<PreprocessorToken>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IReadOnlyCollection<PreprocessorToken>.Count => Count;

        public PreprocessorToken this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException($"Index must be between 0 and {Count}, but it actually is {index}.");
                return _internalArray[index + _from];
            }
        }

        public override string ToString() => new StringBuilder().AppendItems(this).ToString();

        public sealed class Builder
        {
            private readonly PreprocessorToken[] _internalArray;
            private int _currentIndex;
            private PreprocessorToken _previousToken;
            private int _bracketBalance;

            public Builder(PreprocessorToken[] internalArray)
            {
                _internalArray = internalArray.MustNotBeNullOrEmpty(nameof(internalArray));
                _currentIndex = 0;
                _previousToken = default;
            }

            public static Builder CreateDefault() => new Builder(new PreprocessorToken[32]);

            public void Reset()
            {
                for (var i = 0; i < _currentIndex; ++i)
                    _internalArray[i] = default;
                _currentIndex = 0;
            }

            public bool TryAdd(PreprocessorToken token, out string errorMessage)
            {
                token.MustNotBeDefault(nameof(token));
                Check.InvalidOperation(_currentIndex == _internalArray.Length, "The capacity of the internal array is reached. Please use a larger array.");
                if (!CheckIfTokenIsValid(token, out errorMessage))
                    return false;

                if (!CheckBracketBalance(token, out errorMessage))
                    return false;
                _internalArray[_currentIndex++] = _previousToken = token;
                return true;
            }

            public bool TryBuild(out PreprocessorTokenList tokenList, out string errorMessage)
            {
                if (_currentIndex == 0)
                {
                    errorMessage = "Nothing was parsed.";
                    tokenList = default;
                    return false;
                }

                if (_bracketBalance != 0)
                {
                    errorMessage = $"There are {_bracketBalance} more open than closing brackets.";
                    tokenList = default;
                    return false;
                }

                if (_previousToken.Type != PreprocessorTokenType.Symbol &&
                    _previousToken.Type != PreprocessorTokenType.CloseBracket)
                {
                    errorMessage = "The preprocessor expression is not finished.";
                    tokenList = default;
                    return false;
                }

                if (_currentIndex == 1 && _previousToken.Type != PreprocessorTokenType.Symbol)
                {
                    errorMessage = "An expression with a single token must always be a preprocessor symbol.";
                    tokenList = default;
                    return false;
                }

                tokenList = new PreprocessorTokenList(_internalArray, 0, _currentIndex);
                errorMessage = null;
                return true;
            }

            private bool CheckIfTokenIsValid(in PreprocessorToken token, out string errorMessage)
            {
                if (_previousToken == default &&
                    token.Type != PreprocessorTokenType.OpenBracket &&
                    token.Type != PreprocessorTokenType.NotOperator &&
                    token.Type != PreprocessorTokenType.Symbol)
                {
                    errorMessage = $"Expected symbol, open bracket, or not operator, but actually got {token}.";
                    return false;
                }

                if (_previousToken.Type == PreprocessorTokenType.Symbol &&
                    token.Type != PreprocessorTokenType.CloseBracket &&
                    token.Type != PreprocessorTokenType.AndOperator &&
                    token.Type != PreprocessorTokenType.OrOperator)
                {
                    errorMessage = $"Expected Close Bracket, And operator, or Or operator after {_previousToken}, but actually got {token}";
                    return false;
                }

                if (_previousToken.Type == PreprocessorTokenType.NotOperator &&
                    token.Type != PreprocessorTokenType.OpenBracket &&
                    token.Type != PreprocessorTokenType.Symbol)
                {
                    errorMessage = $"Expected Symbol or Open Bracket after {_previousToken}, but actually got {token}.";
                    return false;
                }

                if (_previousToken.Type == PreprocessorTokenType.OpenBracket &&
                    token.Type != PreprocessorTokenType.Symbol &&
                    token.Type != PreprocessorTokenType.NotOperator &&
                    token.Type != PreprocessorTokenType.OpenBracket)
                {
                    errorMessage = $"Expected Symbol, or Not Operator, or Open Bracket after {_previousToken}, but actually got {token}.";
                    return false;
                }

                if (_previousToken.Type == PreprocessorTokenType.CloseBracket &&
                    token.Type != PreprocessorTokenType.OrOperator &&
                    token.Type != PreprocessorTokenType.AndOperator)
                {
                    errorMessage = $"Expected Or Operator or And Operator after {_previousToken}, but actually got {token}.";
                    return false;
                }

                if ((_previousToken.Type == PreprocessorTokenType.AndOperator ||
                     _previousToken.Type == PreprocessorTokenType.OrOperator) &&
                    token.Type != PreprocessorTokenType.Symbol &&
                    token.Type != PreprocessorTokenType.OpenBracket &&
                    token.Type != PreprocessorTokenType.NotOperator)
                {
                    errorMessage = $"Expected Symbol, or Open Bracket, or Not Operator after {_previousToken}, but actually got {token}.";
                    return false;
                }

                errorMessage = null;
                return true;
            }

            private bool CheckBracketBalance(in PreprocessorToken token, out string errorMessage)
            {
                if (token.Type == PreprocessorTokenType.OpenBracket)
                    ++_bracketBalance;

                else if (token.Type == PreprocessorTokenType.CloseBracket)
                {
                    if (--_bracketBalance < 0)
                    {
                        errorMessage = "Unexpected Close Bracket.";
                        return false;
                    }
                }

                errorMessage = null;
                return true;
            }
        }

        public struct Enumerator : IEnumerator<PreprocessorToken>
        {
            private readonly PreprocessorToken[] _internalArray;
            private readonly int _from;
            private readonly int _exclusiveTo;
            private int _currentIndex;

            public Enumerator(PreprocessorToken[] internalArray, int from, int count)
            {
                _internalArray = internalArray;
                _from = from;
                _exclusiveTo = from + count;
                Current = default;
                _currentIndex = from - 1;
            }

            public bool MoveNext()
            {
                if (_currentIndex + 1 >= _exclusiveTo)
                    return false;

                Current = _internalArray[++_currentIndex];
                return true;
            }

            public void Reset()
            {
                _currentIndex = _from - 1;
                Current = default;
            }

            public PreprocessorToken Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
    }
}