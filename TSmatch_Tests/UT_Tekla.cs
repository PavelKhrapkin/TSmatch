/*=========================================
* Model Unit Tekla = TS_OpenAPI 5.10.2017
*=========================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Model;

namespace TSmatch.Tekla.Tests
{
    [TestClass()]
    public class UT_Tekla
    {
        /// <summary>
        /// this test return true if Tekla is Active, false if not,
        /// and various type of fault, when wrong Tekla dll is in use
        /// </summary>
        [TestMethod()]
        public void UT_isTeklaActive()
        {
            bool b = Tekla.isTeklaActive();
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void UT_ReadModObj()
        {
            var u = new _UT_Tekla();
            //var parts = u._ReadModObj();
            //Assert.IsTrue(parts.Count() > 0);
            //foreach (var p in parts)
            {
//                Assert.IsTrue(p.Key.Length > 15);
            }
        }

        /// <summary>
        /// UT_Read - test reading from model in Tekla.
        /// <para>
        /// To check time of read from Tekla, stop after string dt. dt = elapsed time in sec 
        /// </para>
        /// </summary>
        [TestMethod()]
        public void UT_Read()
        {
            // test 1: just read Parts
            var u = new _UT_Tekla();
            DateTime t0 = DateTime.Now;
            u.Read();
            DateTime t1 = DateTime.Now;
            string dt = (t1 - t0).ToString();
            Assert.AreEqual(u.elementsCount(), u.dicPartsCnt());
            Assert.IsTrue(u.dicPartsCnt() > 0);
            // test 2: read embeds - Class 100 and class 101
            int c100 = u.Class100cnt("100");
            int c101 = u.Class100cnt("101");
            u.GrClass();    // Grouping Embeds by Class - not in use
        }
    }

    public class _UT_Tekla: Tekla
    {
        public Dictionary<string, Part> _ReadModObj()
        {
            return ReadModObj<Part>();
        }

        public int dicPartsCnt() { return dicParts.Count; }

        public int Class100cnt(string c) { return dicParts.Count(x => x.Value.Class == c); }

        public void GrClass()
        {
            var grClasses = dicParts.GroupBy(x => x.Value.Class);
            foreach (var gr in grClasses)
            {
                string cl = gr.Key;
                int grCnt = gr.Count();
            }
        }
    }
}