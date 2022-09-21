using System;
using System.Linq;

using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis;

//see https://platform.uno/blog/using-msbuild-items-and-properties-in-c-9-source-generators/
internal static class SourceGeneratorContextExtensions
{
    #region Private 字段

    private const string SourceItemGroupMetadata = "build_metadata.AdditionalFiles.SourceItemGroup";

    #endregion Private 字段

    #region Public 方法

    public static string[] GetMSBuildItems(this GeneratorExecutionContext context, string name)
        => context
            .AdditionalFiles
            .Where(f => context.AnalyzerConfigOptions
                .GetOptions(f)
                .TryGetValue(SourceItemGroupMetadata, out var sourceItemGroup)
                && sourceItemGroup == name)
            .Select(f => f.Path)
            .ToArray();

    public static bool IsMSBuildSwitchOn(this GeneratorExecutionContext context, string switchName)
    {
        return context.TryGetMSBuildProperty(switchName, out var value) && string.Equals("true", value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsMSBuildSwitchOn(this AnalyzerConfigOptionsProvider configOptionsProvider, string switchName)
    {
        return configOptionsProvider.TryGetMSBuildProperty(switchName, out var value) && string.Equals("true", value, StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryGetMSBuildProperty(this GeneratorExecutionContext context,
                                             string name,
                                             out string? value)
    {
        return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out value);
    }

    public static bool TryGetMSBuildProperty(this AnalyzerConfigOptionsProvider configOptionsProvider,
                                             string name,
                                             out string? value)
    {
        return configOptionsProvider.GlobalOptions.TryGetValue($"build_property.{name}", out value);
    }

    #endregion Public 方法
}