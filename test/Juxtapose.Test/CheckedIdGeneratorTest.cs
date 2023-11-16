using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Juxtapose.Test;

[TestClass]
public class CheckedIdGeneratorTest
{
    #region Private 字段

    private const int Count = short.MaxValue * byte.MaxValue;

    private const int ExistCount = byte.MaxValue * 16;

    #endregion Private 字段

    #region Public 方法

    [TestMethod]
    public void ShouldUniqueMessageId()
    {
        var count = Count + ExistCount;

        var dictionary = new ConcurrentDictionary<int, object>();
        var random = new Random();
        while (dictionary.Count < ExistCount)
        {
            var id = random.Next(1, count);
            while (dictionary.ContainsKey(id))
            {
                id = random.Next(1, count);
            }
            dictionary.TryAdd(id, null);
        }
        var generator = new CheckedIdGenerator(dictionary.ContainsKey);

        var allData = new int[Count + ExistCount];

        var index = -1;

        Parallel.For(0, count, _ =>
        {
            allData[Interlocked.Increment(ref index)] = generator.Next();
        });

        Assert.AreEqual(count, allData.Distinct().Count());
        Assert.AreEqual(0, allData.Where(m => dictionary.ContainsKey(m)).Count());
    }

    #endregion Public 方法
}
