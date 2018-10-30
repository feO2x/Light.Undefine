using System.Collections.Generic;

// ReSharper disable PossibleMultipleEnumeration

namespace Light.Undefine.ExpressionParsing
{
    public sealed class AndExpression : BinaryExpression
    {
        public AndExpression(PreprocessorExpression left, PreprocessorExpression right) : base(left, Operators.AndOperator, right) { }

        public override bool Evaluate(IEnumerable<string> definedSymbols) =>
            Left.Evaluate(definedSymbols) && Right.Evaluate(definedSymbols);
    }
}