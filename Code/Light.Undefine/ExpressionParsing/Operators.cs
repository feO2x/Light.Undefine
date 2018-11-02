namespace Light.Undefine.ExpressionParsing
{
    /// <summary>
    /// Contains string representations of the different preprocessor operators.
    /// </summary>
    public static class Operators
    {
        /// <summary>
        /// Gets the logical NOT operator.
        /// </summary>
        public const string NotOperator = "!";

        /// <summary>
        /// Gets the logical AND operator.
        /// </summary>
        public const string AndOperator = "&&";

        /// <summary>
        /// Gets the logical OR operator.
        /// </summary>
        public const string OrOperator = "||";

        /// <summary>
        /// Gets the Open-Bracket that defines the execution order of complex preprocessor expressions.
        /// </summary>
        public const string OpenBracket = "(";

        /// <summary>
        /// Gets the Close-Bracket that defines the execution order of complex preprocessor expressions.
        /// </summary>
        public const string CloseBracket = ")";
    }
}