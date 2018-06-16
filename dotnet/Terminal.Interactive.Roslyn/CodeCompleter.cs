using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Terminal.Interactive.Roslyn
{
    public class CodeCompleter
    {
        private readonly IEnumerable<ISymbol> _symbols;

        private readonly string _prefix;

        public CodeCompleter(SyntaxTree tree, Compilation compilation, int location)
        {
            var semantics = compilation.GetSemanticModel(tree);

            var nodes = GetNodes(tree.GetRoot().GetMostSpecificNodeOrTokenAt(location > 0 ? location - 1 : 0));
            _prefix = nodes.Prefix?.GetText().ToString() ?? "";

            var context = semantics.GetCompletionSymbols(nodes.Container);

            var container = nodes.Container == null
                ? compilation.GetGlobalSymbol() ?? context.NamespaceOrType
                : context.NamespaceOrType;

            _symbols = semantics.LookupSymbols(location, container)
                .Where(s => context.Symbol == null || !s.IsStatic)
                .OrderBy(s => s.Name)
                .ThenBy(s =>
                {
                    switch (s)
                    {
                        case IMethodSymbol m: return m.Parameters.Length;
                        default: return 0;
                    }
                });
        }

        public IEnumerable<CompletionData> GetCompletions()
        {
            return _symbols.Where(s => s.Name.StartsWith(_prefix, StringComparison.InvariantCultureIgnoreCase))
                .Select(s => new CompletionData(s.Name.Substring(0, _prefix.Length),
                    s.ToDisplayString(DisplayFormats.CompletionFormat).Substring(_prefix.Length),
                    s.ToDisplayString(DisplayFormats.ContentFormat),
                    s.ToDisplayString(DisplayFormats.DescriptionFormat))
                );
        }

        private static CompletionNodes GetNodes(SyntaxNodeOrToken nodeOrToken)
        {
            SyntaxNodeOrToken dot;
            SyntaxNode prefix = null;

            if (nodeOrToken.IsNode)
            {
                prefix = nodeOrToken.AsNode();
                dot = nodeOrToken.GetPreviousSibling();
                if (dot.Kind() != SyntaxKind.DotToken)
                {
                    return new CompletionNodes(null, prefix);
                }
            }
            else
            {
                if (nodeOrToken.Kind() != SyntaxKind.DotToken)
                {
                    return new CompletionNodes(null, prefix);
                }
                dot = nodeOrToken;
            }

            var previous = dot.GetPreviousSibling();
            return previous.IsNode ? new CompletionNodes(previous.AsNode(), prefix) : new CompletionNodes(null, prefix);
        }
    }
}