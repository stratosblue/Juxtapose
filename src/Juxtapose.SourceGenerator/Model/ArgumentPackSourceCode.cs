namespace Juxtapose.SourceGenerator.Model
{
    /// <summary>
    /// 参数包源代码
    /// </summary>
    public abstract class ArgumentPackSourceCode : PartialSourceCode
    {
        #region Public 构造函数

        public ArgumentPackSourceCode(string hintName, string source, string @namespace, string typeName, string typeFullName)
            : base(hintName, source, @namespace, typeName, typeFullName)
        {
        }

        #endregion Public 构造函数
    }
}