﻿// <Auto-Generated></Auto-Generated>
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.Test;

public class GreeterFromServiceProvider : IGreeterFromServiceProvider
{
    private readonly IGreeter _greeter;

    public GreeterFromServiceProvider(IGreeter greeter)
    {
        _greeter = greeter ?? throw new ArgumentNullException(nameof(greeter));
    }

    public string Prop { get => _greeter.Prop; set => _greeter.Prop = value; }

    public string PropGet => _greeter.PropGet;

    public string PropSet { set => _greeter.PropSet = value; }

    public Task<string> AsyncMethod(string input)
    {
        return _greeter.AsyncMethod(input);
    }

    public Task<int[]?> AsyncMethod(int[]? input)
    {
        return _greeter.AsyncMethod(input);
    }

    public Task<int?> AsyncMethod(int? input)
    {
        return _greeter.AsyncMethod(input);
    }

    public Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation)
    {
        return _greeter.AsyncMethodCancelable(millisecondsDelay, cancellation);
    }

    public Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation, CancellationToken cancellation2)
    {
        return _greeter.AsyncMethodCancelable(millisecondsDelay, cancellation, cancellation2);
    }

    public Task AsyncMethodWithoutReturn(string input)
    {
        return _greeter.AsyncMethodWithoutReturn(input);
    }

    public Task<string> AwaitedAsyncMethod(string input)
    {
        return _greeter.AwaitedAsyncMethod(input);
    }

    public ValueTask<string> AwaitedValueTaskAsyncMethod(string input)
    {
        return _greeter.AwaitedValueTaskAsyncMethod(input);
    }

    public string Method(string input)
    {
        return _greeter.Method(input);
    }

    public string MethodWithAction(Action callback, string input)
    {
        return _greeter.MethodWithAction(callback, input);
    }

    public string MethodWithAction(Action<string> callback, string input)
    {
        return _greeter.MethodWithAction(callback, input);
    }

    public string MethodWithAction(Action<string, string> callback, string input)
    {
        return _greeter.MethodWithAction(callback, input);
    }

    public string MethodWithDefaultValue(string input1 = "input1", string input2 = null, int input3 = 123, CancellationToken cancellation = default)
    {
        return _greeter.MethodWithDefaultValue(input1, input2, input3, cancellation);
    }

    public string MethodWithDelegate(GetLongerString callback, string input)
    {
        return _greeter.MethodWithDelegate(callback, input);
    }

    public Task<string> MethodWithDelegateAsync(GetLongerStringTaskAsync callback, string input)
    {
        return _greeter.MethodWithDelegateAsync(callback, input);
    }

    public Task<string> MethodWithDelegateAsync(GetLongerStringValueTaskAsync callback, string input)
    {
        return _greeter.MethodWithDelegateAsync(callback, input);
    }

    public string MethodWithFunc(Func<string, string> callback, string input)
    {
        return _greeter.MethodWithFunc(callback, input);
    }

    public Task<string> MethodWithFuncAsync(Func<string, Task<string>> callback, string input)
    {
        return _greeter.MethodWithFuncAsync(callback, input);
    }

    public Task<string> MethodWithFuncAsync(Func<string, string, Task<string>> callback, string input)
    {
        return _greeter.MethodWithFuncAsync(callback, input);
    }

    public void MethodWithoutReturn(string input)
    {
        _greeter.MethodWithoutReturn(input);
    }

    public string TooManyArguments(string arg1, string arg2, string arg3, string arg4, string arg5, string arg6, string arg7, string arg8, string arg9, string arg10, string arg11, string arg12, string arg13, string arg14, string arg15, string arg16, string arg17, string arg18, string arg19, string arg20, string arg21, string arg22, string arg23, string arg24)
    {
        return $"arg1={arg1};arg2={arg2};arg3={arg3};arg4={arg4};arg5={arg5};arg6={arg6};arg7={arg7};arg8={arg8};arg9={arg9};arg10={arg10};arg11={arg11};arg12={arg12};arg13={arg13};arg14={arg14};arg15={arg15};arg16={arg16};arg17={arg17};arg18={arg18};arg19={arg19};arg20={arg20};arg21={arg21};arg22={arg22};arg23={arg23};arg24={arg24}";
    }

    public ValueTask<string> ValueTaskAsyncMethod(string input)
    {
        return _greeter.ValueTaskAsyncMethod(input);
    }

    public ValueTask ValueTaskAsyncMethodWithoutReturn(string input)
    {
        return _greeter.ValueTaskAsyncMethodWithoutReturn(input);
    }
}
