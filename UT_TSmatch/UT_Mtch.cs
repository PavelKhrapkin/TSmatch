/*=================================
* Match Unit Test 21.7.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Matcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileOp = match.FileOp.FileOp;

namespace TSmatch.Matcher.Tests
{
    [TestClass()]
    public class UT_Matcher
    {
        [TestMethod()]
        public void UT_Mtch()
        {
            var boot = new Boot();
            var model = new Mod(); ;


            FileOp.AppQuit();
        }
    }
}