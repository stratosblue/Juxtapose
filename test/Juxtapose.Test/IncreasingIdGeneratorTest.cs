using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Juxtapose.Test;

[TestClass]
public class IncreasingIdGeneratorTest
{
    #region Private 字段

    private const int Count = short.MaxValue * byte.MaxValue;

    private const int InitId = Constants.IdThreshold - Count / 2;

    #endregion Private 字段

    #region Public 方法

    [TestMethod]
    public void ShouldMessageIdResetAfterThreshold()
    {
        var generator = new IncreasingIdGenerator();
        typeof(IncreasingIdGenerator).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(generator, InitId);

        var allData = new int[Count];
        var index = -1;
        Parallel.For(0, Count, _ =>
        {
            allData[Interlocked.Increment(ref index)] = generator.Next();
        });

        Assert.AreEqual(Count, allData.Distinct().Count());
        Assert.IsTrue(allData.Where(m => m < InitId).Any());
    }

    [TestMethod]
    public void ShouldUniqueMessageId()
    {
        var generator = new IncreasingIdGenerator();
        var allData = new int[Count];
        var index = -1;
        Parallel.For(0, Count, _ =>
        {
            allData[Interlocked.Increment(ref index)] = generator.Next();
        });

        Assert.AreEqual(Count, allData.Distinct().Count());
    }

    #endregion Public 方法
}
