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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Juxtapose.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class JuxtaposeIncrementalGenerator : IIncrementalGenerator
{
    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //System.Diagnostics.Debugger.Launch();

        var declarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterContextSyntaxNode, TransformContextSyntaxNode).Where(m => !m.IsDefault);

        var combinedDeclarationsProvider = context.AnalyzerConfigOptionsProvider.Combine(declarationsProvider.Collect());

        context.RegisterSourceOutput(combinedDeclarationsProvider,
                                     (context, source) => GenerateSourceCodes(context, source.Left, source.Right));
    }

    #endregion Public 方法

    #region Private 方法

    #region filter

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

    private static JuxtaposeContextDeclaration TransformContextSyntaxNode(GeneratorSyntaxContext syntaxContext, CancellationToken cancellationToken)
    {
        if (syntaxContext.Node is ClassDeclarationSyntax classDeclarationSyntax
            && syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.IsBaseOnJuxtaposeContext())
        {
            return JuxtaposeContextDeclaration.Create(syntaxContext.SemanticModel, namedTypeSymbol, classDeclarationSyntax);
        }

        return JuxtaposeContextDeclaration.Default;
    }

    #endregion filter

    private static void GenerateSourceCodes(SourceProductionContext context,
                                            AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider,
                                            ImmutableArray<JuxtaposeContextDeclaration> contextDeclarations)
    {
        bool isSaveGeneratedCodeFile = false;
        string? saveGeneratedCodePath = null;

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

            var typeSymbolChecker = new TypeSymbolAnalyzer(compilation);

            var sourceGeneratorContext = new JuxtaposeSourceGeneratorContext(typeSymbolChecker, new SourceProductionContextDiagnosticReporter(context));

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

            var allGeneratedSources = contextDeclarations.Select(m => new JuxtaposeContextCodeGenerator(sourceGeneratorContext, m.TypeSymbol))
                                                         .SelectMany(m => m.GetSources())
                                                         .ToArray();

            var partialSources = allGeneratedSources.OfType<PartialSourceCode>()
                                                    .ToArray();

            var fullSources = allGeneratedSources.OfType<FullSourceCode>()
                                                 .ToArray();

            var allPackTypeHashSet = new HashSet<string>();

            var codeBuilder = new ClassStringBuilder(4096);

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
                                                                 codeBuilder.Clear();
                                                                 codeBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
                                                                 codeBuilder.AppendIndentLine("#nullable disable");
                                                                 codeBuilder.AppendLine();

                                                                 foreach (var namespaceGroup in validItems.GroupBy(m => m.Namespace))
                                                                 {
                                                                     codeBuilder.Namespace(() =>
                                                                     {
                                                                         foreach (var item in namespaceGroup)
                                                                         {
                                                                             codeBuilder.AppendLine();
                                                                             codeBuilder.AppendLine(item.Source);
                                                                         }
                                                                     }
                                                                     , namespaceGroup.Key);
                                                                 }

                                                                 return new FullSourceCode(m.Key, codeBuilder.ToString());
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
    }

    #endregion Private 方法
}
