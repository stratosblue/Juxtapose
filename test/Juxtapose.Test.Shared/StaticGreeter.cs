//批量替换
//(private|public|protected|internal)
//->
//$1 static

namespace Juxtapose.Test;

public static class StaticGreeter
{
    #region Private 字段

    private static string s_propField;

    #endregion Private 字段

    #region Public 属性

    public static string Prop { get => s_propField; set => s_propField = value; }

    public static string PropGet => s_propField;

    public static string PropSet { set => s_propField = value; }

    #endregion Public 属性

    #region Public 构造函数

    static StaticGreeter()
    {
        s_propField = string.Empty;
    }

    #endregion Public 构造函数

    #region Private 方法

    private static string Reserve(string input) => input is null ? null : new string(input.Reverse().ToArray());

    #endregion Private 方法

    #region Public 方法

    public static Task<string> AsyncMethod(string input)
    {
        return Task.FromResult(Reserve(input));
    }

    public static Task<int[]?> AsyncMethod(int[]? input)
    {
        return Task.FromResult(input.AsEnumerable().Reverse().ToArray());
    }

    public static Task<int?> AsyncMethod(int? input)
    {
        return Task.FromResult(input);
    }

    public static async Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation)
    {
        await Task.Delay(millisecondsDelay, cancellation);
        return millisecondsDelay;
    }

    public static async Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation, CancellationToken cancellation2)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation, cancellation2);
        await Task.Delay(millisecondsDelay, cts.Token);
        return millisecondsDelay;
    }

    public static Task AsyncMethodWithoutReturn(string input)
    {
        s_propField = input;
        return Task.CompletedTask;
    }

    public static async Task<string> AwaitedAsyncMethod(string input)
    {
        await Task.CompletedTask;
        return Reserve(input);
    }

    public static async ValueTask<string> AwaitedValueTaskAsyncMethod(string input)
    {
        await Task.CompletedTask;
        return Reserve(input);
    }

    public static string Method(string input)
    {
        return Reserve(input);
    }

    public static string MethodWithAction(Action callback, string input)
    {
        callback();
        return Reserve(input);
    }

    public static string MethodWithAction(Action<string> callback, string input)
    {
        callback(input);
        return Reserve(input);
    }

    public static string MethodWithAction(Action<string, string> callback, string input)
    {
        callback(input, Reserve(input));
        return Reserve(input);
    }

    public static string MethodWithDefaultValue(string input1 = "input1", string? input2 = null, int input3 = 123, CancellationToken cancellation = default)
    {
        return input1 + input2 + input3 + cancellation.CanBeCanceled;
    }

    public static string MethodWithDelegate(GetLongerString callback, string input)
    {
        return callback(input, Reserve(input));
    }

    public static Task<string> MethodWithDelegateAsync(GetLongerStringTaskAsync callback, string input)
    {
        return callback(input, Reserve(input));
    }

    public static async Task<string> MethodWithDelegateAsync(GetLongerStringValueTaskAsync callback, string input)
    {
        return await callback(input, Reserve(input));
    }

    public static string MethodWithFunc(Func<string, string> callback, string input)
    {
        return callback(Reserve(input));
    }

    public static Task<string> MethodWithFuncAsync(Func<string, Task<string>> callback, string input)
    {
        return callback(input);
    }

    public static Task<string> MethodWithFuncAsync(Func<string, string, Task<string>> callback, string input)
    {
        return callback(input, Reserve(input));
    }

    public static void MethodWithoutReturn(string input)
    {
        s_propField = input;
    }

    public static ValueTask<string> ValueTaskAsyncMethod(string input)
    {
        return new ValueTask<string>(Reserve(input));
    }

    public static ValueTask ValueTaskAsyncMethodWithoutReturn(string input)
    {
        s_propField = input;
        return ValueTask.CompletedTask;
    }

    #endregion Public 方法
}
