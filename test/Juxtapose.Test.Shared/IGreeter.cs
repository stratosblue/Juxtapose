using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.Test
{
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

        Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation);

        Task<int> AsyncMethodCancelable(int millisecondsDelay, CancellationToken cancellation, CancellationToken cancellation2);

        Task AsyncMethodWithoutReturn(string input);

        Task<string> AwaitedAsyncMethod(string input);

        ValueTask<string> AwaitedValueTaskAsyncMethod(string input);

        string Method(string input);

        string MethodWithDefaultValue(string input1 = "input1", string? input2 = null, int input3 = 123);

        string MethodWithAction(Action callback, string input);

        string MethodWithAction(Action<string> callback, string input);

        string MethodWithAction(Action<string, string> callback, string input);

        string MethodWithDelegate(GetLongerString callback, string input);

        Task<string> MethodWithDelegateAsync(GetLongerStringTaskAsync callback, string input);

        Task<string> MethodWithDelegateAsync(GetLongerStringValueTaskAsync callback, string input);

        string MethodWithFunc(Func<string, string> callback, string input);

        Task<string> MethodWithFuncAsync(Func<string, Task<string>> callback, string input);

        Task<string> MethodWithFuncAsync(Func<string, string, Task<string>> callback, string input);

        void MethodWithoutReturn(string input);

        ValueTask<string> ValueTaskAsyncMethod(string input);

        ValueTask ValueTaskAsyncMethodWithoutReturn(string input);

        #endregion Public 方法
    }
}