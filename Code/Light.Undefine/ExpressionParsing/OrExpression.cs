using System.Collections.Generic;

// ReSharper disable PossibleMultipleEnumeration

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Represents a logical OR preprocessor expression that contains two child expressions (left-hand and right-hand).
    /// </summary>
    public sealed class OrExpression : BinaryExpression
    {
        /// <summary>
        /// Initializes a new instance of <see cref="OrExpression"/>.
        /// </summary>
        /// <param name="left">The left-hand child expression.</param>
        /// <param name="right">The right-hand child expression.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="left"/> or <paramref name="right"/> are null.</exception>
        public OrExpression(PreprocessorExpression left, PreprocessorExpression right) : base(left, Operators.OrOperator, right) { }

        /// <inheritdoc />
        public override bool Evaluate(IEnumerable<string> definedSymbols) => 
            Left.Evaluate(definedSymbols) || Right.Evaluate(definedSymbols);
    }
}