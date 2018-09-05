using System;
using System.Collections.Generic;
using System.Text;

namespace Light.Undefine
{
    public abstract class PreprocessorExpression
    {
        public abstract bool Evaluate(IEnumerable<string> definedSymbols);
    }
}
