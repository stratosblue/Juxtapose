using System.Collections.Concurrent;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator;

public class JuxtaposeSourceGeneratorContext
{
    #region Public 属性

    public IDiagnosticReporter? DiagnosticReporter { get; protected set; }

    public virtual ConcurrentDictionary<IllusionInstanceClassDescriptor, SubResourceCollection> IllusionInstanceClasses { get; private set; } = new();

    public virtual ConcurrentDictionary<IllusionStaticClassDescriptor, SubResourceCollection> IllusionStaticClasses { get; private set; } = new();

    public virtual ContextResourceCollection Resources { get; private set; } = new();

    #endregion Public 属性

    #region Public 构造函数

    public JuxtaposeSourceGeneratorContext(IDiagnosticReporter? diagnosticReporter)
    {
        DiagnosticReporter = diagnosticReporter;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Clear()
    {
        IllusionInstanceClasses = new();
        IllusionStaticClasses = new();
        Resources = new();
    }

    public void ReportDiagnostic(Diagnostic diagnostic) => DiagnosticReporter?.ReportDiagnostic(diagnostic);

    #endregion Public 方法
}

internal class JuxtaposeSourceGeneratorContextWrapper : JuxtaposeSourceGeneratorContext
{
    #region Private 字段

    private readonly JuxtaposeSourceGeneratorContext _inner;

    #endregion Private 字段

    #region Public 属性

    public override ConcurrentDictionary<IllusionInstanceClassDescriptor, SubResourceCollection> IllusionInstanceClasses => _inner.IllusionInstanceClasses;

    public override ConcurrentDictionary<IllusionStaticClassDescriptor, SubResourceCollection> IllusionStaticClasses => _inner.IllusionStaticClasses;

    public override ContextResourceCollection Resources => _inner.Resources;

    #endregion Public 属性

    #region Public 构造函数

    public JuxtaposeSourceGeneratorContextWrapper(JuxtaposeSourceGeneratorContext inner, IDiagnosticReporter diagnosticReporter) : base(diagnosticReporter)
    {
        _inner = inner;
    }

    #endregion Public 构造函数
}