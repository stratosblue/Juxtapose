using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.Test;

public class Greeter : IGreeter
{
    #region Private 字段

    private string _propField;

    #endregion Private 字段

    #region Public 属性

    public string Prop { get => _propField; set => _propField = value; }

    public string PropGet => _propField;

    public string PropSet { set => _propField = value; }

    #endregion Public 属性

    #region Public 构造函数

    static Greeter()
    { }

    public Greeter()
    {
        _propField = string.Empty;
    }

    public Greeter(string prop)
    {
        _propField = prop;
    }

    #endregion Public 构造函数

    #region Private 方法

    private static string Reserve(string input) => input is null ? null : new string(input.Reverse().ToArray());

    #endregion Private 方法

    #region Public 方法

    public Task<string> AsyncMethod(string input)
    {
        return Task.FromResult(Reserve(input));
    }

    public Task<int[]?> AsyncMethod(int[]? input)
    {
        return Task.FromResult(input.Reverse().ToArray());
    }

    public Task<int?> AsyncMethod(int? input)
    {
        return Task.FromResult(input);
    }

    public async Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation)
    {
        await Task.Delay(millisecondsDelay, cancellation);
        return millisecondsDelay;
    }

    public async Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation, CancellationToken cancellation2)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation, cancellation2);
        await Task.Delay(millisecondsDelay, cts.Token);
        return millisecondsDelay;
    }

    public Task AsyncMethodWithoutReturn(string input)
    {
        _propField = input;
        return Task.CompletedTask;
    }

    public async Task<string> AwaitedAsyncMethod(string input)
    {
        await Task.CompletedTask;
        return Reserve(input);
    }

    public async ValueTask<string> AwaitedValueTaskAsyncMethod(string input)
    {
        await Task.CompletedTask;
        return Reserve(input);
    }

    public string Method(string input)
    {
        return Reserve(input);
    }

    public string MethodWithAction(Action callback, string input)
    {
        callback();
        return Reserve(input);
    }

    public string MethodWithAction(Action<string> callback, string input)
    {
        callback(Reserve(input));
        return Reserve(input);
    }

    public string MethodWithAction(Action<string, string> callback, string input)
    {
        callback(input, Reserve(input));
        return Reserve(input);
    }

    public string MethodWithDefaultValue(string input1 = "input1", string? input2 = null, int input3 = 123, CancellationToken cancellation = default)
    {
        return input1 + input2 + input3 + cancellation.CanBeCanceled;
    }

    public string MethodWithDelegate(GetLongerString callback, string input)
    {
        return callback(input, Reserve(input));
    }

    public Task<string> MethodWithDelegateAsync(GetLongerStringTaskAsync callback, string input)
    {
        return callback(input, Reserve(input));
    }

    public async Task<string> MethodWithDelegateAsync(GetLongerStringValueTaskAsync callback, string input)
    {
        return await callback(input, Reserve(input));
    }

    public string MethodWithFunc(Func<string, string> callback, string input)
    {
        return callback(Reserve(input));
    }

    public Task<string> MethodWithFuncAsync(Func<string, Task<string>> callback, string input)
    {
        return callback(Reserve(input));
    }

    public Task<string> MethodWithFuncAsync(Func<string, string, Task<string>> callback, string input)
    {
        return callback(input, Reserve(input));
    }

    public void MethodWithoutReturn(string input)
    {
        _propField = input;
    }

    public string TooManyArguments(string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, string arg10, string arg11, string arg12, string arg13, string arg14, string arg15, string arg16, string arg17, string arg18, string arg19, string arg20, string arg21, string arg22, string arg23, string arg24)
    {
        return $"arg1={arg1};arg2={arg2};arg3={arg3};arg4={arg4};arg5={arg5};arg6={arg6};arg7={arg7};arg8={arg8};arg9={arg9};arg10={arg10};arg11={arg11};arg12={arg12};arg13={arg13};arg14={arg14};arg15={arg15};arg16={arg16};arg17={arg17};arg18={arg18};arg19={arg19};arg20={arg20};arg21={arg21};arg22={arg22};arg23={arg23};arg24={arg24}";
    }

    public ValueTask<string> ValueTaskAsyncMethod(string input)
    {
        return new ValueTask<string>(Reserve(input));
    }

    public ValueTask ValueTaskAsyncMethodWithoutReturn(string input)
    {
        _propField = input;
        return ValueTask.CompletedTask;
    }

    #endregion Public 方法
}
