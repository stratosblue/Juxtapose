namespace Microsoft.CodeAnalysis;

internal static class AccessibilityExtensions
{
    #region Public 方法

    public static string ToCodeString(this Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.NotApplicable => string.Empty,
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "internal",
            Accessibility.Public => "public",
            _ => string.Empty,
        };
    }

    #endregion Public 方法
}
