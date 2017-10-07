/*=================================
 * Saved Report Unit Test 6.10.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

using Decl = TSmatch.Declaration.Declaration;
using FileOp = match.FileOp.FileOp;
using Mod = TSmatch.Model.Model;
using SR = TSmatch.SaveReport.SavedReport;
using MH = TSmatch.Handler.Handler;
using Boot = TSmatch.Bootstrap.Bootstrap;

namespace TSmatch.SaveReport.Tests
{
    [TestClass()]
    public class UT_SavedReportTests
    {
        public Boot boot;
        SR sr;
        MH mh;
        Mod model;
        UT_SR U = new UT_SR();

        [TestMethod()]
        public void UT_SR_Msg()
        {
            boot = new Boot(init: false);
            var Umsg = new UT_TSmatch._UT_Msg();

            model = new Mod();
            var sr = model.sr;
            Umsg.SetCulture("en");

            // test 0: "en" NoFile, NoDoc, Obsolet, ErrResource

            try { sr.SetModel(boot); } catch { }
            Assert.AreEqual("[SavedReport.SetModelDir]: No \"TSmatchINFO.xlsx\" in model directory \r\nEnsure, that this file is written by TSmatch application, when Tekla is available", Umsg.GetMsg());
        }

        [TestMethod()]
        public void UT_SetModel()
        {
            boot = new Boot(init:true);
            sr = new SR();

            model = sr.SetModel(boot);

            Assert.IsTrue(sr.CheckModelIntegrity(model));

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_SetModDir()
        {
            boot = new Boot(init: true);
            sr = new SR();

            model = sr.SetModel(boot);

            Assert.IsTrue(sr.CheckModelIntegrity(model));

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_CheckModelIntegrity()
        {
            boot = new Boot(init:true);
            model = new Mod();
            model = model.sr.SetModel(boot);

            bool ok = model.sr.CheckModelIntegrity(model);

            Assert.IsTrue(ok);
            Assert.IsTrue(model.dir.Length > 0);
            Assert.IsTrue(FileOp.isDirExist(model.dir));
            Assert.IsTrue(model.date > Decl.OLD && model.date <= DateTime.Now);
            Assert.IsTrue(model.pricingDate > Decl.OLD && model.pricingDate <= DateTime.Now);
            Assert.AreEqual(32, model.MD5.Length);
            Assert.AreEqual(32, model.pricingMD5.Length);
            Assert.IsTrue(model.elements.Count > 0);
            Assert.IsTrue(model.elmGroups.Count > 0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_GetSavedReport()
        {
            init();
            U._GetTSmatchINFO(model);

            model = U._GetSavedReport();

            bool ok = model.sr.CheckModelIntegrity(model);
            Assert.IsTrue(ok);

            var total_price = model.elmGroups.Sum(x => x.totalPrice);
            Assert.IsTrue(model.elmGroups.Sum(x => x.totalPrice) > 0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_GetTSmatchINFO_FiliExists()
        {
            // GetModelINFO() - базовый метод, вызываемый в SetModel.
            //..поэтому пользоваться обычным init() для этого UT_ нельзя 
            boot = new Boot(init:true);
            model = new Mod();
            model.dir = boot.ModelDir;
            if (string.IsNullOrEmpty(model.dir)) model.dir = boot.DebugDir;
            Assert.IsTrue(model.dir.Length > 0);
            bool isModelINFOexists = FileOp.isFileExist(model.dir, "TSmatchINFO.xlsx");
            if (!isModelINFOexists) goto exit;

            U._GetTSmatchINFO(model);

            bool ok = model.sr.CheckModelIntegrity(model);
            Assert.IsTrue(ok);

            exit:
            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_GetTSmatchINFO_NoFile()
        {
            // GetModelINFO() - базовый метод, вызываемый в SetModel.
            //..поэтому пользоваться обычным init() для этого UT_ нельзя 
            const string defaultModName = "MyTestName";
            boot = new Boot(init:true);
            model = new Mod();
            model.dir = boot.ModelDir;
            if (string.IsNullOrEmpty(model.dir)) model.dir = boot.DebugDir;
            Assert.IsTrue(model.dir.Length > 0);
            bool isModelINFOexists = FileOp.isFileExist(model.dir, "TSmatchINFO.xlsx");
            if (isModelINFOexists) goto exit;

            U._GetTSmatchINFO(model);

            bool ok = model.sr.CheckModelIntegrity(model);
            if (isModelINFOexists)

                Assert.IsTrue(model.isChanged);
            Assert.IsTrue(ok);
            exit: FileOp.AppQuit();
        }
#if OLD // 21.8.17
        [TestMethod()]
        public void UT_Recover()
        {
            init();

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
#endif
        [TestMethod()]
        public void UT_SR_RawMsg()
        {
            // -- чтобы посмотреть, как выглядит MessageBox, но с Assert.Fault по Msg.FOK(), используй
            //var U = new UT_TSmatch._UT_Msg(true);
            var U = new UT_TSmatch._UT_Msg();
            // test 1: Msg("No model dir") -- RU
            Mod mod = new Mod();
            mod.dir = @"C:\ABCDEF";
            try { mod.sr.Raw(mod); } catch (Exception ex) { Assert.AreEqual("Msg.F", ex.Message); }
            Assert.AreEqual("[SavedReport.Raw]: не найден каталог модели, на который указывает\r\n            запись в TSmatchINFO.xlsx или записано в регистре Windows\r\n\r\n\"C:\\ABCDEF\"\r\n\r\nЭто сообщение возникает, когда нет файла TSmatchINFO.xlsx,\r\nи нет Tekla, чтобы его можно было создать заново.\r\nПопробуйте запустить TSmatch на машине, где есть Tekla.", U.GetMsg());

            // test 2:  Msg("No model dir") -- EN
            // -- чтобы посмотреть, как выглядит MessageBox, но с Assert.Fault по Msg.FOK(), используй
            //U.SetCulture("en", true);
            U.SetCulture("en");
            try { mod.sr.Raw(mod); }  catch (Exception ex) { Assert.AreEqual("Msg.F", ex.Message); }
            Assert.AreEqual("[SavedReport.Raw]:  not found model directory, pointed by\r\n     TSmatchINFO.xlsx, or written in Windows Environment\r\n\r\n\"C:\\ABCDEF\" \r\n\r\nand there is no Tekla active to read and re-create it again. \r\nPlease, try to run TSmatch on PC with Tekla.", U.GetMsg());

            // test 3:  Msg("Raw_CAD_Read") -- EN
            // -- чтобы посмотреть, как выглядит MessageBox, но с Assert.Fault по Msg.FOK(), используй
            //U.SetCulture("en", true);
            U.SetCulture("en");
            mod.dir = @"C:\Windows";
            try { mod.sr.Raw(mod); } catch (Exception ex) { Assert.AreEqual("Msg.F", ex.Message); }
            Assert.AreEqual("[SavedReport.Raw]: File \"Raw.xml\" is corrupted or unavailable.\r\nWould you like to read it from CAD once again?", U.GetMsg());

            // test 4:  Msg("Raw_CAD_Read") -- RU
            // -- чтобы посмотреть, как выглядит MessageBox, но с Assert.Fault по Msg.FOK(), используй
            //U.SetCulture("ru", true);
            U.SetCulture("ru");
            mod.dir = @"C:\Windows";
            try { mod.sr.Raw(mod); } catch (Exception ex) { Assert.AreEqual("Msg.F", ex.Message); }
            Assert.AreEqual("[SavedReport.Raw]: Файл \"Raw.xml\" не доступен или испорчен.\r\nВы действительно хотите получить его из САПР заново?", U.GetMsg());
        }

        [TestMethod()]
        public void UT_SR_Raw()
        { 

            init();

            model.elements = model.sr.Raw(model);

            Assert.IsTrue(model.elements.Count > 0);
            Assert.IsTrue(model.date > Decl.OLD & model.date < DateTime.Now);
            Assert.AreEqual(32, model.MD5.Length);

            foreach(var elm in model.elements)
            {
                Assert.AreEqual(38, elm.guid.Length);
            }

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_getSavedRules()
        {
            init();

            // test no Rules Init
            model = U._GetSavedRules(model, init_mode: false);

            Assert.IsTrue(model.Rules.Count > 0);
            foreach (var rule in model.Rules)
            {
                Assert.IsNull(rule.Supplier);
                Assert.IsNull(rule.CompSet);
            }

            // test with Rules Init = true
            model = U._GetSavedRules(model, init_mode: true);

            Assert.IsTrue(model.Rules.Count > 0);
            foreach (var rule in model.Rules)
            {
                Assert.IsNotNull(rule.Supplier);
                Assert.IsNotNull(rule.CompSet);
                Assert.IsTrue(rule.CompSet.Components.Count > 0);
            }

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Save()
        {
            init();
            model = model.sr.SetModel(boot, initSupl: false);   // with Rule Initialization

            model.sr.Save(model);

            bool ok = model.sr.CheckModelIntegrity(model);
            Assert.IsTrue(ok);

            FileOp.AppQuit();
        }

        // эта инициализация класса SavedReport общая для всех тестов этого класса
        // здесь используется sr.SetModel в сокращенном режиме, т.е. без обращения к тестируемым методам
        private void init()
        {
            boot = new Boot();
            model = new Mod();
            model = model.sr.SetModel(boot, initSupl: true);
        }
    }

    class UT_SR : SR
    {
        public void _GetTSmatchINFO(Mod mod, bool ut_mode = false)
        {
            GetTSmatchINFO(mod, ut_mode);
        }

        internal Mod _GetSavedReport()
        {
            return GetSavedReport();
        }

        internal Mod _GetSavedRules(Mod model, bool init_mode)
        {
            return GetSavedRules(model, init_mode);
        }

        internal Mod _SetModDir(Mod mod)
        {
  //          var gg = UT_SavedReportTests.
   //         var result = mod.SetModDir();
            return null;
        }
    }
}