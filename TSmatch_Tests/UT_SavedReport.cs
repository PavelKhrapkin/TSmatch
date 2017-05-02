/*=================================
 *  Saved Report Unit Test 1.5.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.SaveReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;

namespace TSmatch.SaveReport.Tests
{
    [TestClass()]
    public class SavedReportTests
    {
        [TestMethod()]
        public void UT_getSavedReport()
        {
            var boot = new Bootstrap.Bootstrap();
            var sr = new SavedReport();
            //1/5            model = sr;
            //1/5            model.SetModel();
            sr.getSavedReport();
            Assert.Fail();
        }
        [TestMethod()]
        public void UT_Recover()
        {
            var boot = new Bootstrap.Bootstrap();
            var sr = new SavedReport();

            string repNm = Decl.TSMATCHINFO_MODELINFO;
            sr.Recover(repNm, SavedReport.RecoverToDo.ResetRep);
            Assert.IsTrue(Docs.IsDocExists(repNm));

            Docs modINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO);
        }
    }
}