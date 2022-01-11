using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Juxtapose.SourceGenerator
{
    internal class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        #region Public 属性

        /// <summary>
        /// 没有标记 partial 的上下文类型
        /// </summary>
        public List<INamedTypeSymbol> NoPartialKeywordContextTypes { get; } = new();

        /// <summary>
        /// 需要生成的类型
        /// </summary>
        public List<INamedTypeSymbol> ShouldGenerateTypes { get; } = new();

        #endregion Public 属性

        #region Public 方法

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            //System.Diagnostics.Debug.WriteLine(context.Node.GetType());
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                && classDeclarationSyntax.AttributeLists.Count > 0
                && !classDeclarationSyntax.Modifiers.Any(SyntaxKind.AbstractKeyword)
                && context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol namedTypeSymbol
                && namedTypeSymbol.IsBaseOnJuxtaposeContext())
            {
                if (classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    //HACK 暂时移除此判断
                    //if (namedTypeSymbol.GetAttributes().Any(m => m.IsIllusionAttribute()))
                    //{
                    ShouldGenerateTypes.Add(namedTypeSymbol);
                    //}
                }
                else
                {
                    NoPartialKeywordContextTypes.Add(namedTypeSymbol);
                }
            }
        }

        #endregion Public 方法
    }
}