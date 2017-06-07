using Microsoft.VisualStudio.TestTools.UnitTesting;
using match.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//MatchLib.using Lib = match.Lib.MatchLib;

namespace match.Lib.Tests
{
    [TestClass()]
    public class UT_MatchLib
    {
        [TestMethod()]
        public void UT_IContains()
        {
            // test 1: 
            string v = "yгoлoк75x6";
            v = MatchLib.ToLat(v);
            List<string> lst0 = new List<string>() { "угoлoк", "l" };
            lst0[0] = MatchLib.ToLat(lst0[0]);
            bool b0 = MatchLib.IContains(lst0, v);
            Assert.IsTrue(b0);

            // test 2:
            List<string> lst = new List<string>() { "уголок", "l" };
            v = "уголок";
            bool b = MatchLib.IContains(lst, v);
            Assert.IsTrue(b);
        }
    }
}