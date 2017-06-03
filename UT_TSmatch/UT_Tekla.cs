/*=========================================
 * Model Unit Tekla = TS_OpenAPI 2.6.2017
 *=========================================
 */
 using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Tekla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string path = @"C:\Users\khrapkin\Desktop\test.txt";
            TS ts = new TS();
            ts.WriteToReport(path);
            Assert.IsTrue(FileOp.isFileExist(path));
            var rd = ts.ReadReport(path);
        }
    }
}