using System;
using Light.GuardClauses;
using Light.Undefine.ExpressionParsing;

namespace Light.Undefine
{
    public readonly ref struct LineOfCode
    {
        public readonly LineOfCodeType Type;
        public readonly PreprocessorExpression Expression;
        public readonly ReadOnlySpan<char> Span;
        public readonly int StartIndex;
        public readonly int InclusiveEndIndex;

        public LineOfCode(LineOfCodeType type, PreprocessorExpression expression, in ReadOnlySpan<char> span, int startIndex, int inclusiveEndIndex)
        {
            Type = type.MustBeValidEnumValue(nameof(type));
            if (type == LineOfCodeType.IfDirective ||
                type == LineOfCodeType.ElseIfDirective)
                expression.MustNotBeNull(nameof(expression), $"{nameof(expression)} must not be null when {nameof(type)} is {nameof(LineOfCodeType.IfDirective)} or {nameof(LineOfCodeType.ElseIfDirective)}.");

            Expression = expression;
            Span = span;
            StartIndex = startIndex.MustBeGreaterThanOrEqualTo(0,  nameof(startIndex));
            InclusiveEndIndex = inclusiveEndIndex.MustBeGreaterThanOrEqualTo(startIndex, nameof(inclusiveEndIndex));
        }

        public bool Equals(LineOfCode other) => Type == other.Type &&
                                                Span == other.Span;

        public override string ToString() => Span.ToString();
        public override bool Equals(object obj) => throw new NotSupportedException("Equals and GetHashCode are not supported on ref structs.");
        public override int GetHashCode() => throw new NotSupportedException("Equals and GetHashCode are not supported on ref structs.");

        public static bool operator ==(LineOfCode x, LineOfCode y) => x.Equals(y);
        public static bool operator !=(LineOfCode x, LineOfCode y) => !x.Equals(y);
    }
}