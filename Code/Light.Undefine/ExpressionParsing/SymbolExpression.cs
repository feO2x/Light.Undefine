using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Represents a symbol preprocessor expression.
    /// </summary>
    public sealed class SymbolExpression : PreprocessorExpression
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SymbolExpression"/>.
        /// </summary>
        /// <param name="symbol">The symbol that this expression represents.</param>
        /// <exception cref="GuardClauses.Exceptions.WhiteSpaceStringException">Thrown when <paramref name="symbol"/> contains only white space.</exception>
        /// <exception cref="GuardClauses.Exceptions.EmptyStringException">Thrown when <paramref name="symbol"/> is an empty string.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="symbol"/> is null.</exception>
        public SymbolExpression(string symbol) => 
            Symbol = symbol.MustNotBeNullOrWhiteSpace(nameof(symbol));

        /// <summary>
        /// Gets the symbol of this expression.
        /// </summary>
        public string Symbol { get; }

        /// <inheritdoc />
        public override bool Evaluate(IEnumerable<string> definedSymbols) => 
            definedSymbols is ICollection<string> collection ? collection.Contains(Symbol) : definedSymbols.Contains(Symbol);

        /// <inheritdoc />
        public override string ToString() => Symbol;
    }
}