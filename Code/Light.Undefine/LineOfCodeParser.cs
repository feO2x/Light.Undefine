using System;
using System.Runtime.CompilerServices;

// ReSharper disable ImpureMethodCallOnReadonlyValueField - ReadOnlySpan<T>.Slice is actually a pure method but ReSharper does not recognize this

namespace Light.Undefine
{
    public ref struct LineOfCodeParser
    {
        public readonly ReadOnlySpan<char> SourceCode;
        public readonly PreprocessorExpressionTokenList.Builder TokenListBuilder;
        private int _currentIndex;
        private int _lineCount;

        public LineOfCodeParser(ReadOnlySpan<char> sourceCode, PreprocessorExpressionTokenList.Builder tokenListBuilder = null)
        {
            SourceCode = sourceCode;
            TokenListBuilder = tokenListBuilder ?? PreprocessorExpressionTokenList.Builder.CreateDefault();
            _currentIndex = _lineCount = 0;
        }

        public bool TryParseNext(out LineOfCode lineOfCode)
        {
            var startIndex = _currentIndex;

            // Check if there are actually any non-white-space characters on this line of code
            if (!AdvanceToNextNonWhiteSpaceCharacterOnSameLine(out var currentCharacter))
            {
                if (startIndex < _currentIndex && _currentIndex < SourceCode.Length)
                {
                    ++_lineCount;
                    lineOfCode = CreateLineOfCode(LineOfCodeType.SourceCode, startIndex);
                    return true;
                }

                lineOfCode = default;
                return false;
            }

            ++_lineCount;

            // If yes, then check if it is a preprocessor directive
            if (currentCharacter != '#')
            {
                // If it is not, then return this line as source code
                AdvanceToEndOfLineOrEndOfSpan();
                lineOfCode = CreateLineOfCode(LineOfCodeType.SourceCode, startIndex);
                return true;
            }

            // Parse the preprocessor directive - there can be white space between "#" and the first character
            if (!AdvanceToNextNonWhiteSpaceCharacterOnSameLine(out currentCharacter))
                ThrowInvalidDirective(startIndex);

            // Check if it is an "#if" directive
            if (currentCharacter == 'i')
                return ExpectIfDirective(startIndex, out lineOfCode);

            // ReSharper disable once CommentTypo
            // If not, then it must be an #else, #elif, or #endif directive
            if (currentCharacter != 'e' ||
                !AdvanceCurrentIndex())
                ThrowInvalidDirective(startIndex);

            if (currentCharacter == 'n')
                return ExpectEndIfDirective(startIndex, out lineOfCode);

            if (currentCharacter != 'l' ||
                !AdvanceCurrentIndex())
                ThrowInvalidDirective(startIndex);

            if (currentCharacter == 's')
                return ExpectElseDirective(startIndex, out lineOfCode);

            if (currentCharacter == 'i')
                return ExpectElseIfDirective(startIndex, out lineOfCode);

            ThrowInvalidDirective(startIndex);
            lineOfCode = default;
            return false;
        }

        private bool ExpectIfDirective(int startIndex, out LineOfCode lineOfCode)
        {
            if (GetNextCharacter() != 'f' ||
                !AdvanceCurrentIndex())
                ThrowInvalidDirective(startIndex);

            var expressionStartIndex = GetExpressionStartIndex(startIndex);

            AdvanceToEndOfLineOrEndOfSpan();

            var expression = PreprocessorExpressionParser.Parse(SourceCode.Slice(expressionStartIndex, CalculateSubSpanLength(expressionStartIndex)), TokenListBuilder);
            lineOfCode = CreateLineOfCode(LineOfCodeType.IfDirective, startIndex, expression);
            return true;
        }

        private int GetExpressionStartIndex(int startIndex)
        {
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

            return expressionStartIndex;
        }

        private bool ExpectEndIfDirective(int startIndex, out LineOfCode lineOfCode)
        {
            if (GetNextCharacter() != 'd' ||
                GetNextCharacter() != 'i' ||
                GetNextCharacter() != 'f')
                ThrowInvalidDirective(startIndex);

            AdvanceToEndOfLineOrEndOfSpan();

            lineOfCode = CreateLineOfCode(LineOfCodeType.EndIfDirective, startIndex);
            return true;
        }

        private bool ExpectElseDirective(int startIndex, out LineOfCode lineOfCode)
        {
            if (GetNextCharacter() != 'e')
                ThrowInvalidDirective(startIndex);

            AdvanceToEndOfLineOrEndOfSpan();

            lineOfCode = CreateLineOfCode(LineOfCodeType.ElseDirective, startIndex);
            return true;
        }

        private bool ExpectElseIfDirective(int startIndex, out LineOfCode lineOfCode)
        {
            if (GetNextCharacter() != 'f' ||
                !AdvanceCurrentIndex())
                ThrowInvalidDirective(startIndex);

            var expressionStartIndex = GetExpressionStartIndex(startIndex);

            var expression = PreprocessorExpressionParser.Parse(SourceCode.Slice(expressionStartIndex, CalculateSubSpanLength(expressionStartIndex)), TokenListBuilder);
            lineOfCode = CreateLineOfCode(LineOfCodeType.ElseIfDirective, startIndex, expression);
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
        private char GetNextCharacter() => !AdvanceCurrentIndex() ? default : GetCurrentCharacter();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateSubSpanLength(int startIndex) => _currentIndex - startIndex + 1;

        private void ThrowInvalidDirective(int startIndex)
        {
            _currentIndex = startIndex;
            AdvanceToEndOfLineOrEndOfSpan();
            var expression = SourceCode.Slice(startIndex, CalculateSubSpanLength(startIndex));
            throw new InvalidPreprocessorDirectiveExpression($"\"{expression.ToString()}\" on line {_lineCount} cannot be parsed to a valid preprocessor directive.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LineOfCode CreateLineOfCode(LineOfCodeType type, int startIndex, PreprocessorExpression expression = null) =>
            new LineOfCode(type, expression, SourceCode.Slice(startIndex, CalculateSubSpanLength(startIndex)), startIndex, _currentIndex);
    }
}