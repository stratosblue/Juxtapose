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

        var declarationsProvider = context.CompilationProvider.SelectMany(GetContextDeclarations);

        var combinedDeclarationsProvider = context.AnalyzerConfigOptionsProvider.Combine(declarationsProvider.Collect());

        var globalSourceGeneratorContext = new JuxtaposeSourceGeneratorContext(null);

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

                var firstContextDeclarations = contextDeclarations.First();
                var currentAssembly = firstContextDeclarations.TypeSymbol.ContainingAssembly;
                var compilation = firstContextDeclarations.Compilation;

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

    private static ImmutableArray<ContextDeclaration> GetContextDeclarations(Compilation compilation, CancellationToken cancellationToken)
    {
        BuildEnvironment.Init(compilation);

        var declarations = new List<ContextDeclaration>();
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            if (syntaxTree.TryGetRoot(out var syntaxNode)
                && syntaxNode is CompilationUnitSyntax compilationUnitSyntax)
            {
                var classDeclarationSyntaxs = compilationUnitSyntax.Members.OfType<NamespaceDeclarationSyntax>()
                                                                           .SelectMany(m => m.Members.OfType<ClassDeclarationSyntax>());

                foreach (var classDeclarationSyntax in classDeclarationSyntaxs)
                {
                    if (classDeclarationSyntax.AttributeLists.Count > 0
                        && !classDeclarationSyntax.Modifiers.Any(SyntaxKind.AbstractKeyword)
                        && compilation.GetSemanticModel(syntaxTree).GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol namedTypeSymbol
                        && namedTypeSymbol.IsBaseOnJuxtaposeContext())
                    {
                        declarations.Add(new(compilation, classDeclarationSyntax, namedTypeSymbol, classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)));
                    }
                }
            }
        }
        return declarations.ToImmutableArray();
    }

    #endregion Private 方法
}

public struct ContextDeclaration
{
    #region Public 属性

    public ClassDeclarationSyntax ClassDeclarationSyntax { get; set; }

    public Compilation Compilation { get; }

    public bool HasPartialKeyword { get; }
    public INamedTypeSymbol TypeSymbol { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ContextDeclaration(Compilation compilation, ClassDeclarationSyntax classDeclarationSyntax, INamedTypeSymbol typeSymbol, bool hasPartialKeyword)
    {
        TypeSymbol = typeSymbol;
        HasPartialKeyword = hasPartialKeyword;
        Compilation = compilation;
        ClassDeclarationSyntax = classDeclarationSyntax;
    }

    #endregion Public 构造函数
}
