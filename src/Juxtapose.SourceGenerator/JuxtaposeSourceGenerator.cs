using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Juxtapose.SourceGenerator.CodeGenerate;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Juxtapose.SourceGenerator;

[Generator]
public class JuxtaposeSourceGenerator : ISourceGenerator
{
    #region Public 方法

    public void Execute(GeneratorExecutionContext context)
    {
        bool isSaveGeneratedCodeFile = false;
        string? saveGeneratedCodePath = null;
        try
        {
            DebuggerLauncher.TryLaunch(context);

            BuildEnvironment.Init(context);

            if (context.SyntaxContextReceiver is not SyntaxContextReceiver contextReceiver)
            {
                throw new NullReferenceException($"not found {nameof(contextReceiver)}");
            }

            foreach (var item in contextReceiver.NoPartialKeywordContextTypes)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoPartialKeywordForContext, null, item.ToDisplayString()));
            }

            if (contextReceiver.ShouldGenerateTypes.Count == 0)
            {
                return;
            }

            isSaveGeneratedCodeFile = context.TryGetMSBuildProperty("SaveJuxtaposeGeneratedCode", out saveGeneratedCodePath);

            if (isSaveGeneratedCodeFile)
            {
                if (!context.TryGetMSBuildProperty("ProjectDir", out var projectdir)
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

            var sourceGeneratorContext = new JuxtaposeSourceGeneratorContext(context);

            var allGeneratedSources = contextReceiver.ShouldGenerateTypes.Select(m => new JuxtaposeContextCodeGenerator(sourceGeneratorContext, m))
                                                                         .SelectMany(m => m.GetSources())
                                                                         .ToArray();

            var partialSources = allGeneratedSources.OfType<PartialSourceCode>()
                                                    .ToArray();

            var fullSources = allGeneratedSources.OfType<FullSourceCode>()
                                                 .ToArray();

            var allPackTypeHashSet = new HashSet<string>();

            var currentAssembly = contextReceiver.ShouldGenerateTypes.First().ContainingAssembly;

            var aggregatedPartialSources = partialSources.GroupBy(m => m.HintName)
                                                         .Select(m =>
                                                         {
                                                             var validItems = new List<PartialSourceCode>();
                                                             foreach (var item in m)
                                                             {
                                                                 if (context.Compilation.GetTypeByMetadataName(item.TypeFullName) is INamedTypeSymbol existedTypeSymbol
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
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    #endregion Public 方法
}