using Juxtapose.SourceGenerator;

namespace Microsoft.CodeAnalysis
{
    internal static class AttributeDataExtensions
    {
        #region Public 方法

        public static bool IsIllusionAttribute(this AttributeData attributeData)
        {
            return attributeData.AttributeClass?.ToDisplayString() == TypeFullNames.Juxtapose.SourceGenerator.IllusionAttribute_NoGlobal;
        }

        #endregion Public 方法
    }
}