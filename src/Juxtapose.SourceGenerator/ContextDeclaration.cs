using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Juxtapose.SourceGenerator;

public struct JuxtaposeContextDeclaration : IEquatable<JuxtaposeContextDeclaration>
{
    #region Public 字段

    private readonly string _hash;
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
                                              .OrderBy(m => m.TargetType.Name)
                                              .ThenBy(m => m.GeneratedTypeName ?? string.Empty)
                                              .ThenBy(m => m.InheritType?.Name ?? string.Empty)
                                              .ThenBy(m => m.FromIoCContainer)
                                              .ThenBy(m => m.Accessibility)
                                              .ToArray();

        //计算上下文Hash，减少代码生成次数

        AppendString(classDeclarationSyntax.Identifier.ValueText);

        foreach (var attribute in allIllusionAttributes)
        {
            Append(BitConverter.GetBytes(attribute.FromIoCContainer));
            Append(BitConverter.GetBytes((int)attribute.Accessibility));
            AppendString(attribute.GeneratedTypeName);
            AppendTypeSymbol(attribute.InheritType);
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

            AppendString(typeSymbol.ContainingNamespace.Name);
            AppendString(typeSymbol.Name);

            foreach (var item in typeSymbol.GetMembers().OrderBy(m => m.Name))
            {
                AppendString(item.Name);
                switch (item)
                {
                    case IMethodSymbol methodSymbol:
                        {
                            AppendInt32((int)methodSymbol.DeclaredAccessibility);

                            foreach (var parameter in methodSymbol.Parameters)
                            {
                                AppendString(parameter.Type.ContainingNamespace.Name);
                                AppendString(parameter.Type.Name);
                                AppendString(parameter.Name);
                            }
                            AppendString(methodSymbol.ReturnType.ContainingNamespace.Name);
                            AppendString(methodSymbol.ReturnType.Name);
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }

    #region Equals

    public static bool operator !=(JuxtaposeContextDeclaration a, JuxtaposeContextDeclaration b) => a._hash != b._hash;

    public static bool operator ==(JuxtaposeContextDeclaration a, JuxtaposeContextDeclaration b) => a._hash == b._hash;

    public bool Equals(JuxtaposeContextDeclaration other) => _hash == other._hash;

    public override bool Equals(object obj) => obj is JuxtaposeContextDeclaration other && _hash == other._hash;

    public override int GetHashCode() => _hash.GetHashCode();

    #endregion Equals

    #endregion Public 方法
}
