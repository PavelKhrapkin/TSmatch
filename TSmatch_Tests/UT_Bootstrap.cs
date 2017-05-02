using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileOp = match.FileOp.FileOp;

namespace TSmatch.Bootstrap.Tests
{
    [TestClass()]
    public class BootstrapTests
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