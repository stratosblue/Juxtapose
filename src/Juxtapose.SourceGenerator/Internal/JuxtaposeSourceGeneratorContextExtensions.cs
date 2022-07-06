using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator;

internal static class JuxtaposeSourceGeneratorContextExtensions
{
    #region Public 方法

    public static bool TryGetConstructorParameterPackWithDiagnostic(this JuxtaposeSourceGeneratorContext context, IMethodSymbol constructor, string generatedTypeName, out ConstructorParameterPackSourceCode? parameterPackSourceCode)
    {
        if (!context.Resources.TryGetConstructorParameterPackSourceCode(constructor, generatedTypeName, out parameterPackSourceCode)
            || parameterPackSourceCode is null)
        {
            context.GeneratorExecutionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CanNotFoundGeneratedConstructorParameterPack, null, constructor.ToDisplayString(), generatedTypeName));
            return false;
        }
        return true;
    }

    public static bool TryGetMethodParameterPackWithDiagnostic(this JuxtaposeSourceGeneratorContext context, IMethodSymbol methodSymbol, out ParameterPackSourceCode parameterPackSourceCode)
    {
        if (!context.Resources.TryGetMethodArgumentPackSourceCode(methodSymbol, out parameterPackSourceCode!)
            || parameterPackSourceCode is null)
        {
            context.GeneratorExecutionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CanNotFoundGeneratedParameterPack, null, methodSymbol.ToDisplayString()));
            return false;
        }
        return true;
    }

    public static bool TryGetMethodResultPackWithDiagnostic(this JuxtaposeSourceGeneratorContext context, IMethodSymbol methodSymbol, out ResultPackSourceCode resultPackSourceCode)
    {
        if (!context.Resources.TryGetMethodResultPackSourceCode(methodSymbol, out resultPackSourceCode!)
            || resultPackSourceCode is null)
        {
            context.GeneratorExecutionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CanNotFoundGeneratedParameterPack, null, methodSymbol.ToDisplayString()));
            return false;
        }
        return true;
    }

    #endregion Public 方法
}