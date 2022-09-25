using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator;

public interface IDiagnosticReporter
{
    #region Public 方法

    void ReportDiagnostic(Diagnostic diagnostic);

    #endregion Public 方法
}

internal class SourceProductionContextDiagnosticReporter : IDiagnosticReporter
{
    #region Private 字段

    private readonly SourceProductionContext _context;

    #endregion Private 字段

    #region Public 构造函数

    public SourceProductionContextDiagnosticReporter(SourceProductionContext context)
    {
        _context = context;
    }

    #endregion Public 构造函数

    #region Public 方法

    public void ReportDiagnostic(Diagnostic diagnostic) => _context.ReportDiagnostic(diagnostic);

    #endregion Public 方法
}