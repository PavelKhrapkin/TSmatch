/*=================================
 * ProfileUpdate Unit Test 2.07.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using match.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace match.Lib.Tests
{
    [TestClass()]
    public class UT_MatchLib
    {
        [TestMethod()]
        public void UT_MatchLib_GetPars()
        {
            string str = "I10_8239_89";
            var pars = MatchLib.GetPars(str);
            Assert.AreEqual(1, pars.Count);
            Assert.AreEqual(10, pars[0]);
        }

        [TestMethod()]
        public void UT_MatchLib_GetParsStr()
        {
            // test 1
            string str = "I10.5_8239_89";
            List<string> pars = MatchLib.GetParsStr(str);
            Assert.AreEqual(1, pars.Count);
            Assert.AreEqual("10.5", pars[0]);

            // test 2
            str = "Prf:U10.5x7,8_8239_89;";
            pars = MatchLib.GetParsStr(str);
            Assert.AreEqual(2, pars.Count);
            Assert.AreEqual("10.5", pars[0]);
            Assert.AreEqual("7,8", pars[1]);

            // test 3
            str = "I10_8239_89";
            pars = MatchLib.GetParsStr(str);
            Assert.AreEqual(1, pars.Count);
            Assert.AreEqual("10", pars[0]);
        }

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

            // test 3:
            lst = new List<string>() { "балка", "i" };
            v = "i20";
            b = MatchLib.IContains(lst, v);
            Assert.IsTrue(b);
        }
    }
}