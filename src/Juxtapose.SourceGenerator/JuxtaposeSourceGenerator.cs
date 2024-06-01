using System.Collections.Immutable;
using System.Text;

using Juxtapose.SourceGenerator;
using Juxtapose.SourceGenerator.CodeGenerate;
using Juxtapose.SourceGenerator.Internal;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Juxtapose;

[Generator(LanguageNames.CSharp)]
public class JuxtaposeSourceGenerator : IIncrementalGenerator
{
    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterContextSyntaxNode, TransformContextSyntaxNode)
                                                         .Where(m => !m.IsDefault)
                                                         .WithComparer(JuxtaposeContextDeclaration.Default);

#if SAVE_GENERATED_CODE

        var analyzerConfigOptionsProvider = context.AnalyzerConfigOptionsProvider;
        context.RegisterSourceOutput(analyzerConfigOptionsProvider.Combine(declarationsProvider.Collect()),
                                     (context, source) => GenerateSourceCodes(context, source.Left, source.Right));

#else

        context.RegisterSourceOutput(declarationsProvider.Collect(),
                                      (context, source) => GenerateSourceCodes(context, null, source));

#endif
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
                                            AnalyzerConfigOptionsProvider? analyzerConfigOptionsProvider,
                                            ImmutableArray<JuxtaposeContextDeclaration> contextDeclarations)
    {
        bool isSaveGeneratedCodeFile = false;
        string? saveGeneratedCodePath = null;

        try
        {
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

            var compilation = firstContextDeclaration.SemanticModel.Compilation;

            var typeSymbolChecker = new TypeSymbolAnalyzer(compilation);

            isSaveGeneratedCodeFile = analyzerConfigOptionsProvider?.TryGetMSBuildProperty("SaveJuxtaposeGeneratedCode", out saveGeneratedCodePath) == true;

            if (isSaveGeneratedCodeFile)
            {
                if (!analyzerConfigOptionsProvider!.TryGetMSBuildProperty("ProjectDir", out var projectdir)
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

            JuxtaposeContextCodeGenerator CreateContextGenerator(JuxtaposeContextDeclaration declaration)
            {
                var reproter = new SourceProductionContextDiagnosticReporter(context);
                var generatorContext = new JuxtaposeContextSourceGeneratorContext(declaration, typeSymbolChecker, reproter);
                return new JuxtaposeContextCodeGenerator(generatorContext).Preparation();
            }

            var allGeneratedSources = contextDeclarations.OrderBy(m => m.TypeSymbol.ToString(), PersistentStringComparer.Instance)
                                                         .Select(CreateContextGenerator)
                                                         .SelectMany(m => m.GetSources())
                                                         .ToArray();

            var fullSources = allGeneratedSources.OfType<FullSourceCode>()
                                                 .ToArray();

            AddSources(fullSources);
        }
        catch (Exception ex)
        {
#if DEBUG
            throw;
#endif
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
