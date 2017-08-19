/*=========================================
 * Model Unit Tekla = TS_OpenAPI 28.7.2017
 *=========================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            FileOp.AppQuit();
        }
    }
}