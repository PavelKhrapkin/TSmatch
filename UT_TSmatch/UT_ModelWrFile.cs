/*=================================
 *ModWrFile Unit Test 9.8.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Model.WrModelInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSmatch.Model.WrModelInfo.Tests
{
    [TestClass()]
    public class UT_ModelWrFile
    {
        [TestMethod()]
        public void UT_sInt()
        {
            var x = new ModelWrFile();
            var s = x.sInt(0);
            Assert.AreEqual("0", s);

            s = x.sInt(1234567890);
            Assert.AreEqual("1 234 567 890", s);
        }

        [TestMethod()]
        public void UT_sDbl()
        {
            var x = new ModelWrFile();
            var s = x.sDbl(0);
            Assert.AreEqual("0,00", s);

            s = x.sDbl(-1234567890.2234);
            Assert.AreEqual("-1 234 567 890,22", s);
        }

        [TestMethod()]
        public void UT_sDbl00()
        {
            var x = new ModelWrFile();
            var s = x.sDbl00(0);
            Assert.AreEqual("0", s);

            s = x.sDbl00(-1234567890.2234);
            Assert.AreEqual("-1 234 567 890", s);
        }

        [TestMethod()]
        public void UT_sDat()
        {
            var x = new ModelWrFile();
            var s = x.sDat(DateTime.Now);
            string should_be = DateTime.Now.ToString("d.MM.yyyy HH:mm");
            Assert.AreEqual(should_be, s);

            s = x.sDat(new DateTime(2016,5,22,18,45,15));
            Assert.AreEqual("22.05.2016 18:45", s);
        }
    }
}