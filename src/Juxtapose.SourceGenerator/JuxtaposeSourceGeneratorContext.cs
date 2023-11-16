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

    /// <summary>
    /// 整个Context的全部资源
    /// </summary>
    public virtual ContextResourceCollection Resources { get; private set; }

    public virtual TypeSymbolAnalyzer TypeSymbolAnalyzer { get; private set; }

    #endregion Public 属性

    #region Public 构造函数

    public JuxtaposeSourceGeneratorContext(TypeSymbolAnalyzer typeSymbolAnalyzer, IDiagnosticReporter? diagnosticReporter)
    {
        TypeSymbolAnalyzer = typeSymbolAnalyzer ?? throw new ArgumentNullException(nameof(typeSymbolAnalyzer));
        Resources = new(TypeSymbolAnalyzer);
        DiagnosticReporter = diagnosticReporter;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void ReportDiagnostic(Diagnostic diagnostic) => DiagnosticReporter?.ReportDiagnostic(diagnostic);

    #endregion Public 方法
}

public class JuxtaposeContextSourceGeneratorContext : JuxtaposeSourceGeneratorContext
{
    #region Public 属性

    public JuxtaposeContextDeclaration ContextDeclaration { get; }

    #endregion Public 属性

    #region Public 构造函数

    public JuxtaposeContextSourceGeneratorContext(JuxtaposeContextDeclaration contextDeclaration, TypeSymbolAnalyzer typeSymbolAnalyzer, IDiagnosticReporter? diagnosticReporter)
        : base(typeSymbolAnalyzer, diagnosticReporter)
    {
        ContextDeclaration = contextDeclaration;
    }

    #endregion Public 构造函数
}
