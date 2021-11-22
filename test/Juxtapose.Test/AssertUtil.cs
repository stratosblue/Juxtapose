using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Juxtapose.Test
{
    internal static class AssertUtil
    {
        #region Protected 方法

        public static void Equal<T>(T origin,
                                    T illusion,
                                    [CallerArgumentExpression("origin")] string originExpression = null,
                                    [CallerArgumentExpression("illusion")] string illusionExpression = null)
        {
            Debug.WriteLine("origin value:   {0} - {1}", origin, originExpression);
            Debug.WriteLine("illusion value: {0} - {1}", illusion, illusionExpression);
            Debug.WriteLine("   ----   ");
            Assert.AreEqual(origin, illusion, "{0} Not Equals {1}", originExpression, illusionExpression);
        }

        #endregion Protected 方法
    }
}