using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Undefine.ExpressionParsing
{
    public sealed class NotExpression : PreprocessorExpression
    {
        public NotExpression(PreprocessorExpression expression) => Expression = expression.MustNotBeNull(nameof(expression));

        public PreprocessorExpression Expression { get; }
        public override bool Evaluate(IEnumerable<string> definedSymbols) => !Expression.Evaluate(definedSymbols);
    }
}