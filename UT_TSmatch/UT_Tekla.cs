using TSmatch.Tekla;
/*=========================================
* Model Unit Tekla = TS_OpenAPI 5.9.2017
*=========================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

using TS = TSmatch.Tekla.Tekla;
using FileOp = match.FileOp.FileOp;

namespace TSmatch.Tekla.Tests
{
    [TestClass()]
    public class UT_Tekla
    {
        [TestMethod()]
        public void UT_WriteToReport()
        {
            ////string path = @"C:\Users\khrapkin\Desktop\test.txt";
            ////TS ts = new TS();
            ////ts.WriteToReport(path);
            ////Assert.IsTrue(FileOp.isFileExist(path));
            ////var rd = ts.ReadReport(path);

            ////FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Example1()
        {
            var ts = new TS();
            //            ts.Example1();

        }

        [TestMethod()]
        public void UT_ReadCustomEmbeds()
        {
            var embeds = TS.ReadCustomEmbeds();
            int cnt = embeds.Count;
            Assert.IsTrue(cnt > 0);
            var embGrps = embeds.GroupBy(x => x.Name);
            var quot = new Dictionary<string, int>();
            foreach(var p in embGrps)
            {
                string part = p.Key;
                int n = embeds.Count(x => x.Name == part);
                quot.Add(p.Key, n);
            }
            Assert.Fail();
        }
    }
}