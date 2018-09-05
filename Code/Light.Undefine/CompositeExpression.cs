using Light.GuardClauses;

namespace Light.Undefine
{
    public abstract class CompositeExpression : PreprocessorExpression
    {
        protected CompositeExpression(PreprocessorExpression left, string @operator, PreprocessorExpression right)
        {
            Left = left.MustNotBeNull(nameof(left));
            Operator = @operator.MustNotBeNullOrWhiteSpace(nameof(@operator));
            Right = right.MustNotBeNull(nameof(right));
        }

        public PreprocessorExpression Left { get; }

        public string Operator { get; }

        public PreprocessorExpression Right { get; }
    }
}