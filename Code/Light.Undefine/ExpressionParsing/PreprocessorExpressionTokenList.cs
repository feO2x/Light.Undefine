using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Light.GuardClauses;
using Light.GuardClauses.FrameworkExtensions;

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Represents a light-weight collection that holds preprocessor expression tokens.
    /// </summary>
    public readonly struct PreprocessorExpressionTokenList : IReadOnlyList<PreprocessorExpressionToken>
    {
        private readonly PreprocessorExpressionToken[] _internalArray;
        /// <summary>
        /// Gets the number of tokens in this collection.
        /// </summary>
        public readonly int Count;
        private readonly int _from;

        private PreprocessorExpressionTokenList(PreprocessorExpressionToken[] internalArray, int from, int count)
        {
            _internalArray = internalArray;
            _from = from;
            Count = count;
        }

        /// <summary>
        /// Slices this collection from the specified index to the end into a new collection.
        /// </summary>
        /// <param name="from">The index that identifies the first element of the new collection.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="from"/> is less than 0, or greater or equal to <see cref="Count"/>.</exception>
        public PreprocessorExpressionTokenList Slice(int from)
        {
            from.MustBeGreaterThanOrEqualTo(0, nameof(from)).MustBeLessThan(Count, nameof(from));
            return new PreprocessorExpressionTokenList(_internalArray, from + _from, Count - from);
        }

        /// <summary>
        /// Slices this collection from the specified <paramref name="from"/> index to the specified <paramref name="exclusiveTo"/> index into a new collection.
        /// </summary>
        /// <param name="from">The index that identifies the first element of the new collection.</param>
        /// <param name="exclusiveTo">The exclusive index that identifies the last element in the new collection.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="from"/> is less than 0, or greater or equal to <see cref="Count"/>, or when <paramref name="exclusiveTo"/> is not greater than <paramref name="from"/>, or greater then <see cref="Count"/>.</exception>
        public PreprocessorExpressionTokenList Slice(int from, int exclusiveTo)
        {
            from.MustBeGreaterThanOrEqualTo(0, nameof(from)).MustBeLessThan(Count, nameof(from));
            exclusiveTo.MustBeLessThanOrEqualTo(Count, nameof(exclusiveTo)).MustBeGreaterThan(from, nameof(exclusiveTo));

            return new PreprocessorExpressionTokenList(_internalArray, from + _from, exclusiveTo - from);
        }

        /// <summary>
        /// Gets the enumerator to iterate over this collection.
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(_internalArray, _from, Count);

        /// <summary>
        /// Analyzes the tokens in this collection and identifies the top level operator as well as its index and whether
        /// the first and the last token represent brackets that can be ignored when parsing the tokens.
        /// </summary>
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

        /// <summary>
        /// Represents the analysis result of the <see cref="PreprocessorExpressionTokenList.AnalyzeComplexExpression"/> method.
        /// </summary>
        public readonly struct OperatorAnalysisResult
        {
            /// <summary>
            /// Gets the token that represents the top-level operator.
            /// </summary>
            public readonly PreprocessorExpressionToken TopLevelOperator;

            /// <summary>
            /// Gets the index of the top level operator.
            /// </summary>
            public readonly int TopLevelOperatorIndex;

            /// <summary>
            /// Gets the value indicating whether the token list has outer brackets that can be ignored.
            /// </summary>
            public readonly bool CanOuterBracketsBeIgnored;

            /// <summary>
            /// Initializes a new instance of <see cref="OperatorAnalysisResult"/>.
            /// </summary>
            public OperatorAnalysisResult(PreprocessorExpressionToken topLevelOperator, int topLevelOperatorIndex, bool canOuterBracketsBeIgnored)
            {
                TopLevelOperator = topLevelOperator;
                TopLevelOperatorIndex = topLevelOperatorIndex;
                CanOuterBracketsBeIgnored = canOuterBracketsBeIgnored;
            }
        }

        /// <inheritdoc />
        IEnumerator<PreprocessorExpressionToken> IEnumerable<PreprocessorExpressionToken>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        int IReadOnlyCollection<PreprocessorExpressionToken>.Count => Count;

        /// <inheritdoc />
        public PreprocessorExpressionToken this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException($"Index must be between 0 and {Count}, but it actually is {index}.");
                return _internalArray[index + _from];
            }
        }

        /// <inheritdoc />
        public override string ToString() => new StringBuilder().AppendItems(this).ToString();

        /// <summary>
        /// Represents a builder that can be used to create <see cref="PreprocessorExpressionTokenList"/> instances.
        /// </summary>
        public sealed class Builder
        {
            private readonly PreprocessorExpressionToken[] _internalArray;
            private int _currentIndex;
            private PreprocessorExpressionToken _previousToken;
            private int _bracketBalance;

            /// <summary>
            /// Initializes a new instance of <see cref="Builder"/>.
            /// </summary>
            /// <param name="internalArray">The array that is used to store parsed tokens.</param>
            /// <exception cref="GuardClauses.Exceptions.EmptyCollectionException">Thrown when array has a capacity of 0.</exception>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="internalArray"/> is null.</exception>
            public Builder(PreprocessorExpressionToken[] internalArray)
            {
                _internalArray = internalArray.MustNotBeNullOrEmpty(nameof(internalArray));
                _currentIndex = 0;
                _previousToken = default;
            }

            /// <summary>
            /// Creates a new instance of <see cref="Builder"/> with a new default internal array with 32 slots.
            /// </summary>
            public static Builder CreateDefault() => new Builder(new PreprocessorExpressionToken[32]);

            /// <summary>
            /// Resets the builder so that a new expression can be parsed to tokens.
            /// </summary>
            public Builder Reset()
            {
                for (var i = 0; i < _currentIndex; ++i)
                    _internalArray[i] = default;
                _currentIndex = 0;
                _previousToken = default;
                _bracketBalance = 0;
                return this;
            }

            /// <summary>
            /// Tries to add the specified token to the existing list of tokens.
            /// </summary>
            /// <param name="token">The token to be added.</param>
            /// <param name="errorMessage">The error message is assigned when this method returns false.</param>
            /// <returns>True when the token could successfully be added, otherwise false.</returns>
            /// <exception cref="GuardClauses.Exceptions.ArgumentDefaultException">Thrown when an invalid token is passed in (initialized via default).</exception>
            /// <exception cref="InvalidOperationException">Thrown when the internal array is full and additional tokens cannot be appended.</exception>
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

            /// <summary>
            /// Tries to build a <see cref="PreprocessorExpressionTokenList"/> from the given tokens.
            /// </summary>
            /// <param name="tokenList">The built token list will be assigned to this value when all validation checks passed successfully.</param>
            /// <param name="errorMessage">The error message is assigned when this method returns false.</param>
            /// <returns>True when the token list could be built successfully, else false.</returns>
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

        /// <summary>
        /// Represents the enumerator for the <see cref="PreprocessorExpressionTokenList"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<PreprocessorExpressionToken>
        {
            private readonly PreprocessorExpressionToken[] _internalArray;
            private readonly int _from;
            private readonly int _exclusiveTo;
            private int _currentIndex;

            /// <summary>
            /// Initializes a new instance of <see cref="Enumerator"/>.
            /// </summary>
            public Enumerator(PreprocessorExpressionToken[] internalArray, int from, int count)
            {
                _internalArray = internalArray;
                _from = from;
                _exclusiveTo = from + count;
                Current = default;
                _currentIndex = from - 1;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_currentIndex + 1 >= _exclusiveTo)
                    return false;

                Current = _internalArray[++_currentIndex];
                return true;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _currentIndex = _from - 1;
                Current = default;
            }

            /// <inheritdoc />
            public PreprocessorExpressionToken Current { get; private set; }

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <summary>
            /// Does nothing because this enumerator does not need to be disposed.
            /// </summary>
            public void Dispose() { }
        }
    }
}