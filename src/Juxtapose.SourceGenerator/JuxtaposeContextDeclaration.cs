using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Juxtapose.SourceGenerator;

public readonly struct JuxtaposeContextDeclaration : IEquatable<JuxtaposeContextDeclaration>
{
    #region Public 字段

    public static readonly JuxtaposeContextDeclaration Default = new();

    #endregion Public 字段

    #region Public 属性

    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }

    public string ClassName { get; }

    public bool HasPartialKeyword => ClassDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);

    public bool IsDefault => TypeSymbol is null;

    public string NameSpace { get; }

    public SemanticModel SemanticModel { get; }

    public INamedTypeSymbol TypeSymbol { get; }

    #endregion Public 属性

    #region Public 构造函数

    private JuxtaposeContextDeclaration(SemanticModel semanticModel,
                                        INamedTypeSymbol typeSymbol,
                                        ClassDeclarationSyntax classDeclarationSyntax,
                                        string className,
                                        string nameSpace)
    {
        SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        TypeSymbol = typeSymbol ?? throw new ArgumentNullException(nameof(typeSymbol));
        ClassDeclarationSyntax = classDeclarationSyntax ?? throw new ArgumentNullException(nameof(classDeclarationSyntax));
        ClassName = className ?? throw new ArgumentNullException(nameof(className));
        NameSpace = nameSpace ?? throw new ArgumentNullException(nameof(nameSpace));
    }

    #endregion Public 构造函数

    #region Public 方法

    public static JuxtaposeContextDeclaration Create(SemanticModel semanticModel, INamedTypeSymbol typeSymbol, ClassDeclarationSyntax classDeclarationSyntax)
    {
        var className = classDeclarationSyntax.Identifier.ValueText;
        var nameSpace = typeSymbol.ContainingNamespace.ToDisplayString();

        return new(semanticModel, typeSymbol, classDeclarationSyntax, className, nameSpace);
    }

    #region Equals

    public static bool operator !=(JuxtaposeContextDeclaration a, JuxtaposeContextDeclaration b) => !a.Equals(b);

    public static bool operator ==(JuxtaposeContextDeclaration a, JuxtaposeContextDeclaration b) => a.Equals(b);

    public readonly bool Equals(JuxtaposeContextDeclaration other) => string.Equals(NameSpace, other.NameSpace) && string.Equals(ClassName, other.ClassName);

    public override readonly bool Equals(object obj) => obj is JuxtaposeContextDeclaration other && Equals(other);

    public override readonly int GetHashCode() => ClassName.GetHashCode() & NameSpace.GetHashCode();

    #endregion Equals

    #endregion Public 方法
}

public class JuxtaposeContextDeclarationEqualityComparer : IEqualityComparer<JuxtaposeContextDeclaration>
{
    #region Public 字段

    public static readonly JuxtaposeContextDeclarationEqualityComparer Default = new();

    #endregion Public 字段

    #region Public 方法

    public bool Equals(JuxtaposeContextDeclaration x, JuxtaposeContextDeclaration y)
    {
        return x.GetHashCode() == y.GetHashCode();
    }

    public int GetHashCode(JuxtaposeContextDeclaration obj)
    {
        return obj.GetHashCode();
    }

    #endregion Public 方法
}
