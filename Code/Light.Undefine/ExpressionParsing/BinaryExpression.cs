using Light.GuardClauses;

namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Represents a binary operator expression that has a left-hand and right-hand child expression.
    /// </summary>
    public abstract class BinaryExpression : PreprocessorExpression
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BinaryExpression"/>.
        /// </summary>
        /// <param name="left">The left-hand child expression.</param>
        /// <param name="operator">The string that represents the operator.</param>
        /// <param name="right">The right-hand child expression.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when any of the parameter is null.</exception>
        /// <exception cref="GuardClauses.Exceptions.EmptyStringException">Thrown when <paramref name="operator"/> is an empty string.</exception>
        /// <exception cref="GuardClauses.Exceptions.WhiteSpaceStringException">Thrown when <paramref name="operator"/> contains only white space.</exception>
        protected BinaryExpression(PreprocessorExpression left, string @operator, PreprocessorExpression right)
        {
            Left = left.MustNotBeNull(nameof(left));
            Operator = @operator.MustNotBeNullOrWhiteSpace(nameof(@operator));
            Right = right.MustNotBeNull(nameof(right));
        }

        /// <summary>
        /// Gets the left-hand child expression.
        /// </summary>
        public PreprocessorExpression Left { get; }

        /// <summary>
        /// Gets the string representation of the logical operator of this expression.
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// Gets the right-hand child expression.
        /// </summary>
        public PreprocessorExpression Right { get; }
    }
}