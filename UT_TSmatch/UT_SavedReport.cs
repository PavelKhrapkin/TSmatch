/*=================================
 * Saved Report Unit Test 18.7.2017
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
using TSmatch.Model;

namespace TSmatch.SaveReport.Tests
{
    [TestClass()]
    public class UT_SavedReportTests
    {
        Boot boot;
        SR sr;
        Mod model;

        [TestMethod()]
        public void UT_GetSavedReport()
        {
            var sr = init();

            sr.GetSavedReport(model);


            Assert.IsTrue(sr.elementsCount > 0);
            Assert.AreEqual(sr.elementsCount, sr.elements.Count);
            Assert.IsTrue(sr.elmGroups.Count > 0);
            var total_price = sr.elmGroups.Sum(x => x.totalPrice);
            Assert.IsTrue(sr.elmGroups.Sum(x => x.totalPrice) > 0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_GetModelINFO()
        {
            // GetModelINFO() - базовый метод, вызываемый в SetModel.
            //..поэтому пользоваться обычным init() для этого UT_ нельзя 
            const string defaultModName = "MyTestName";
            boot = new Bootstrap.Bootstrap();
            var sr = new SR();
            sr.dir = boot.ModelDir;
            if (string.IsNullOrEmpty(sr.dir)) sr.dir = boot.DebugDir;
            if (!FileOp.isFileExist(sr.dir, "TSmatchINFO.xlsx"))
            {
                sr.name = defaultModName;
                sr.adrCity = "Санкт-Петербург";
                sr.adrStreet = "Кудрово";
                sr.date = DateTime.Now;
                sr.MD5 = "sample-MD5";
            }

            var dINFO = sr.GetModelINFO(sr);

            Assert.IsNotNull(dINFO);
            Assert.AreEqual(2, dINFO.i0);
            Assert.IsTrue(dINFO.il > 9);
            var b = dINFO.Body;
            string b_name = b.Strng(Decl.MODINFO_NAME_R, 2);
            Assert.AreEqual("Название модели =", b.Strng(Decl.MODINFO_NAME_R, 1));
            Assert.IsTrue(b_name.Length >= 1);
            Assert.AreEqual("Адрес проекта:", b.Strng(Decl.MODINFO_ADDRESS_R, 1));
            Assert.IsTrue(b.Strng(Decl.MODINFO_ADDRESS_R, 2).Length >= 1);

            if (b_name == defaultModName) FileOp.Delete(sr.dir, b_name);
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
            var sr = init(set_model: false);
            model.SetModDir(boot);
            model.date = new DateTime(2015, 6, 12, 14, 15, 16);
            model.MD5 = "-- моя имитация MD5 --";
            model.pricingMD5 = "-- моя имитация MD5 --";
            model.pricingDate = new DateTime(2017, 4, 4, 20, 19, 18);
            model.setCity("Санкт-Петербург, Зенит-Арена");
            sr.resetDialog = false;

            // проверяем создание TSmatchINFO.xlsx/ModelINFO
            string repNm = Decl.TSMATCHINFO_MODELINFO;
            sr.Recover(model, repNm, SR.RecoverToDo.ResetRep);

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

            sr.GetSavedReport(model);

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
        // Model.SetModel() здесь использовать нельзя, т.к. SetModel dspsdftn SetReport
        private SR init(bool set_model = true)
        {
            boot = new Boot();
            sr = new SR();
            model = sr;
            if (set_model) model.SetModel(boot);
            return sr;
        }
    }
}