using Microsoft.CodeAnalysis;

namespace Terminal.Interactive.Roslyn
{
    public struct CompletionSymbols
    {
        public INamespaceOrTypeSymbol NamespaceOrType { get; }
        public ISymbol Symbol { get; }

        public bool SearchForStatic => Symbol == null;

        public CompletionSymbols(INamespaceOrTypeSymbol namespaceOrType, ISymbol symbol)
        {
            NamespaceOrType = namespaceOrType;
            Symbol = symbol;
        }
    }
}