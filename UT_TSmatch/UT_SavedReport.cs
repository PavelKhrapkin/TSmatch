/*=================================
 * Saved Report Unit Test 23.7.2017
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
using MH = TSmatch.Handler.Handler;
using Boot = TSmatch.Bootstrap.Bootstrap;
using TSmatch.Model;

namespace TSmatch.SaveReport.Tests
{
     [TestClass()]
    public class UT_SavedReportTests
    {
        Boot boot;
        SR sr;
        MH mh;
        Mod model;

        [TestMethod()]
        public void UT_GetSavedReport()
        {
            var sr = init();
            model.SetModDir(boot);
            model = sr.GetModelINFO(model);
            ////////////////model.elements = sr.Raw(model);
            ////////////////mh = new MH();
            ////////////////model.elmGroups = mh.getGrps(model.elements);

            model = sr.GetSavedReport();
            

            Assert.IsTrue(model.elmGroups.Count > 0);
            var total_price = model.elmGroups.Sum(x => x.totalPrice);
            //           Assert.IsTrue(model.elmGroups.Sum(x => x.totalPrice) > 0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_GetModelINFO()
        {
            // GetModelINFO() - базовый метод, вызываемый в SetModel.
            //..поэтому пользоваться обычным init() для этого UT_ нельзя 
            const string defaultModName = "MyTestName";
            boot = new Boot();
            var sr = new SR();
            model = new Mod();

            model.dir = boot.ModelDir;
            if (string.IsNullOrEmpty(model.dir)) model.dir = boot.DebugDir;
            Assert.IsTrue(model.dir.Length > 0);
            if (!FileOp.isFileExist(model.dir, "TSmatchINFO.xlsx"))
            {
                model.name = defaultModName;
                model.adrCity = "Санкт-Петербург";
                model.adrStreet = "Кудрово";
                model.date = DateTime.Now;
                model.MD5 = "sample-MD5";
            }

            model = sr.GetModelINFO(model);

            Docs dINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO);
            Assert.IsNotNull(dINFO);
            Assert.AreEqual(2, dINFO.i0);
            Assert.IsTrue(dINFO.il > 9);
            var b = dINFO.Body;
            string b_name = b.Strng(Decl.MODINFO_NAME_R, 2);
            Assert.AreEqual("Название модели =", b.Strng(Decl.MODINFO_NAME_R, 1));
            Assert.IsTrue(b_name.Length >= 1);
            Assert.AreEqual("Адрес проекта:", b.Strng(Decl.MODINFO_ADDRESS_R, 1));
            Assert.IsTrue(b.Strng(Decl.MODINFO_ADDRESS_R, 2).Length >= 1);
            sr.CheckModelIntegrity();
            Assert.IsTrue(model.elements.Count > 0);
            Assert.AreEqual(model.MD5, dINFO.Body.Strng(Decl.MODINFO_MD5_R, 2));
            Assert.IsTrue(model.elmGroups.Count > 0);
//24/7            Assert.AreEqual(model.pricingMD5, dINFO.Body.Strng(Decl.MODINFO_PRCMD5_R, 2));

            if (b_name == defaultModName) FileOp.Delete(model.dir, b_name);

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
            model.SetModDir(boot);
            model.date = new DateTime(2015, 6, 12, 14, 15, 16);
            model.MD5 = "-- моя имитация MD5 --";
            model.pricingMD5 = "-- моя имитация MD5 --";
            model.pricingDate = new DateTime(2017, 4, 4, 20, 19, 18);
            model.setCity("Санкт-Петербург, Зенит-Арена");
            sr.resetDialog = false;

            // проверяем создание TSmatchINFO.xlsx/ModelINFO
            string repNm = Decl.TSMATCHINFO_MODELINFO;
            sr.Recover(repNm, SR.RecoverToDo.ResetRep);

            //закрываем модель и открываем ее заново для чистоты проверки
            Assert.IsTrue(Docs.IsDocExists(repNm));
            Docs modINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO);
            modINFO.Close();
            model = new Mod();
            Assert.IsNull(model.name);

            var m = Docs.getDoc(repNm).Body;
            string modName = m.Strng(Decl.MODINFO_NAME_R, 2);
            string dir = m.Strng(Decl.MODINFO_DIR_R, 2);
            string dat = m.Strng(Decl.MODINFO_DATE_R, 2);
            DateTime date = Lib.getDateTime(dat);
            string adr = m.Strng(Decl.MODINFO_ADDRESS_R, 2);
            int cnt = m.Int(Decl.MODINFO_ELMCNT_R, 2);
            string MD5 = m.Strng(Decl.MODINFO_MD5_R, 2);
            string pricingMD5 = m.Strng(Decl.MODINFO_PRCMD5_R, 2);
            Assert.IsTrue(modName.Length > 0);
            Assert.IsTrue(dir.Length > 0);
            Assert.IsTrue(dir.Contains(@"\"));
            Assert.IsTrue(dir.Contains(":"));
            Assert.IsFalse(dir.Contains("."));
            Assert.IsTrue(dat.Length > 0);
            Assert.IsTrue(date > Decl.OLD && date < DateTime.Now);
            Assert.AreEqual("-- моя имитация MD5 --", MD5);
            Assert.AreEqual("-- моя имитация MD5 --", pricingMD5);
            Assert.AreEqual("Санкт-Петербург, Зенит-Арена", adr);

            //-- Raw теперь - отдельный xml файл, его не надо проверять 27.05.2017
            //// проверяем создание TSmatchINFO.xlsx/Raw
            //string raw = Decl.TSMATCHINFO_RAW;
            //// 4/5 долго: 2 мин            sr.Recover(raw, SR.RecoverToDo.ResetRep);
            //Assert.IsTrue(Docs.IsDocExists(raw));

            // проверяем создание TSmatchINFO.xlsx/Report
            string report = Decl.TSMATCHINFO_REPORT;
            //14/7            sr.Recover(report, SR.RecoverToDo.ResetRep);
            Assert.IsTrue(Docs.IsDocExists(report));

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_SR_SetFrSavedModelINFO()
        {
            var sr = init();
            model.dir = boot.ModelDir;

            // SetFrSavedMoodel работает только если нет Tekla
            //..вызывается из Model.SetModel
            if (boot.isTeklaActive) { Assert.IsTrue(true); return; }

            Mod m = sr.SetFrSavedModelINFO(model);
            model.name = m.name;
            model.phase = m.phase;

            Assert.IsTrue(model.name.Length > 0);
            Assert.IsTrue(model.phase.Length > 0);
            Assert.IsTrue(FileOp.isDirExist(model.dir));

            FileOp.AppQuit();

        }

        [TestMethod()]
        public void UT_SR_Raw()
        {
            var sr = init();
            model.dir = boot.ModelDir;

            model.elements = sr.Raw(model);

            Assert.IsTrue(model.elements.Count > 0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_getSavedRules()
        {
            var sr = init();
            model.SetModDir(boot);

            // test no Rules Init
            model = sr.getSavedRules(model);

            Assert.IsTrue(model.Rules.Count > 0);
            foreach(var rule in model.Rules)
            {
                Assert.IsNull(rule.Supplier);
                Assert.IsNull(rule.CompSet);
            }

            // test with Rules Init = true
            model = sr.getSavedRules(model, init: true);

            Assert.IsTrue(model.Rules.Count > 0);
            foreach(var rule in model.Rules)
            {
                Assert.IsNotNull(rule.Supplier);
                Assert.IsNotNull(rule.CompSet);
                Assert.IsTrue(rule.CompSet.Components.Count > 0);
            }

            FileOp.AppQuit();
        }

        // проверяем как дополняются eleGroups из листа TSmatchINFO.xlsx/Report
        // 2017.06.28 переписан sr.getSavedGroups() -- getGrps + читаю из файла Report
        [TestMethod()]
        public void UT_SavedReport_getSavedGroup()
        {
            var sr = init();
            model.dir = boot.ModelDir;
#if OLD //23/7
            sr.elements = sr.Raw(model);

            sr.GetSavedReport(model);

            double sumPrice = sr.elmGroups.Select(x => x.totalPrice).Sum();
            Assert.AreEqual(sumPrice, sr.total_price);
            Docs dRep = Docs.getDoc("Report");
            // в Report <Заголовок> + строки по числу elmGroups.Count + <Summary>
            int cnt = sr.elmGroups.Count + dRep.i0;
            Assert.AreEqual(dRep.il, cnt);
#endif //OLD //23/7
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
        // Model.SetModel() здесь использовать нельзя, т.к. SetModel dspsdftn SetReport
        private SR init()
        {
            boot = new Boot();
            sr = new SR();
            model = new Mod();
            return sr;
        }
    }
}