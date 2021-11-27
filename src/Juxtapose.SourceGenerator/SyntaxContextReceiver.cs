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
        #region Public 属性

        public List<INamedTypeSymbol> ShouldGenerateTypes { get; } = new();

        #endregion Public 属性

        #region Public 方法

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            //Debug.WriteLine(context.Node.GetType());
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                && classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
                && classDeclarationSyntax.AttributeLists.Count > 0
                && context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol namedTypeSymbol
                && namedTypeSymbol.IsBaseOnJuxtaposeContext())
            {
                if (namedTypeSymbol.GetAttributes().Any(m => m.IsIllusionAttribute() || m.IsIllusionAttribute()))
                {
                    ShouldGenerateTypes.Add(namedTypeSymbol);
                }
            }
        }

        #endregion Public 方法
    }
}