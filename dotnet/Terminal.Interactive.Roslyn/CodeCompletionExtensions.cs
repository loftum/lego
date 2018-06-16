using Microsoft.CodeAnalysis;

namespace Terminal.Interactive.Roslyn
{
    public static class CodeCompletionExtensions
    {
        public static CompletionSymbols GetCompletionSymbols(this SemanticModel semantics, SyntaxNode node)
        {
            if (node == null)
            {
                return semantics.GetDefault();
            }
            var symbolInfo = semantics.GetSymbolInfo(node);

            var symbol = symbolInfo.Symbol;
            if (symbol is INamespaceOrTypeSymbol namespaceOrType)
            {
                return new CompletionSymbols(namespaceOrType, null);
            }

            var typeInfo = semantics.GetTypeInfo(node);
            var type = typeInfo.ConvertedType ?? typeInfo.Type;

            return type != null ? new CompletionSymbols(type, symbol) : semantics.GetDefault();
        }

        private static CompletionSymbols GetDefault(this SemanticModel semantics)
        {
            return new CompletionSymbols(semantics.Compilation.GlobalNamespace, null);
        }

        public static INamedTypeSymbol GetGlobalSymbol(this Compilation compilation)
        {
            var globalsType = compilation.ScriptCompilationInfo.GlobalsType;
            if (globalsType == null)
            {
                return null;
            }
            var global = compilation.GetTypeByMetadataName(globalsType.FullName);
            return global;
        }
    }
}