using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.Undefine.ExpressionParsing
{
    public readonly struct PreprocessorExpressionTokenList : IReadOnlyList<PreprocessorExpressionToken>
    {
        private readonly PreprocessorExpressionToken[] _internalArray;
        public readonly int Count;
        private readonly int _from;

        private PreprocessorExpressionTokenList(PreprocessorExpressionToken[] internalArray, int from, int count)
        {
            _internalArray = internalArray;
            _from = from;
            Count = count;
        }

        public PreprocessorExpressionTokenList Slice(int from)
        {
            from.MustBeGreaterThanOrEqualTo(0, nameof(from)).MustBeLessThan(Count, nameof(from));
            return new PreprocessorExpressionTokenList(_internalArray, from + _from, Count - from);
        }

        public PreprocessorExpressionTokenList Slice(int from, int exclusiveTo)
        {
            from.MustBeGreaterThanOrEqualTo(0, nameof(from)).MustBeLessThan(Count, nameof(from));
            exclusiveTo.MustBeLessThanOrEqualTo(Count, nameof(exclusiveTo));

            return new PreprocessorExpressionTokenList(_internalArray, from + _from, exclusiveTo - from);
        }

        public Enumerator GetEnumerator() => new Enumerator(_internalArray, _from, Count);

        public OperatorAnalysisResult AnalyzeComplexExpression()
        {
            var topLevelOperator = default(PreprocessorExpressionToken);
            var operatorIndex = -1;
            var operatorBracketLevel = int.MaxValue;
            
            var isFirstTokenABracket = false;
            var isLastTokenABracket = false;
            var canOuterBracketsBeIgnored = false;

            var exclusiveTo = _from + Count;
            var bracketLevel = 0;
            for (var i = _from; i < exclusiveTo; ++i)
            {
                var currentToken = _internalArray[i];
                if (currentToken.Type == PreprocessorExpressionTokenType.OpenBracket)
                {
                    ++bracketLevel;
                    if (i == _from)
                        isFirstTokenABracket = true;
                    continue;
                }

                if (currentToken.Type == PreprocessorExpressionTokenType.CloseBracket)
                {
                    --bracketLevel;
                    if (i == exclusiveTo - 1)
                        isLastTokenABracket = true;
                    else if (bracketLevel == 0)
                        canOuterBracketsBeIgnored = true;
                    continue;
                }

                if (currentToken.Type == PreprocessorExpressionTokenType.Symbol ||
                    bracketLevel > operatorBracketLevel ||
                    bracketLevel == operatorBracketLevel && currentToken.Type <= topLevelOperator.Type)
                    continue;

                topLevelOperator = currentToken;
                operatorIndex = i - _from;
                operatorBracketLevel = bracketLevel;
            }

            return new OperatorAnalysisResult(topLevelOperator, operatorIndex, isFirstTokenABracket && isLastTokenABracket && !canOuterBracketsBeIgnored);
        }

        public readonly struct OperatorAnalysisResult
        {
            public readonly PreprocessorExpressionToken TopLevelOperator;
            public readonly int TopLevelOperatorIndex;
            public readonly bool CanOuterBracketsBeIgnored;

            public OperatorAnalysisResult(PreprocessorExpressionToken topLevelOperator, int topLevelOperatorIndex, bool canOuterBracketsBeIgnored)
            {
                TopLevelOperator = topLevelOperator;
                TopLevelOperatorIndex = topLevelOperatorIndex;
                CanOuterBracketsBeIgnored = canOuterBracketsBeIgnored;
            }
        }

        IEnumerator<PreprocessorExpressionToken> IEnumerable<PreprocessorExpressionToken>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IReadOnlyCollection<PreprocessorExpressionToken>.Count => Count;

        public PreprocessorExpressionToken this[int index]
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
            private readonly PreprocessorExpressionToken[] _internalArray;
            private int _currentIndex;
            private PreprocessorExpressionToken _previousToken;
            private int _bracketBalance;

            public Builder(PreprocessorExpressionToken[] internalArray)
            {
                _internalArray = internalArray.MustNotBeNullOrEmpty(nameof(internalArray));
                _currentIndex = 0;
                _previousToken = default;
            }

            public static Builder CreateDefault() => new Builder(new PreprocessorExpressionToken[32]);

            public Builder Reset()
            {
                for (var i = 0; i < _currentIndex; ++i)
                    _internalArray[i] = default;
                _currentIndex = 0;
                _previousToken = default;
                _bracketBalance = 0;
                return this;
            }

            public bool TryAdd(PreprocessorExpressionToken token, out string errorMessage)
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

            public bool TryBuild(out PreprocessorExpressionTokenList tokenList, out string errorMessage)
            {
                if (_currentIndex == 0)
                {
                    errorMessage = "The expression is empty.";
                    tokenList = default;
                    return false;
                }

                if (_bracketBalance != 0)
                {
                    errorMessage = $"There are {_bracketBalance} more open than closing brackets.";
                    tokenList = default;
                    return false;
                }

                if (_previousToken.Type != PreprocessorExpressionTokenType.Symbol &&
                    _previousToken.Type != PreprocessorExpressionTokenType.CloseBracket)
                {
                    errorMessage = "The preprocessor expression is not finished.";
                    tokenList = default;
                    return false;
                }

                if (_currentIndex == 1 && _previousToken.Type != PreprocessorExpressionTokenType.Symbol)
                {
                    errorMessage = "An expression with a single token must always be a preprocessor symbol.";
                    tokenList = default;
                    return false;
                }

                tokenList = new PreprocessorExpressionTokenList(_internalArray, 0, _currentIndex);
                errorMessage = null;
                return true;
            }

            private bool CheckIfTokenIsValid(in PreprocessorExpressionToken token, out string errorMessage)
            {
                if (_previousToken == default &&
                    token.Type != PreprocessorExpressionTokenType.OpenBracket &&
                    token.Type != PreprocessorExpressionTokenType.NotOperator &&
                    token.Type != PreprocessorExpressionTokenType.Symbol)
                {
                    errorMessage = $"Expected symbol, open bracket, or not operator, but actually got {token}.";
                    return false;
                }

                if (_previousToken.Type == PreprocessorExpressionTokenType.Symbol &&
                    token.Type != PreprocessorExpressionTokenType.CloseBracket &&
                    token.Type != PreprocessorExpressionTokenType.AndOperator &&
                    token.Type != PreprocessorExpressionTokenType.OrOperator)
                {
                    errorMessage = $"Expected Close Bracket, And operator, or Or operator after {_previousToken}, but actually got {token}";
                    return false;
                }

                if (_previousToken.Type == PreprocessorExpressionTokenType.NotOperator &&
                    token.Type != PreprocessorExpressionTokenType.OpenBracket &&
                    token.Type != PreprocessorExpressionTokenType.Symbol)
                {
                    errorMessage = $"Expected Symbol or Open Bracket after {_previousToken}, but actually got {token}.";
                    return false;
                }

                if (_previousToken.Type == PreprocessorExpressionTokenType.OpenBracket &&
                    token.Type != PreprocessorExpressionTokenType.Symbol &&
                    token.Type != PreprocessorExpressionTokenType.NotOperator &&
                    token.Type != PreprocessorExpressionTokenType.OpenBracket)
                {
                    errorMessage = $"Expected Symbol, or Not Operator, or Open Bracket after {_previousToken}, but actually got {token}.";
                    return false;
                }

                if (_previousToken.Type == PreprocessorExpressionTokenType.CloseBracket &&
                    token.Type != PreprocessorExpressionTokenType.OrOperator &&
                    token.Type != PreprocessorExpressionTokenType.AndOperator &&
                    token.Type != PreprocessorExpressionTokenType.CloseBracket)
                {
                    errorMessage = $"Expected Or Operator, or And Operator, or Close Bracket after {_previousToken}, but actually got {token}.";
                    return false;
                }

                if ((_previousToken.Type == PreprocessorExpressionTokenType.AndOperator ||
                     _previousToken.Type == PreprocessorExpressionTokenType.OrOperator) &&
                    token.Type != PreprocessorExpressionTokenType.Symbol &&
                    token.Type != PreprocessorExpressionTokenType.OpenBracket &&
                    token.Type != PreprocessorExpressionTokenType.NotOperator)
                {
                    errorMessage = $"Expected Symbol, or Open Bracket, or Not Operator after {_previousToken}, but actually got {token}.";
                    return false;
                }

                errorMessage = null;
                return true;
            }

            private bool CheckBracketBalance(in PreprocessorExpressionToken token, out string errorMessage)
            {
                if (token.Type == PreprocessorExpressionTokenType.OpenBracket)
                    ++_bracketBalance;

                else if (token.Type == PreprocessorExpressionTokenType.CloseBracket)
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

        public struct Enumerator : IEnumerator<PreprocessorExpressionToken>
        {
            private readonly PreprocessorExpressionToken[] _internalArray;
            private readonly int _from;
            private readonly int _exclusiveTo;
            private int _currentIndex;

            public Enumerator(PreprocessorExpressionToken[] internalArray, int from, int count)
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

            public PreprocessorExpressionToken Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
    }
}