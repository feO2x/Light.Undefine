using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;

namespace Light.Undefine.ExpressionParsing
{
    public readonly struct PreprocessorExpressionToken : IEquatable<PreprocessorExpressionToken>
    {
        public readonly PreprocessorExpressionTokenType Type;
        public readonly string SymbolText;

        public PreprocessorExpressionToken(PreprocessorExpressionTokenType type, string symbolText = null)
        {
            Type = type;
            if (type == PreprocessorExpressionTokenType.Symbol)
                symbolText.MustNotBeNullOrWhiteSpace(nameof(symbolText), $"The symbolText must be set when type {nameof(PreprocessorExpressionTokenType.Symbol)} is used.");
            SymbolText = symbolText;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PreprocessorExpressionToken other) => 
            other.Type == Type && (Type != PreprocessorExpressionTokenType.Symbol || SymbolText.Equals(other.SymbolText));

        public override bool Equals(object other) => other is PreprocessorExpressionToken token && Equals(token);

        public override int GetHashCode()
        {
            if (Type != PreprocessorExpressionTokenType.Symbol)
                return (int) Type;

            return SymbolText.GetHashCode() ^ (int) Type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PreprocessorExpressionToken x, PreprocessorExpressionToken y) => x.Equals(y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PreprocessorExpressionToken x, PreprocessorExpressionToken y) => !x.Equals(y);

        public override string ToString() => Type != PreprocessorExpressionTokenType.Symbol ? Type.GetStringRepresentationOfOperatorOrBracket() : SymbolText;
    }
}