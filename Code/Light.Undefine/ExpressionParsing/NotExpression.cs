using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Represents a logical NOT preprocessor expression with a single child expression.
    /// </summary>
    public sealed class NotExpression : PreprocessorExpression
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NotExpression"/>.
        /// </summary>
        /// <param name="expression">The child expression that will be inverted by this NOT expression.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="expression"/> is  null.</exception>
        public NotExpression(PreprocessorExpression expression) => Expression = expression.MustNotBeNull(nameof(expression));

        /// <summary>
        /// Gets the child expression that is inverted by this NOT expression.
        /// </summary>
        public PreprocessorExpression Expression { get; }

        /// <inheritdoc />
        public override bool Evaluate(IEnumerable<string> definedSymbols) => !Expression.Evaluate(definedSymbols);
    }
}