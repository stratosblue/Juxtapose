﻿using System;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.Test;

public delegate string GetLongerString(string str1, string str2);

public delegate Task<string> GetLongerStringTaskAsync(string str1, string str2);

public delegate ValueTask<string> GetLongerStringValueTaskAsync(string str1, string str2);

public interface IGreeter
{
    #region Public 属性

    string Prop { get; set; }

    string PropGet { get; }

    string PropSet { set; }

    #endregion Public 属性

    #region Public 方法

    Task<string> AsyncMethod(string input);

    Task<int[]?> AsyncMethod(int[]? input);

    Task<int?> AsyncMethod(int? input);

    Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation);

    Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation, CancellationToken cancellation2);

    Task AsyncMethodWithoutReturn(string input);

    Task<string> AwaitedAsyncMethod(string input);

    ValueTask<string> AwaitedValueTaskAsyncMethod(string input);

    string Method(string input);

    string MethodWithAction(Action callback, string input);

    string MethodWithAction(Action<string> callback, string input);

    string MethodWithAction(Action<string, string> callback, string input);

    string MethodWithDefaultValue(string input1 = "input1", string? input2 = null, int input3 = 123, CancellationToken cancellation = default);

    string MethodWithDelegate(GetLongerString callback, string input);

    Task<string> MethodWithDelegateAsync(GetLongerStringTaskAsync callback, string input);

    Task<string> MethodWithDelegateAsync(GetLongerStringValueTaskAsync callback, string input);

    string MethodWithFunc(Func<string, string> callback, string input);

    Task<string> MethodWithFuncAsync(Func<string, Task<string>> callback, string input);

    Task<string> MethodWithFuncAsync(Func<string, string, Task<string>> callback, string input);

    void MethodWithoutReturn(string input);

    string TooManyArguments(string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, string arg10, string arg11, string arg12, string arg13, string arg14, string arg15, string arg16, string arg17, string arg18, string arg19, string arg20, string arg21, string arg22, string arg23, string arg24);

    ValueTask<string> ValueTaskAsyncMethod(string input);

    ValueTask ValueTaskAsyncMethodWithoutReturn(string input);

    #endregion Public 方法
}
