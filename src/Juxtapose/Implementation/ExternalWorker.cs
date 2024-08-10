using Juxtapose.Utils;

namespace Juxtapose;

/// <inheritdoc cref="IExternalWorker"/>
public class ExternalWorker : KeepRunningObject, IExternalWorker
{
    #region Public 事件

    /// <inheritdoc/>
    public event Action<object>? OnInvalid;

    #endregion Public 事件

    #region Protected 属性

    /// <inheritdoc cref="IMessageExchanger"/>
    protected IMessageExchanger MessageExchanger { get; }

    #endregion Protected 属性

    #region Public 属性

    /// <inheritdoc/>
    public IExternalProcess ExternalProcess { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="ExternalWorker"/>
    public ExternalWorker(IExternalProcess externalProcess, IMessageExchanger messageExchanger)
    {
        ExternalProcess = externalProcess ?? throw new ArgumentNullException(nameof(externalProcess));
        MessageExchanger = messageExchanger ?? throw new ArgumentNullException(nameof(messageExchanger));

        externalProcess.OnProcessInvalid += OnExternalProcessInvalid;
    }

    #endregion Public 构造函数

    #region Private 方法

    private void OnExternalProcessInvalid(IExternalProcess _)
    {
        Dispose();
    }

    #endregion Private 方法

    #region Protected 方法

    /// <inheritdoc/>
    protected override bool Dispose(bool disposing)
    {
        if (base.Dispose(disposing))
        {
            ExternalProcess.OnProcessInvalid -= OnExternalProcessInvalid;

            OnInvalid?.Invoke(this);

            ExternalProcess.Dispose();
            MessageExchanger.Dispose();

            return true;
        }
        return false;
    }

    #endregion Protected 方法

    #region Public 方法

    /// <inheritdoc/>
    public IAsyncEnumerable<object> GetMessagesAsync(CancellationToken cancellation) => MessageExchanger.GetMessagesAsync(cancellation);

    /// <inheritdoc/>
    public virtual async Task InitializationAsync(CancellationToken initializationToken)
    {
        try
        {
            var messageExchangerInitTask = MessageExchanger.InitializationAsync(initializationToken);

            var externalProcessInitTask = ExternalProcess.InitializationAsync(initializationToken);

            await Task.WhenAll(messageExchangerInitTask, externalProcessInitTask);
        }
        catch (Exception ex)
        {
            if (!ExternalProcess.IsAlive)
            {
                int exitCode;
                string exitDescription;
                try
                {
                    exitCode = ExternalProcess.ExitCode;
                    exitDescription = ExternalProcessExitCodeUtil.GetExitCodeDescription(exitCode);
                }
                catch (Exception iex)
                {
                    var newException = new ExternalProcessExitedException(ExternalProcess.Id, -1, "ExternalWorker Initialization Fail And Get ExitCode Fail.", iex);
                    newException.Data.Add("RawException", ex);
                    throw newException;
                }
                throw new ExternalProcessExitedException(ExternalProcess.Id, exitCode, $"ExternalWorker Initialization Fail: {exitDescription}", ex);
            }
            throw;
        }
    }

    /// <inheritdoc/>
    public Task WriteMessageAsync(object message, CancellationToken cancellation) => MessageExchanger.WriteMessageAsync(message, cancellation);

    #endregion Public 方法
}
