using System;
using System.Runtime.CompilerServices;
using Light.Undefine.ExpressionParsing;

// ReSharper disable ImpureMethodCallOnReadonlyValueField - ReadOnlySpan<T>.Slice is actually a pure method but ReSharper does not recognize this

namespace Light.Undefine.SourceCodeParsing
{
    /// <summary>
    /// Represents a parser that takes a source file and continuously parses <see cref="LineOfCode"/> instances out of it.
    /// </summary>
    public ref struct LineOfCodeParser
    {
        private const string If = "if";
        private const string ElseIf = "elif";
        private const string Else = "else";
        private const string EndIf = "endif";

        /// <summary>
        /// Gets the underlying source code that is parsed.
        /// </summary>
        public readonly ReadOnlySpan<char> SourceCode;

        /// <summary>
        /// Gets the builder that is used to assemble and validate tokens from preprocessor expressions.
        /// </summary>
        public readonly PreprocessorExpressionTokenList.Builder TokenListBuilder;
        private int _currentIndex;
        private int _currentLineNumber;

        /// <summary>
        /// Initializes a new instance of <see cref="LineOfCodeParser"/>.
        /// </summary>
        /// <param name="sourceCode">The source code that should be parsed.</param>
        /// <param name="tokenListBuilder">builder that is used to assemble and validate tokens from preprocessor expressions (optional).</param>
        public LineOfCodeParser(in ReadOnlySpan<char> sourceCode, PreprocessorExpressionTokenList.Builder tokenListBuilder = null)
        {
            SourceCode = sourceCode;
            TokenListBuilder = tokenListBuilder ?? PreprocessorExpressionTokenList.Builder.CreateDefault();
            _currentLineNumber = 0;
            _currentIndex = 0;
        }

        /// <summary>
        /// Tries to parse the next line of code from the specified source code.
        /// </summary>
        /// <param name="lineOfCode">The line of code instance that was parsed.</param>
        /// <returns>True if parsing was successful, false if no new line of code is available.</returns>
        /// <exception cref="InvalidPreprocessorExpressionException">Thrown when a preprocessor expression of the next line of code is erroneous.</exception>
        /// <exception cref="InvalidPreprocessorDirectiveException">Thrown when a preprocessor directive of the next line of code is erroneous.</exception>
        public bool TryParseNext(out LineOfCode lineOfCode)
        {
            if (!SourceCode.TryGetNextLine(_currentIndex, out var nextLineSpan))
            {
                lineOfCode = default;
                return false;
            }

            var startIndex = _currentIndex;
            _currentIndex += nextLineSpan.Length;
            ++_currentLineNumber;

            var leftTrimmedSpan = nextLineSpan.TrimStart();
            if (leftTrimmedSpan.IsEmpty || leftTrimmedSpan[0] != '#')
            {
                lineOfCode = CreateLineOfCode(LineOfCodeType.SourceCode, startIndex, nextLineSpan);
                return true;
            }

            leftTrimmedSpan = leftTrimmedSpan.Slice(1).TrimStart();

            if (leftTrimmedSpan.StartsWith(If.AsSpan()))
                return CreateLineOfCodeWithExpression(LineOfCodeType.IfDirective,
                                                      startIndex,
                                                      If.Length,
                                                      leftTrimmedSpan,
                                                      nextLineSpan,
                                                      out lineOfCode);

            if (leftTrimmedSpan.StartsWith(ElseIf.AsSpan()))
                return CreateLineOfCodeWithExpression(LineOfCodeType.ElseIfDirective,
                                                      startIndex,
                                                      ElseIf.Length,
                                                      leftTrimmedSpan,
                                                      nextLineSpan,
                                                      out lineOfCode);

            if (leftTrimmedSpan.StartsWith(Else.AsSpan()))
            {
                lineOfCode = CreateLineOfCode(LineOfCodeType.ElseDirective, startIndex, nextLineSpan);
                return true;
            }

            if (leftTrimmedSpan.StartsWith(EndIf.AsSpan()))
            {
                lineOfCode = CreateLineOfCode(LineOfCodeType.EndIfDirective, startIndex, nextLineSpan);
                return true;
            }

            ThrowInvalidDirective(nextLineSpan);
            lineOfCode = default;
            return false;
        }

        private bool CreateLineOfCodeWithExpression(LineOfCodeType type, 
                                                    int startIndex, 
                                                    int expressionStartIndex, 
                                                    in ReadOnlySpan<char> leftTrimmedSpan, 
                                                    in ReadOnlySpan<char> lineSpan, 
                                                    out LineOfCode lineOfCode)
        {
            if (expressionStartIndex >= leftTrimmedSpan.Length)
                ThrowInvalidDirective(lineSpan);

            var expression = PreprocessorExpressionParser.Parse(leftTrimmedSpan.Slice(expressionStartIndex), TokenListBuilder.Reset());
            lineOfCode = CreateLineOfCode(type, startIndex, lineSpan, expression);
            return true;
        }

        private void ThrowInvalidDirective(ReadOnlySpan<char> lineSpan) => 
            throw new InvalidPreprocessorDirectiveException($"\"{lineSpan.ToString()}\" on line {_currentLineNumber} cannot be parsed to a valid preprocessor directive.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LineOfCode CreateLineOfCode(LineOfCodeType type, int startIndex, ReadOnlySpan<char> lineSpan, PreprocessorExpression expression = null) =>
            new LineOfCode(type, expression, lineSpan, _currentLineNumber, startIndex);
    }
}