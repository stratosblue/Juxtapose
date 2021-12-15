using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator
{
    internal static class JuxtaposeSourceGeneratorContextExtensions
    {
        #region Public 方法

        public static bool TryGetParameterPackWithDiagnostic(this JuxtaposeSourceGeneratorContext context, IMethodSymbol methodSymbol, out ParameterPackSourceCode parameterPackSourceCode)
        {
            if (!context.TryGetParameterPack(methodSymbol, out parameterPackSourceCode!)
                || parameterPackSourceCode is null)
            {
                context.GeneratorExecutionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CanNotFoundGeneratedParameterPack, null, methodSymbol.ToFullyQualifiedDisplayString()));
                return false;
            }
            return true;
        }

        #endregion Public 方法
    }
}