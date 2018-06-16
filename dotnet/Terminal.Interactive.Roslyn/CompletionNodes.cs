using Microsoft.CodeAnalysis;

namespace Terminal.Interactive.Roslyn
{
    public struct CompletionNodes
    {
        public SyntaxNode Container { get; }
        public SyntaxNode Prefix { get; }

        public CompletionNodes(SyntaxNode container, SyntaxNode prefix)
        {
            Container = container;
            Prefix = prefix;
        }
    }
}