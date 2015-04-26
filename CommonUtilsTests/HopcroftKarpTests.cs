using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonUtilsTests
{
    [TestClass]
    public class HopcroftKarpTests
    {
        [TestMethod]
        public void TestOneIterationGraph()
        {
            var matching = CommonUtils.HopcroftKarp.GetMatching(
                new List<int[]>()
                {
                    new int[] {0,2},
                    new int[] {0,3},
                    new int[] {1,2},
                    new int[] {3},
                    new int[] {2, 3}
                }, 5
                );
            Assert.IsTrue(Enumerable.SequenceEqual(matching, new int[] {0, 3, 1, -1, 2 }));
        }

        [TestMethod]
        public void TestTwoIterationGraph()
        {
            var matching = CommonUtils.HopcroftKarp.GetMatching(
                new List<int[]>()
                {
                    new int[] {0,1},
                    new int[] {0,4},
                    new int[] {2,3},
                    new int[] {0,4},
                    new int[] {1, 3}
                }, 5
                );
            Assert.IsTrue(Enumerable.SequenceEqual(matching, new int[] { 1, 4, 2, 0, 3 }));
        }
    }
}
