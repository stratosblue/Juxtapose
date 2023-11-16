using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate;

public class IllusionStaticClassCodeGenerator : ISourceCodeProvider<SourceCode>
{
    #region Private 字段

    private readonly ClassStringBuilder _sourceBuilder = new();

    private readonly VariableName _vars;

    private string? _generatedSource = null;

    #endregion Private 字段

    #region Public 属性

    public JuxtaposeContextSourceGeneratorContext Context { get; }

    public IllusionStaticClassDescriptor Descriptor { get; }

    public SubResourceCollection Resources { get; }

    public string SourceHintName { get; }

    #endregion Public 属性

    #region Public 构造函数

    public IllusionStaticClassCodeGenerator(JuxtaposeContextSourceGeneratorContext context, IllusionStaticClassDescriptor descriptor, SubResourceCollection resources)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        SourceHintName = $"{Descriptor.TypeFullName}.StaticIllusion.g.cs";

        _vars = new VariableName();
    }

    #endregion Public 构造函数

    #region Private 方法

    private void GenerateProxyClassSource()
    {
        _sourceBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
        _sourceBuilder.AppendLine();
        _sourceBuilder.Namespace(() =>
        {
            _sourceBuilder.AppendIndentLine($"/// <inheritdoc cref=\"{Descriptor.TargetType.ToFullyQualifiedDisplayString()}\"/>");
            _sourceBuilder.AppendIndentLine($"{Descriptor.Accessibility.ToCodeString()} static class {Descriptor.TypeName}");
            _sourceBuilder.Scope(() =>
            {
                _sourceBuilder.AppendIndentLine($"private static readonly {Descriptor.ContextType.ToFullyQualifiedDisplayString()} s_context = {Descriptor.ContextType.ToFullyQualifiedDisplayString()}.SharedInstance;", true);

                new StaticProxyCodeGenerator(Context, Resources, _sourceBuilder, Descriptor.TargetType, _vars).GenerateMemberProxyCode();
            });
        }, Descriptor.Namespace);
    }

    private string GenerateProxyTypeSource()
    {
        if (_generatedSource != null)
        {
            return _generatedSource;
        }

        GenerateProxyClassSource();
        _generatedSource = _sourceBuilder.ToString();

        return _generatedSource;
    }

    #endregion Private 方法

    #region Public 方法

    public IEnumerable<SourceCode> GetSources()
    {
        yield return new FullSourceCode(SourceHintName, GenerateProxyTypeSource());
    }

    #endregion Public 方法
}
