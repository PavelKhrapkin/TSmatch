/*=================================
* Bootstrap Test 17.7.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FileOp = match.FileOp.FileOp;

namespace TSmatch.Bootstrap.Tests
{
    [TestClass()]
    public class UT_BootstrapTests
    {
        [TestMethod()]
        public void UT_Bootstrap()
        {
            var boot = new Bootstrap();

            Assert.IsTrue(boot.ModelDir.Length > 0);
            Assert.IsTrue(boot.TOCdir.Length > 0);
            Assert.IsNotNull(boot.docTSmatch);
            Assert.AreEqual(boot.docTSmatch.name, "TOC");
            Assert.IsTrue(boot.docTSmatch.il > 20);

            FileOp.AppQuit();
        }
    }
}