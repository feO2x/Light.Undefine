using System.Collections.Generic;
// ReSharper disable PossibleMultipleEnumeration

namespace Light.Undefine
{
    public sealed class AndExpression : BinaryExpression
    {
        public AndExpression(PreprocessorExpression left, PreprocessorExpression right) : base(left, Operators.And, right) { }

        public override bool Evaluate(IEnumerable<string> definedSymbols) =>
            Left.Evaluate(definedSymbols) && Right.Evaluate(definedSymbols);
    }
}