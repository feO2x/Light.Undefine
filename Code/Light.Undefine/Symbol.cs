using System.Collections.Generic;
using System.Linq;
using Light.GuardClauses;

namespace Light.Undefine
{
    public sealed class Symbol : PreprocessorExpression
    {
        public Symbol(string name) => 
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));

        public string Name { get; }

        public override bool Evaluate(IEnumerable<string> definedSymbols) => 
            definedSymbols is ICollection<string> collection ? collection.Contains(Name) : definedSymbols.Contains(Name);

        public override string ToString() => Name;
    }
}