using System.Collections.Generic;

// ReSharper disable PossibleMultipleEnumeration

namespace Light.Undefine.ExpressionParsing
{
    public sealed class OrExpression : BinaryExpression
    {
        public OrExpression(PreprocessorExpression left, PreprocessorExpression right) : base(left, Operators.OrOperator, right) { }

        public override bool Evaluate(IEnumerable<string> definedSymbols) => 
            Left.Evaluate(definedSymbols) || Right.Evaluate(definedSymbols);
    }
}