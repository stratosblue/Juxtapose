using Juxtapose.SourceGenerator;

namespace Microsoft.CodeAnalysis
{
    internal static class AttributeDataExtensions
    {
        #region Public 方法

        public static bool IsIllusionClassAttribute(this AttributeData attributeData)
        {
            return attributeData.AttributeClass?.ToDisplayString() == TypeFullNames.Juxtapose.SourceGenerator.IllusionClassAttribute_NoGlobal;
        }

        public static bool IsIllusionStaticClassAttribute(this AttributeData attributeData)
        {
            return attributeData.AttributeClass?.ToDisplayString() == TypeFullNames.Juxtapose.SourceGenerator.IllusionStaticClassAttribute_NoGlobal;
        }

        #endregion Public 方法
    }
}