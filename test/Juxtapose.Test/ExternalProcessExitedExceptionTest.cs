using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Juxtapose.Test;

[TestClass]
public class ExternalProcessExitedExceptionTest
{
    #region Private 属性

    private GreeterIllusion _illusion;

    #endregion Private 属性

    #region Public 方法

    [TestCleanup]
    public void Cleanup()
    {
        _illusion?.Dispose();
        _illusion = null;
    }

    [TestInitialize]
    public async Task Init()
    {
        _illusion = await GreeterIllusion.NewAsync("CSharp");
    }

    [TestMethod]
    public async Task ShouldThrowExternalProcessExitedException()
    {
        if (!_illusion.TryGetExternalProcess(out var externalProcess))
        {
            throw new InvalidOperationException("can not get externalProcess.");
        }
        var process = Process.GetProcessById(externalProcess.Id);

        var assertTask = Assert.ThrowsExactlyAsync<ExternalProcessExitedException>(async () => await _illusion.AsyncMethodCancelable(2000, CancellationToken.None));

        Console.WriteLine($"Kill process {externalProcess.Id}");

        await Task.Delay(100);

        process.Kill();

        var exception = await assertTask;

        Console.WriteLine(exception.Message);
    }

    #endregion Public 方法
}
