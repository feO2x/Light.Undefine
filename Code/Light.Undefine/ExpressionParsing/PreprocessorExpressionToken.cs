using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Represents a token of a preprocessor expression.
    /// </summary>
    public readonly struct PreprocessorExpressionToken : IEquatable<PreprocessorExpressionToken>
    {
        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        public readonly PreprocessorExpressionTokenType Type;

        /// <summary>
        /// Gets the symbol when <see cref="Type"/> is <see cref="PreprocessorExpressionTokenType.Symbol"/>, otherwise null is returned.
        /// </summary>
        public readonly string SymbolText;

        /// <summary>
        /// Initializes a new instance of <see cref="PreprocessorExpressionToken"/>.
        /// </summary>
        /// <param name="type">The type of the token.</param>
        /// <param name="symbolText">The text of the symbol. This value must not be null, empty or white space when <paramref name="type"/> is <see cref="PreprocessorExpressionTokenType.Symbol"/>, otherwise it must be null.</param>
        /// <exception cref="GuardClauses.Exceptions.WhiteSpaceStringException">Thrown when <paramref name="type"/> is <see cref="PreprocessorExpressionTokenType.Symbol"/> and <paramref name="symbolText"/> contains only white space.</exception>
        /// <exception cref="GuardClauses.Exceptions.EmptyStringException">Thrown when <paramref name="type"/> is <see cref="PreprocessorExpressionTokenType.Symbol"/> and <paramref name="symbolText"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see cref="PreprocessorExpressionTokenType.Symbol"/> and <paramref name="symbolText"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not <see cref="PreprocessorExpressionTokenType.Symbol"/> and <paramref name="symbolText"/> is not null.</exception>
        public PreprocessorExpressionToken(PreprocessorExpressionTokenType type, string symbolText = null)
        {
            Type = type;
            if (type == PreprocessorExpressionTokenType.Symbol)
                symbolText.MustNotBeNullOrWhiteSpace(nameof(symbolText), $"The symbolText must be set when type {nameof(PreprocessorExpressionTokenType.Symbol)} is used.");
            else if (symbolText != null)
                throw new ArgumentException($"{nameof(symbolText)} must be null when {nameof(type)} is \"{type}\".", nameof(symbolText));
            SymbolText = symbolText;
        }

        /// <summary>
        /// Checks if the other token is equal to this one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PreprocessorExpressionToken other) => 
            other.Type == Type && (Type != PreprocessorExpressionTokenType.Symbol || SymbolText.Equals(other.SymbolText));

        /// <summary>
        /// Checks if the other token is equal to this one.
        /// </summary>
        public override bool Equals(object other) => other is PreprocessorExpressionToken token && Equals(token);

        /// <summary>
        /// Returns the hash code of this token.
        /// </summary>
        public override int GetHashCode()
        {
            if (Type != PreprocessorExpressionTokenType.Symbol)
                return (int) Type;

            return SymbolText.GetHashCode() ^ (int) Type;
        }

        /// <summary>
        /// Checks if the two tokens are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(PreprocessorExpressionToken x, PreprocessorExpressionToken y) => x.Equals(y);

        /// <summary>
        /// Checks if the two tokens are not equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(PreprocessorExpressionToken x, PreprocessorExpressionToken y) => !x.Equals(y);

        /// <summary>
        /// Returns the string representation of this token.
        /// </summary>
        public override string ToString() => Type != PreprocessorExpressionTokenType.Symbol ? Type.GetStringRepresentationOfOperatorOrBracket() : SymbolText;
    }
}