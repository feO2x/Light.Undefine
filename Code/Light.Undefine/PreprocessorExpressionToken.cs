using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;

namespace Light.Undefine
{
    public readonly struct PreprocessorExpressionToken : IEquatable<PreprocessorExpressionToken>
    {
        public readonly PreprocessorTokenType Type;
        public readonly string SymbolText;

        public PreprocessorExpressionToken(PreprocessorTokenType type, string symbolText = null)
        {
            Type = type;
            if (type == PreprocessorTokenType.Symbol)
                symbolText.MustNotBeNullOrWhiteSpace(nameof(symbolText), $"The symbolText must be set when type {nameof(PreprocessorTokenType.Symbol)} is used.");
            SymbolText = symbolText;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PreprocessorExpressionToken other) => 
            other.Type == Type && (Type != PreprocessorTokenType.Symbol || SymbolText.Equals(other.SymbolText));

        public override bool Equals(object other) => other is PreprocessorExpressionToken token && Equals(token);

        public override int GetHashCode()
        {
            if (Type != PreprocessorTokenType.Symbol)
                return (int) Type;

            return SymbolText.GetHashCode() ^ (int) Type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PreprocessorExpressionToken x, PreprocessorExpressionToken y) => x.Equals(y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PreprocessorExpressionToken x, PreprocessorExpressionToken y) => !x.Equals(y);

        public override string ToString() => Type != PreprocessorTokenType.Symbol ? Type.GetStringRepresentationOfOperatorOrBracket() : SymbolText;
    }
}