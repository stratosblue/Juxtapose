namespace Microsoft.CodeAnalysis
{
    internal static class AccessibilityExtensions
    {
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
    }
}