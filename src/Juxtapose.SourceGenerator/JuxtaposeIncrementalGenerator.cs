using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Juxtapose.SourceGenerator.CodeGenerate;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Juxtapose.SourceGenerator;

[Generator]
public class JuxtaposeIncrementalGenerator : IIncrementalGenerator
{
    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //System.Diagnostics.Debugger.Launch();

        var declarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterContextSyntaxNode, TransformContextSyntaxNode).Where(m => !m.IsDefault);

        var combinedDeclarationsProvider = context.AnalyzerConfigOptionsProvider.Combine(declarationsProvider.Collect());

        context.RegisterImplementationSourceOutput(combinedDeclarationsProvider, (context, input) =>
        {
            bool isSaveGeneratedCodeFile = false;
            string? saveGeneratedCodePath = null;

            var analyzerConfigOptionsProvider = input.Left;
            var contextDeclarations = input.Right;
            try
            {
                DebuggerLauncher.TryLaunch(analyzerConfigOptionsProvider);

                foreach (var item in contextDeclarations.Where(m => !m.HasPartialKeyword))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoPartialKeywordForContext, null, item.TypeSymbol.ToDisplayString()));
                }

                contextDeclarations = contextDeclarations.Where(m => m.HasPartialKeyword).ToImmutableArray();

                var firstContextDeclaration = contextDeclarations.FirstOrDefault();

                if (firstContextDeclaration.IsDefault)
                {
                    return;
                }

                var currentAssembly = firstContextDeclaration.TypeSymbol.ContainingAssembly;
                var compilation = firstContextDeclaration.SemanticModel.Compilation;

                BuildEnvironment.Init(compilation);

                var globalSourceGeneratorContext = new JuxtaposeSourceGeneratorContext(null);

                isSaveGeneratedCodeFile = analyzerConfigOptionsProvider.TryGetMSBuildProperty("SaveJuxtaposeGeneratedCode", out saveGeneratedCodePath);

                if (isSaveGeneratedCodeFile)
                {
                    if (!analyzerConfigOptionsProvider.TryGetMSBuildProperty("ProjectDir", out var projectdir)
                        || string.IsNullOrWhiteSpace(projectdir))
                    {
                        throw new InvalidOperationException("can not get ProjectDir.");
                    }

                    if (string.IsNullOrWhiteSpace(saveGeneratedCodePath))
                    {
                        saveGeneratedCodePath = projectdir;
                    }
                    else if (!Path.IsPathRooted(saveGeneratedCodePath))
                    {
                        saveGeneratedCodePath = Path.Combine(projectdir, saveGeneratedCodePath);
                    }

                    if (!Directory.Exists(saveGeneratedCodePath))
                    {
                        Directory.CreateDirectory(saveGeneratedCodePath);
                    }
                }

                var sourceGeneratorContext = new JuxtaposeSourceGeneratorContextWrapper(globalSourceGeneratorContext, new SourceProductionContextDiagnosticReporter(context));

                var allGeneratedSources = contextDeclarations.Select(m => new JuxtaposeContextCodeGenerator(sourceGeneratorContext, m.TypeSymbol))
                                                             .SelectMany(m => m.GetSources())
                                                             .ToArray();

                var partialSources = allGeneratedSources.OfType<PartialSourceCode>()
                                                        .ToArray();

                var fullSources = allGeneratedSources.OfType<FullSourceCode>()
                                                     .ToArray();

                var allPackTypeHashSet = new HashSet<string>();

                var aggregatedPartialSources = partialSources.GroupBy(m => m.HintName)
                                                             .Select(m =>
                                                             {
                                                                 var validItems = new List<PartialSourceCode>();
                                                                 foreach (var item in m)
                                                                 {
                                                                     if (compilation.GetTypeByMetadataName(item.TypeFullName) is INamedTypeSymbol existedTypeSymbol
                                                                         && existedTypeSymbol.ContainingAssembly.Equals(currentAssembly, SymbolEqualityComparer.Default))
                                                                     {
                                                                         continue;
                                                                     }
                                                                     if (allPackTypeHashSet.Contains(item.TypeFullName))
                                                                     {
                                                                         continue;
                                                                     }
                                                                     if (allPackTypeHashSet.Add(item.TypeFullName))
                                                                     {
                                                                         validItems.Add(item);
                                                                     }
                                                                 }
                                                                 if (validItems.Count > 0)
                                                                 {
                                                                     var builder = new ClassStringBuilder();
                                                                     builder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
                                                                     builder.AppendIndentLine("#nullable disable");
                                                                     builder.AppendLine();

                                                                     foreach (var namespaceGroup in validItems.GroupBy(m => m.Namespace))
                                                                     {
                                                                         builder.Namespace(() =>
                                                                         {
                                                                             foreach (var item in namespaceGroup)
                                                                             {
                                                                                 builder.AppendLine();
                                                                                 builder.AppendLine(item.Source);
                                                                             }
                                                                         }
                                                                         , namespaceGroup.Key);
                                                                     }

                                                                     return new FullSourceCode(m.Key, builder.ToString());
                                                                 }
                                                                 return null;
                                                             })
                                                             .OfType<SourceCode>()
                                                             .ToArray();

                AddSources(aggregatedPartialSources);

                AddSources(fullSources);
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnknownExceptionHasThrew, null, ex.GetType().FullName, OneLine(ex.ToString())));
            }

            void AddSources(IEnumerable<SourceCode> sourceInfo)
            {
                foreach (var item in sourceInfo)
                {
                    AddSource(item);
                }
            }

            void AddSource(SourceCode sourceInfo)
            {
                var hintName = string.Join(".", sourceInfo.HintName.Split('.').Reverse().Where(m => m != "cs" && m != "g"));
                hintName = $"{hintName}.g.cs";

                if (isSaveGeneratedCodeFile)
                {
                    try
                    {
                        var outputPath = Path.Combine(saveGeneratedCodePath!, hintName);
                        File.WriteAllText(outputPath, sourceInfo.Source);
                    }
                    catch (Exception ex)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.SaveCodeAsFileFail, null, hintName, OneLine(ex.ToString())));
                    }
                }

                context.AddSource(hintName, SourceText.From(sourceInfo.Source, Encoding.UTF8));
            }

            static string OneLine(string text)
            {
                return text.Replace("\r", "").Replace("\n", "\\n");
            }
        });
    }

    #endregion Public 方法

    #region Private 方法

    private static bool FilterContextSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
            && classDeclarationSyntax.AttributeLists.Count > 0
            && !classDeclarationSyntax.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return true;
        }
        return false;
    }

    private static ContextDeclaration TransformContextSyntaxNode(GeneratorSyntaxContext syntaxContext, CancellationToken cancellationToken)
    {
        if (syntaxContext.Node is ClassDeclarationSyntax classDeclarationSyntax
            && syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsBaseOnJuxtaposeContext())
        {
            return new(syntaxContext.SemanticModel, classDeclarationSyntax, namedTypeSymbol, classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword));
        }

        return ContextDeclaration.Default;
    }

    #endregion Private 方法
}

public struct ContextDeclaration
{
    #region Public 字段

    public static readonly ContextDeclaration Default = new();

    #endregion Public 字段

    #region Public 属性

    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }

    public bool HasPartialKeyword { get; }
    
    public bool IsDefault => ClassDeclarationSyntax is null;
    
    public SemanticModel SemanticModel { get; }

    public INamedTypeSymbol TypeSymbol { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ContextDeclaration(SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax, INamedTypeSymbol typeSymbol, bool hasPartialKeyword)
    {
        SemanticModel = semanticModel;
        ClassDeclarationSyntax = classDeclarationSyntax;
        TypeSymbol = typeSymbol;
        HasPartialKeyword = hasPartialKeyword;
    }

    #endregion Public 构造函数
}
