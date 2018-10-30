using System.Collections.Generic;

namespace Light.Undefine.ExpressionParsing
{
    public abstract class PreprocessorExpression
    {
        public abstract bool Evaluate(IEnumerable<string> definedSymbols);
    }
}