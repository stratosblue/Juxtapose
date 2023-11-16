using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Juxtapose.SourceGenerator.Internal;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Juxtapose.SourceGenerator;

public struct JuxtaposeContextDeclaration : IEquatable<JuxtaposeContextDeclaration>, IEqualityComparer<JuxtaposeContextDeclaration>
{
    #region Private 字段

    private static readonly SymbolDisplayFormat s_minimallyQualifiedFormat = new(SymbolDisplayGlobalNamespaceStyle.Omitted,
                                                                                 SymbolDisplayTypeQualificationStyle.NameOnly,
                                                                                 SymbolDisplayGenericsOptions.IncludeTypeParameters,
                                                                                 SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeRef,
                                                                                 SymbolDisplayDelegateStyle.NameOnly,
                                                                                 SymbolDisplayExtensionMethodStyle.Default,
                                                                                 SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeDefaultValue,
                                                                                 SymbolDisplayPropertyStyle.NameOnly,
                                                                                 SymbolDisplayLocalOptions.IncludeType,
                                                                                 SymbolDisplayKindOptions.IncludeMemberKeyword,
                                                                                 SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    private readonly string _hash;

    #endregion Private 字段

    #region Public 字段

    public static readonly JuxtaposeContextDeclaration Default = new();

    #endregion Public 字段

    #region Public 属性

    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }

    public bool HasPartialKeyword => ClassDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);

    public bool IsDefault => TypeSymbol is null;

    public SemanticModel SemanticModel { get; }

    public INamedTypeSymbol TypeSymbol { get; }

    #endregion Public 属性

    #region Public 构造函数

    private JuxtaposeContextDeclaration(SemanticModel semanticModel, INamedTypeSymbol typeSymbol, ClassDeclarationSyntax classDeclarationSyntax, string hash)
    {
        SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        TypeSymbol = typeSymbol ?? throw new ArgumentNullException(nameof(typeSymbol));
        ClassDeclarationSyntax = classDeclarationSyntax ?? throw new ArgumentNullException(nameof(classDeclarationSyntax));
        _hash = hash ?? throw new ArgumentNullException(nameof(hash));
    }

    #endregion Public 构造函数

    #region Public 方法

    private static readonly ConditionalWeakTable<string, byte[]> s_stringBytesCache = new();

    public static JuxtaposeContextDeclaration Create(SemanticModel semanticModel, INamedTypeSymbol typeSymbol, ClassDeclarationSyntax classDeclarationSyntax)
    {
        using var hashAlgorithm = MD5.Create();

        var allIllusionAttributes = typeSymbol.GetAttributes()
                                              .Where(m => m.IsIllusionAttribute())
                                              .Select(m => new IllusionAttributeDefine(m))
                                              .OrderBy(m => m.TargetType.Name, PersistentStringComparer.Instance)
                                              .ThenBy(m => m.FromIoCContainer)
                                              .ThenBy(m => m.Accessibility)
                                              .ThenBy(m => m.GeneratedTypeName ?? m.TargetType.Name, PersistentStringComparer.Instance)
                                              .ToArray();

        //计算上下文Hash，减少代码生成次数

        AppendString(classDeclarationSyntax.Identifier.ValueText);

        foreach (var attribute in allIllusionAttributes)
        {
            Append(BitConverter.GetBytes(attribute.FromIoCContainer));
            Append(BitConverter.GetBytes((int)attribute.Accessibility));
            AppendString(attribute.GeneratedTypeName);
            AppendTypeSymbol(attribute.TargetType);
        }

        hashAlgorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        return new(semanticModel, typeSymbol, classDeclarationSyntax, hashAlgorithm.Hash.ToHexString());

        void Append(byte[]? buffer)
        {
            if (buffer is null
                || buffer.Length == 0)
            {
                return;
            }

            hashAlgorithm.TransformBlock(inputBuffer: buffer, 0, buffer.Length, null, 0);
        }

        void AppendInt32(int value) => Append(BitConverter.GetBytes(value));

        void AppendString(string? value)
        {
            if (value is null)
            {
                return;
            }

            if (!s_stringBytesCache.TryGetValue(value, out var data))
            {
                data = Encoding.UTF8.GetBytes(value);
                s_stringBytesCache.Add(value, data);
            }
            Append(data);
        }

        void AppendTypeSymbol(INamedTypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
            {
                return;
            }

            AppendString(typeSymbol.ToDisplayString(s_minimallyQualifiedFormat));

            var members = typeSymbol.GetMembers()
                                    .Select(m => m.ToDisplayString(s_minimallyQualifiedFormat))
                                    .OrderBy(m => m, PersistentStringComparer.Instance);

            foreach (var item in members)
            {
                AppendString(item);
            }
        }
    }

    #region Equals

    public static bool operator !=(JuxtaposeContextDeclaration a, JuxtaposeContextDeclaration b) => !PersistentStringComparer.Instance.Equals(a._hash, b._hash);

    public static bool operator ==(JuxtaposeContextDeclaration a, JuxtaposeContextDeclaration b) => PersistentStringComparer.Instance.Equals(a._hash, b._hash);

    public bool Equals(JuxtaposeContextDeclaration other) => PersistentStringComparer.Instance.Equals(_hash, other._hash);

    public override bool Equals(object obj) => obj is JuxtaposeContextDeclaration other && PersistentStringComparer.Instance.Equals(_hash, other._hash);

    public override int GetHashCode() => PersistentStringComparer.Instance.GetHashCode(_hash);

    #endregion Equals

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
