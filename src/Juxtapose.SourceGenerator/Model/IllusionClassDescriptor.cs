using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model
{
    public abstract class IllusionClassDescriptor : IEquatable<IllusionClassDescriptor>
    {
        #region Public 属性

        public Accessibility Accessibility { get; }

        public INamedTypeSymbol ContextType { get; }

        /// <summary>
        /// 生成的类型的命名空间
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// 要生成代理的目标类型
        /// </summary>
        public INamedTypeSymbol TargetType { get; }

        /// <summary>
        /// 生成的类型完整名称
        /// </summary>
        public string TypeFullName { get; }

        /// <summary>
        /// 生成的类型名称
        /// </summary>
        public string TypeName { get; }

        #endregion Public 属性

        #region Public 构造函数

        public IllusionClassDescriptor(IllusionAttributeDefine attributeDefine, INamedTypeSymbol contextType)
        {
            if (attributeDefine is null)
            {
                throw new ArgumentNullException(nameof(attributeDefine));
            }

            if (contextType is null)
            {
                throw new ArgumentNullException(nameof(contextType));
            }

            TargetType = attributeDefine.TargetType ?? throw new ArgumentNullException(nameof(attributeDefine.TargetType));
            ContextType = contextType;
            Accessibility = GetAccessibility(attributeDefine, contextType);

            Namespace = GenerateNameSpace(attributeDefine, contextType);
            TypeName = GenerateTypeName(attributeDefine, contextType);
            TypeFullName = $"{Namespace}.{TypeName}";
        }

        #endregion Public 构造函数

        #region Protected 方法

        protected virtual string GenerateNameSpace(IllusionAttributeDefine attributeDefine, INamedTypeSymbol contextType)
        {
            if (attributeDefine.GeneratedTypeName is string proxyTypeName
                && string.IsNullOrWhiteSpace(proxyTypeName)
                && proxyTypeName.Contains("."))
            {
                return proxyTypeName.Substring(0, proxyTypeName.LastIndexOf('.'));
            }
            var implementTypeFullName = attributeDefine.TargetType.ToDisplayString();
            return implementTypeFullName.Substring(0, implementTypeFullName.LastIndexOf('.'));
        }

        protected virtual string GenerateTypeName(IllusionAttributeDefine attributeDefine, INamedTypeSymbol contextType)
        {
            var inheritType = attributeDefine.InheritType;
            var targetTypeName = attributeDefine.TargetType.Name;

            if (attributeDefine.GeneratedTypeName is not string proxyTypeName
                || string.IsNullOrWhiteSpace(proxyTypeName))
            {
                return inheritType is null
                       ? $"{targetTypeName}Illusion"
                       : $"{targetTypeName}As{inheritType.Name}Illusion";
            }
            else
            {
                return proxyTypeName.Contains('.')
                       ? proxyTypeName.Substring(proxyTypeName.LastIndexOf('.') + 1)
                       : proxyTypeName;
            }
        }

        protected virtual Accessibility GetAccessibility(IllusionAttributeDefine attributeDefine, INamedTypeSymbol contextType)
        {
            return attributeDefine.Accessibility switch
            {
                GeneratedAccessibility.InheritContext => contextType.DeclaredAccessibility,
                GeneratedAccessibility.InheritBase => attributeDefine.InheritType?.DeclaredAccessibility ?? Accessibility.Public,
                GeneratedAccessibility.Public => Accessibility.Public,
                GeneratedAccessibility.Internal => Accessibility.Internal,
                _ => attributeDefine.TargetType.DeclaredAccessibility,
            };
        }

        #endregion Protected 方法

        #region IEquatable

        public static bool operator !=(IllusionClassDescriptor? left, IllusionClassDescriptor? right)
        {
            return !(left == right);
        }

        public static bool operator ==(IllusionClassDescriptor? left, IllusionClassDescriptor? right)
        {
            return left is not null
                   && right is not null
                   && EqualityComparer<IllusionClassDescriptor>.Default.Equals(left, right);
        }

        public override bool Equals(object? obj)
        {
            return obj is IllusionClassDescriptor descriptor
                   && Equals(descriptor);
        }

        public virtual bool Equals(IllusionClassDescriptor descriptor)
        {
            return Accessibility == descriptor.Accessibility
                   && SymbolEqualityComparer.Default.Equals(ContextType, descriptor.ContextType)
                   && Namespace == descriptor.Namespace
                   && SymbolEqualityComparer.Default.Equals(TargetType, descriptor.TargetType)
                   && TypeFullName == descriptor.TypeFullName
                   && TypeName == descriptor.TypeName;
        }

        public override int GetHashCode()
        {
            int hashCode = 1771693360;
            hashCode = hashCode * -1521134295 + Accessibility.GetHashCode();
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(ContextType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Namespace);
            hashCode = hashCode * -1521134295 + SymbolEqualityComparer.Default.GetHashCode(TargetType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeFullName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
            return hashCode;
        }

        #endregion IEquatable
    }
}