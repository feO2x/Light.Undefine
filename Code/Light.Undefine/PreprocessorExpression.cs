using System.Collections.Generic;

namespace Light.Undefine
{
    public abstract class PreprocessorExpression
    {
        public abstract bool Evaluate(IEnumerable<string> definedSymbols);
    }
}