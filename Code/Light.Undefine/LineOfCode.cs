using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;

// ReSharper disable ImpureMethodCallOnReadonlyValueField - ReadOnlySpan<T>.Slice is actually a pure method but ReSharper does not recognize this


namespace Light.Undefine
{
    public readonly ref struct LineOfCode
    {
        public readonly CodeLineType Type;
        public readonly PreprocessorExpression Expression;
        public readonly ReadOnlySpan<char> Span;

        public LineOfCode(CodeLineType type, PreprocessorExpression expression, in ReadOnlySpan<char> span)
        {
            Type = type.MustBeValidEnumValue(nameof(type));
            if (type == CodeLineType.IfDirective ||
                type == CodeLineType.ElseIfDirective)
                expression.MustNotBeNull(nameof(expression), $"{nameof(expression)} must not be null when {nameof(type)} is {nameof(CodeLineType.IfDirective)} or {nameof(CodeLineType.ElseIfDirective)}.");

            Expression = expression;
            Span = span;
        }

        public bool Equals(LineOfCode other) => Type == other.Type &&
                                                Span == other.Span;

        public override string ToString() => Span.ToString();
        public override bool Equals(object obj) => throw new NotSupportedException("Equals and GetHashCode are not supported on ref structs.");
        public override int GetHashCode() => throw new NotSupportedException("Equals and GetHashCode are not supported on ref structs.");

        public static bool operator ==(LineOfCode x, LineOfCode y) => x.Equals(y);
        public static bool operator !=(LineOfCode x, LineOfCode y) => !x.Equals(y);
    }

    public ref struct LineOfCodeParser
    {
        public readonly ReadOnlySpan<char> SourceCode;
        public readonly PreprocessorTokenList.Builder TokenListBuilder;
        private int _currentIndex;
        private int _lineCount;

        public LineOfCodeParser(ReadOnlySpan<char> sourceCode, PreprocessorTokenList.Builder tokenListBuilder = null)
        {
            SourceCode = sourceCode;
            TokenListBuilder = tokenListBuilder ?? PreprocessorTokenList.Builder.CreateDefault();
            _currentIndex = _lineCount = 0;
        }

        

        public bool TryParseNext(out LineOfCode lineOfCode)
        {
            var startIndex = _currentIndex;
            ++_lineCount;

            // Check if there are actually any non-white-space characters on this line of code
            if (!AdvanceToNextNonWhiteSpaceCharacterOnSameLine(out var currentCharacter))
            {
                if (startIndex < _currentIndex && _currentIndex < SourceCode.Length)
                {
                    lineOfCode = new LineOfCode(CodeLineType.SourceCode, null, SourceCode.Slice(startIndex, CalculateSubSpanLength(startIndex)));
                    return true;
                }

                lineOfCode = default;
                return false;
            }

            // If yes, then check if it is a preprocessor directive
            if (currentCharacter != '#')
            {
                // If it is not, then return this line as source code
                AdvanceToEndOfLineOrEndOfSpan();
                lineOfCode = new LineOfCode(CodeLineType.SourceCode, null, SourceCode.Slice(startIndex, CalculateSubSpanLength(startIndex)));
                return true;
            }

            // Parse the preprocessor directive - there can be white space between "#" and the first character
            if (!AdvanceToNextNonWhiteSpaceCharacterOnSameLine(out currentCharacter))
                ThrowInvalidDirective(startIndex);

            // Check if it is an "#if" directive
            if (currentCharacter == 'i')
                return ExpectIfDirective(startIndex, out lineOfCode);

            throw new NotImplementedException();
        }

        private bool ExpectIfDirective(int startIndex, out LineOfCode lineOfCode)
        {
            if (!AdvanceCurrentIndex() ||
                GetCurrentCharacter() != 'f' ||
                !AdvanceCurrentIndex())
                ThrowInvalidDirective(startIndex);

            var currentCharacter = GetCurrentCharacter();
            var expressionStartIndex = -1;
            switch (currentCharacter)
            {
                case ' ':
                    expressionStartIndex = _currentIndex + 1;
                    break;
                case '(':
                    expressionStartIndex = _currentIndex;
                    break;
                default:
                    ThrowInvalidDirective(startIndex);
                    break;
            }
            AdvanceToEndOfLineOrEndOfSpan();

            var expression = PreprocessorExpressionParser.Parse(SourceCode.Slice(expressionStartIndex, CalculateSubSpanLength(expressionStartIndex)), TokenListBuilder);
            lineOfCode = new LineOfCode(CodeLineType.IfDirective, expression, SourceCode.Slice(startIndex, CalculateSubSpanLength(startIndex)));
            return true;
        }

        private bool AdvanceToNextNonWhiteSpaceCharacterOnSameLine(out char currentCharacter)
        {
            while (AdvanceCurrentIndex())
            {
                currentCharacter = GetCurrentCharacter();
                if (currentCharacter == Environment.NewLine[0])
                {
                    for (var i = 1; i < Environment.NewLine.Length; ++i)
                    {
                        if (!AdvanceCurrentIndex())
                            goto NoCharacterFound;

                        currentCharacter = GetCurrentCharacter();
                        if (currentCharacter == Environment.NewLine[i])
                            continue;

                        goto CheckForWhiteSpace;
                    }

                    goto NoCharacterFound;
                }
                CheckForWhiteSpace:
                if (!char.IsWhiteSpace(currentCharacter))
                    return true;
            }

            NoCharacterFound:
            currentCharacter = default;
            return false;
        }

        private void AdvanceToEndOfLineOrEndOfSpan()
        {
            BeforeLoop:
            while (AdvanceCurrentIndex())
            {
                if (GetCurrentCharacter() != Environment.NewLine[0])
                    continue;

                for (var i = 1; i < Environment.NewLine.Length; ++i)
                {
                    if (!AdvanceCurrentIndex())
                        return;

                    if (GetCurrentCharacter() != Environment.NewLine[i])
                        goto BeforeLoop;
                }

                return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AdvanceCurrentIndex()
        {
            if (++_currentIndex < SourceCode.Length)
                return true;

            _currentIndex = SourceCode.Length;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char GetCurrentCharacter() => SourceCode[_currentIndex];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateSubSpanLength(int startIndex) => _currentIndex - startIndex + 1;

        private void ThrowInvalidDirective(int startIndex)
        {
            _currentIndex = startIndex;
            AdvanceToEndOfLineOrEndOfSpan();
            var expression = SourceCode.Slice(startIndex, CalculateSubSpanLength(startIndex));
            throw new InvalidPreprocessorDirectiveExpression($"\"{expression.ToString()}\" on line {_lineCount} cannot be parsed to a valid preprocessor directive.");
        }
    }
}