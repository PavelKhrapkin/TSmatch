/*=================================
 * Rules Unit Test 19.6.2017
 *=================================
 */
 using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSmatch.Rule.Tests
{
    [TestClass()]
    public class UT_Rule
    {
        [TestMethod()]
        public void UT_RuleSynParse()
        {
            Rule rule = new Rule();
            string txt = "Prf: PL =—*x * ";
            Assert.Fail();
        }
    }
}