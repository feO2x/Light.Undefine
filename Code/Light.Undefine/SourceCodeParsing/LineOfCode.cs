using System;
using Light.GuardClauses;
using Light.Undefine.ExpressionParsing;

namespace Light.Undefine.SourceCodeParsing
{
    /// <summary>
    /// Represents a line of code encapsulating a span of the source code.
    /// </summary>
    public readonly ref struct LineOfCode
    {
        /// <summary>
        /// Gets the type of the line of code.
        /// </summary>
        public readonly LineOfCodeType Type;

        /// <summary>
        /// Gets the preprocessor expression that is associated with this line of code. Only <see cref="LineOfCodeType.IfDirective"/> and <see cref="LineOfCodeType.ElseIfDirective"/> have an associated expression.
        /// </summary>
        public readonly PreprocessorExpression Expression;

        /// <summary>
        /// Gets the span that represents the actual line of code.
        /// </summary>
        public readonly ReadOnlySpan<char> Span;

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// Gets the index of the first character of this line of code.
        /// </summary>
        public readonly int StartIndex;

        /// <summary>
        /// Initializes a new instance of <see cref="LineOfCode"/>.
        /// </summary>
        /// <param name="type">The type this line of code represents.</param>
        /// <param name="expression">The preprocessor expression that is associated with this line of code. This value must not be null when <paramref name="type"/>
        /// is <see cref="LineOfCodeType.IfDirective"/> or <see cref="LineOfCodeType.ElseIfDirective"/>, otherwise it must be null.</param>
        /// <param name="span">The underlying span that contains the whole line of code.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="startIndex">The index of the first character.</param>
        /// <exception cref="GuardClauses.Exceptions.EnumValueNotDefinedException">Thrown when the specified type is no valid value of <see cref="LineOfCodeType"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see cref="LineOfCodeType.IfDirective"/> or <see cref="LineOfCodeType.ElseIfDirective"/> and <paramref name="expression"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lineNumber"/> or <paramref name="startIndex"/> is equal or less than 0.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is <see cref="LineOfCodeType.SourceCode"/>, or <see cref="LineOfCodeType.ElseDirective"/>, or <see cref="LineOfCodeType.EndIfDirective"/> and <paramref name="expression"/> is not null.</exception>
        public LineOfCode(LineOfCodeType type, PreprocessorExpression expression, in ReadOnlySpan<char> span, int lineNumber, int startIndex)
        {
            Type = type.MustBeValidEnumValue(nameof(type));
            if (type == LineOfCodeType.IfDirective ||
                type == LineOfCodeType.ElseIfDirective)
                expression.MustNotBeNull(nameof(expression), $"{nameof(expression)} must not be null when {nameof(type)} is {nameof(LineOfCodeType.IfDirective)} or {nameof(LineOfCodeType.ElseIfDirective)}.");
            else if (expression != null)
                throw new ArgumentException($"\"{nameof(expression)}\" must be null when parameter \"{nameof(type)}\" has value \"{type}\".", nameof(expression));
                
            Expression = expression;
            Span = span;
            LineNumber = lineNumber.MustBeGreaterThan(0, nameof(lineNumber));
            StartIndex = startIndex.MustBeGreaterThanOrEqualTo(0,  nameof(startIndex));
        }

        /// <summary>
        /// Checks if two lines of code are equal.
        /// </summary>
        public bool Equals(LineOfCode other) => Type == other.Type &&
                                                LineNumber == other.LineNumber &&
                                                StartIndex == other.StartIndex &&
                                                Span == other.Span;

        /// <summary>
        /// Gets the underlying span as a string.
        /// </summary>
        public override string ToString() => Span.ToString();

        /// <summary>
        /// This method always throws a <see cref="NotSupportedException"/> because ref structs cannot be accessed as a reference to <see cref="object"/>.
        /// </summary>
        public override bool Equals(object obj) => throw new NotSupportedException("Equals and GetHashCode are not supported on ref structs.");

        /// <summary>
        /// This method always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        public override int GetHashCode() => throw new NotSupportedException("Equals and GetHashCode are not supported on ref structs.");

        /// <summary>
        /// Checks if two <see cref="LineOfCode"/> instances are equal.
        /// </summary>
        public static bool operator ==(LineOfCode x, LineOfCode y) => x.Equals(y);

        /// <summary>
        /// Checks if two <see cref="LineOfCode"/> instances are not equal.
        /// </summary>
        public static bool operator !=(LineOfCode x, LineOfCode y) => !x.Equals(y);
    }
}