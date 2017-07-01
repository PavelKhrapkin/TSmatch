/*=================================
 * Saved Report Unit Test 28.6.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.SaveReport;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Decl = TSmatch.Declaration.Declaration;
using Lib = match.Lib.MatchLib;
using FileOp = match.FileOp.FileOp;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
using SR = TSmatch.SaveReport.SavedReport;
using Boot = TSmatch.Bootstrap.Bootstrap;

namespace TSmatch.SaveReport.Tests
{
    [TestClass()]
    public class SavedReportTests
    {
        Boot boot;
        SR sr;
        Mod model;

        [TestMethod()]
        public void UT_getSavedReport()
        {
            var sr = init();

            sr.GetSavedReport(sr);


            Assert.IsTrue(sr.elementsCount > 0);
            Assert.AreEqual(sr.elementsCount, sr.elements.Count);
            Assert.IsTrue(sr.elmGroups.Count > 0);
            var total_price = sr.elmGroups.Sum(x => x.totalPrice);
            Assert.IsTrue(sr.elmGroups.Sum(x => x.totalPrice) > 0);

            FileOp.AppQuit();
        }

        //////////////[TestMethod()]
        //////////////public void UT_SR_ChechModel()
        //////////////{
        //////////////    var sr = init();

        //////////////    bool ok = sr.CheckModel(sr);
        //////////////    Assert.IsTrue(ok);

        //////////////    Docs dRaw = Docs.getDoc("Raw");
        //////////////    string path = dRaw.Path();
        // 12/5/17 ///    string dir = Path.GetDirectoryName(path);
        //////////////    string name = Path.GetFileName(path);
        //////////////    string nameSAV = Path.GetFileNameWithoutExtension(path) + "_SAV.xlsx";
        //////////////    dRaw.Close();
        //////////////    FileOp.fileRenSAV(dir, name);

        //////////////    ok = sr.CheckModel(sr);
        //////////////    if (boot.isTeklaActive)
        //////////////    {
        //////////////        Assert.IsTrue(ok);
        //////////////    }
        //////////////    else
        //////////////    {
        //////////////        Assert.IsFalse(ok);
        //////////////    }
        //////////////    // вернем TSmatchINFO.xlsx как было
        //////////////    FileOp.fileDelete(path);
        //////////////    FileOp.fileRename(dir, nameSAV, name);

        //////////////    FileOp.AppQuit();
        //////////////}

        [TestMethod()]
        public void UT_Recover()
        {
            var sr = init();

            // проверяем создание TSmatchINFO.xlsx/ModelINFO
            string repNm = Decl.TSMATCHINFO_MODELINFO;
            sr.Recover(repNm, SR.RecoverToDo.ResetRep);
            Assert.IsTrue(Docs.IsDocExists(repNm));
            Docs modINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO);
            string modName = modINFO.Body.Strng(2, 2);
            string dir = modINFO.Body.Strng(3, 2);
            string dat = modINFO.Body.Strng(5, 2);
            DateTime date = Lib.getDateTime(dat);
            int cnt = modINFO.Body.Int(7, 2);
            Assert.IsTrue(modName.Length > 0);
            Assert.IsTrue(dir.Length > 0);
            Assert.IsTrue(dir.Contains(@"\"));
            Assert.IsTrue(dir.Contains(":"));
            Assert.IsFalse(dir.Contains("."));
            Assert.IsTrue(dat.Length > 0);
            DateTime old = new DateTime(2010, 1, 1);
            Assert.IsTrue(date > old);
            Assert.IsTrue(date < DateTime.Now);

            //-- Raw теперь - отдельный xml файл, его не надо проверять 27.05.2017
            //// проверяем создание TSmatchINFO.xlsx/Raw
            //string raw = Decl.TSMATCHINFO_RAW;
            //// 4/5 долго: 2 мин            sr.Recover(raw, SR.RecoverToDo.ResetRep);
            //Assert.IsTrue(Docs.IsDocExists(raw));

            // проверяем создание TSmatchINFO.xlsx/Report
            string report = Decl.TSMATCHINFO_REPORT;
            sr.Recover(report, SR.RecoverToDo.ResetRep);
            Assert.IsTrue(Docs.IsDocExists(report));

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_SavedReport_SetFrSavedModelINFO()
        {
            var sr = init();
            model.dir = boot.ModelDir;

            if (boot.isTeklaActive) sr.SetFrSavedModelINFO(model.dir);
            // else -- sr.SetFrSavedModelINFO(model.dir); -- вызывается init-Model.SetModel

            Assert.IsTrue(Test_ModelINFO());

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_SavedReport_Raw()
        {
            var sr = init();
            model.dir = boot.ModelDir;
            if (boot.isTeklaActive) sr.SetFrSavedModelINFO(model.dir);
            // else -- sr.SetFrSavedModelINFO(model.dir); -- вызывается init-Model.SetModel
            Assert.IsTrue(Test_ModelINFO());

            model.elements = sr.Raw(model);

            Assert.IsTrue(model.elements.Count > 0);
            Assert.AreEqual(model.elementsCount, model.elements.Count);
            Docs docModelINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO);
            int cnt = docModelINFO.Body.Int(7, 2);
            Assert.AreEqual(model.elementsCount, cnt);
            string md5 = docModelINFO.Body.Strng(6, 2);
            Assert.AreEqual(md5, model.MD5);

            FileOp.AppQuit();
        }

        // проверяем как дополняются eleGroups из листа TSmatchINFO.xlsx/Report
        // 2017.06.28 переписан sr.getSavedGroups() -- getGrps + читаю из файла Report
        [TestMethod()]
        public void UT_SavedReport_getSavedGroup()
        {
            var sr = init();
            model.dir = boot.ModelDir;
            sr.elements = sr.Raw(model);

            sr.getSavedGroups();

            double sumPrice = sr.elmGroups.Select(x => x.totalPrice).Sum();
            Assert.AreEqual(sumPrice, sr.total_price);
            Docs dRep = Docs.getDoc("Report");
            // в Report <Заголовок> + строки по числу elmGroups.Count + <Summary>
            int cnt = sr.elmGroups.Count + dRep.i0;
            Assert.AreEqual(dRep.il, cnt);

            FileOp.AppQuit();
        }

        // проверяем содержимое TSmatchINFO.xlsx/ModelINFO
        private bool Test_ModelINFO()
        {
            string repNm = Decl.TSMATCHINFO_MODELINFO;
            Assert.IsTrue(Docs.IsDocExists(repNm));
            Docs modINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO);
            string modName = modINFO.Body.Strng(2, 2);
            string dir = modINFO.Body.Strng(3, 2);
            string dat = modINFO.Body.Strng(5, 2);
            DateTime date = Lib.getDateTime(dat);
            int cnt = modINFO.Body.Int(7, 2);
            Assert.IsTrue(modName.Length > 0);
            Assert.IsTrue(dir.Length > 0);
            Assert.IsTrue(dir.Contains(@"\"));
            Assert.IsTrue(dir.Contains(":"));
            //8/5/17            Assert.IsFalse(dir.Contains(".")); //Попалась такая модель с точкой в названии!
            Assert.IsTrue(dat.Length > 0);
            DateTime old = new DateTime(2010, 1, 1);
            Assert.IsTrue(date > old);
            Assert.IsTrue(date < DateTime.Now);
            return true;
        }

        // эта инициализация класса SavedReport общая для всех тестов этого класса
        private SR init()
        {
            boot = new Bootstrap.Bootstrap();
            sr = new SavedReport();
            model = sr;
            model.SetModel(boot);
            return sr;
        }
    }
}