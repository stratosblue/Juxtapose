using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator;

internal static class GeneratedCommandUtil
{
    #region Public 方法

    public static string GetAccessExpression(INamedTypeSymbol contextTypeSymbol, int commandId) => $"{contextTypeSymbol.ToFullyQualifiedDisplayString()}.JuxtaposeGeneratedCommand.Command_{commandId}";

    public static string GetCommandAccessExpression(this JuxtaposeContextSourceGeneratorContext context, int commandId) => $"{GetAccessExpression(context.ContextDeclaration.TypeSymbol, commandId)}";

    public static string GetName(int commandId) => $"Command_{commandId}";

    #endregion Public 方法
}
