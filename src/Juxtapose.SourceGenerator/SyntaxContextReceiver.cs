using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Juxtapose.SourceGenerator
{
    internal class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        public List<INamedTypeSymbol> ShouldGenerateTypes { get; } = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            //Debug.WriteLine(context.Node.GetType());
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                && classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
                && classDeclarationSyntax.AttributeLists.Count > 0
                && context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol namedTypeSymbol
                && namedTypeSymbol.IsBaseOnJuxtaposeContext())
            {
                if (namedTypeSymbol.GetAttributes().Any(m => m.IsIllusionClassAttribute()))
                {
                    ShouldGenerateTypes.Add(namedTypeSymbol);
                }
            }
        }
    }
}