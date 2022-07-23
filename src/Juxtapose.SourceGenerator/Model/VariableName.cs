using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Juxtapose.SourceGenerator.Model;

public class VariableName
{
    #region Private 字段

    private readonly Dictionary<string, string?> _storedNames;

    #endregion Private 字段

    #region Public 索引器

    public string? this[string key]
    {
        get => GetName(key);
        set => SetName(value, key);
    }

    #endregion Public 索引器

    #region Public 属性

    public string? Executor { get => GetName(); set => SetName(value); }

    public string? ExecutorOwner { get => GetName(); set => SetName(value); }

    /// <summary>
    /// 当前上下文中的实例
    /// </summary>
    public string? Instance { get => GetName(); set => SetName(value); }

    /// <summary>
    /// 当前上下文中的InstanceId
    /// </summary>
    public string? InstanceId { get => GetName(); set => SetName(value); }

    /// <summary>
    /// 进行交互的 Message 变量名称
    /// </summary>
    public string? Message { get => GetName(); set => SetName(value); }

    /// <summary>
    /// 方法体前缀代码片段
    /// </summary>
    public string? MethodBodyPrefixSnippet { get => GetName(); set => SetName(value); }

    public string? ParameterPack { get => GetName(); set => SetName(value); }

    public string? RunningToken { get => GetName(); set => SetName(value); }

    #endregion Public 属性

    #region Public 构造函数

    public VariableName()
    {
        _storedNames = new();

        ParameterPack = "@___parameterPack_";
        Executor = "@___executor_";
        InstanceId = "@___instanceId_";
        RunningToken = "@___runningToken_";
        ExecutorOwner = "@___executorOwner_";
        Instance = "@___instance_";
        Message = "@___message_";
    }

    public VariableName(IDictionary<string, string?> storedNames)
    {
        _storedNames = new(storedNames);
    }

    public VariableName(VariableName variableName)
    {
        _storedNames = new(variableName._storedNames);
    }

    #endregion Public 构造函数

    #region Private 方法

    private string? GetName([CallerMemberName] string propName = null!)
    {
        return _storedNames.TryGetValue(propName, out var name) ? name : null;
    }

    private void SetName(string? name, [CallerMemberName] string propName = null!)
    {
        _storedNames[propName] = name;
    }

    #endregion Private 方法
}