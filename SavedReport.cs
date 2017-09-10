/*-----------------------------------------------------------------------------------
 * SavedReport -- class for handle saved reports in TSmatchINFO.xlsx
 * 
 *  21.08.2017 П.Л. Храпкин
 *  
 *--- Unit Tests ---
 * UT_GetModelInfo, UT-GetSavedReport, UT_GetSavedRules 18.8.2017 OK 
 * UT_TSmatchINFO_FileExists, UT_TSmatchINFO_NoFiles    18.8.2017 OK
 * UT_SR_Raw    2017.07.23 OK 2 sec
 * UT_CheckModelIntegrity 2017.07.16 OK
 * UT_Save      2017.07.17 OK
 * UT_SetModel  2017.08.17 OK
 *--- History  ---
 * 17.04.2017 выделен из модуля Model
 *  1.05.2017 with Document Reset and ReSave
 *  7.05.2017 написал SetFrSavedModelINFO(), переписал isReportConsystant()
 * 27.05.2017 - XML read and write model.elements as Raw.xml in Raw() 
 *  5.06.2017 - bug fix in SetFrSavedModel - recoursive call after Reset
 * 23.07.2017 - re-engineering with no heritage from Model
 * 27.07.2017 - no isRuleChanged -- model.isChanged handle in Save() method
 * 28.07.2017 - getSavedInit() init flag handle
 * 31.07.2017 - remove local ref mh - it is in Model; fix FATAL "No TSmatchINFO.xlsx"
 *  3.07.2017 - corrections in GetSavedRules -- if(init)
 *  7.08.2017 - GetModelINFO audit; getRaw(model.dat = File.GetLastWriteTime(file);
 * 11.08.2017 - more tests in CeckIntegrityModel, and GetSavedRules updated
 * 14.08.2017 - CheckModelIntegrity: no model.name is tested in without file; check IfDirExist()
 * 16.08.2017 - SetSavedMod method removed; protected instead of public methods; Recovery audit
 * 20.08.2017 - SavedReport audit: model.dir in TSMatchINFO.xlsx ignored, used one from Boot
 *--- Methods: -------------------      
 * SetModel(boot)   - initialize model by reading from TSmatchINFO.xlsx ans Raw.xml or from scratch
 *      private SetModDir(boot) - subset of SetModel(), setup model.dir, name and phase
 *      protected GetTSmatchINFO(Model) - Main SaveReport method. Read TSmatchINFO.xlsx and Raw.xml
 * CheckModelIntegrity(model) - Check if model data are consistant
 *      private GetTSmatchINFO() - read TSmatchINFO.xlsx, check, set it as a current Model

 * IsModelCahanged - проверяет, изменилась ли Модель относительно сохраненного MD5
 ! lngGroup(atr)   - группирует элементы модели по парам <Материал, Профиль> возвращая массивы длинны 
 * Save(Model mod) - Save model mod in file TSmatchINFO.xlsx
 */
using log4net;
using System;
using System.IO;
using System.Collections.Generic;

using Log = match.Lib.Log;
using Lib = match.Lib.MatchLib;
using FileOp = match.FileOp.FileOp;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using Mod = TSmatch.Model.Model;
using WrMod = TSmatch.Model.WrModelInfo.ModelWrFile;
using WrM = TSmatch.Model.WrModelInfo.ModelWrFile.WrMod;
using TS = TSmatch.Tekla.Tekla;
using MH = TSmatch.Handler.Handler;
using Boot = TSmatch.Bootstrap.Bootstrap;

namespace TSmatch.SaveReport
{
    public class SavedReport
    {
        public static readonly ILog log = LogManager.GetLogger("SavedReport");

        #region -- field definitions
        /// <summary> Model class reference</summary>
        Mod model;
        /// <summary> Handler class reference</summary>
        MH mh;
        /// <summary> ModelINFO Document.Name - string</summary>
        const string sINFO = Decl.TSMATCHINFO_MODELINFO;
        /// <summary> Report Document.Name - string</summary>
        const string sRep = Decl.TSMATCHINFO_REPORT;
        /// <summary> Rules Document.Name - string</summary>
        const string sRul = Decl.TSMATCHINFO_RULES;
        /// <summary>
        /// TSMatchINFO.xlsx Documents: ModelINFO, Repoer, Rules
        /// </summary>
        Docs dINFO, dRep, dRul;
        #endregion -- field definitions

        #region --- SetModel ---
        /// <summary>
        /// SetModel(boot) - initialize model by reading from TSmatchINFO.xlsx ans Raw.xml or from scratch
        /// </summary>
        /// <remarks>
        /// With unit_test_mode = true not full model initializing happened.
        /// It is used for testing methods are used on initialization stade.
        /// </remarks>
        /// <param name="boot"></param>
        /// <returns>initialized Model</returns>
        public Mod SetModel(Boot boot, bool initSupl = false)
        {
            Log.set("SR.Model(boot)");
            model = new Mod();
            SetModDir(boot);
            GetTSmatchINFO(model, initSupl);
            Log.exit();
            return model;
        }

        private void SetModDir(Boot boot)
        {
            if (boot.isTeklaActive)
            {   // if Tekla is active - get Path of TSmatch
                model.name = Path.GetFileNameWithoutExtension(TS.ModInfo.ModelName);
                model.dir = TS.GetTeklaDir(TS.ModelDir.model);
                model.phase = TS.ModInfo.CurrentPhase.ToString();
                //6/4/17                        macroDir = TS.GetTeklaDir(TS.ModelDir.macro);
                model.HighLightClear();
            }
            else
            {   // if Tekla not active - get model attributes from TSmatchINFO.xlsx in ModelDir
                model.dir = boot.ModelDir;
                if (!FileOp.isDirExist(model.dir)) Msg.F("No Model Directory", model.dir);
                if (!Docs.IsDocExists(Decl.TSMATCHINFO_MODELINFO)) Msg.F("No TSmatchINFO.xlsx file");
                dINFO = Docs.getDoc(sINFO, fatal: false);
                if (dINFO == null || dINFO.il < 10 || !FileOp.isDirExist(model.dir)) error();
                model.name = dINFO.Body.Strng(Decl.MODINFO_NAME_R, 2);
                model.phase = dINFO.Body.Strng(Decl.MODINFO_PHASE_R, 2);
            }
        }

        /// <summary>
        /// GetTSmatchINFO(Model) - Main SaveReport method. Read TSmatchINFO.xlsx and Raw.xml
        /// </summary>
        /// <remarks>When no TSmatchINFO.xlsx or Raw.xml files exists, create (or mark to create) them, and check model integrity</remarks>
        protected void GetTSmatchINFO(Mod mod, bool initRules = false)
        {
            Log.set("SR.GetTSmatchINFO(\"" + mod.name + "\")");
            model = mod;
            dINFO = Docs.getDoc(sINFO, fatal: false);
            if (dINFO == null) error();

            model.elements = Raw(model);

            if (model.isChanged)
            { //-- no information available from TSmatchINFO.xlsx or it was changed -- doing re-Pricing
                model.mh.Pricing(ref model, initRules);
            }
            else
            { //- get ModelINFO and pricing from TSmatchINFO.xlsx
                model.name = dINFO.Body.Strng(Decl.MODINFO_NAME_R, 2);
                model.setCity(dINFO.Body.Strng(Decl.MODINFO_ADDRESS_R, 2));
                //20/8/2017                model.dir = dINFO.Body.Strng(Decl.MODINFO_DIR_R, 2).Trim();
                model.date = Lib.getDateTime(dINFO.Body.Strng(Decl.MODINFO_DATE_R, 2));
                model.pricingDate = Lib.getDateTime(dINFO.Body.Strng(Decl.MODINFO_PRCDAT_R, 2));
                model.pricingMD5 = dINFO.Body.Strng(Decl.MODINFO_PRCMD5_R, 2);
                GetSavedReport();
                GetSavedRules(model, initRules);
            }
            if (!CheckModelIntegrity(model)) error();
            Log.exit();
        }

        /// <summary>
        /// GetSavedReport() - read Report from TSmatchINFO.xlsx; pick-up Pricing from there, if available
        /// </summary>
        /// <returns></returns>
        protected Mod GetSavedReport()
        {
            Log.set("SR.GetSavedReport");
            bool errRep = true;
            model.elmGroups = model.mh.getGrps(model.elements);
            Docs dRep = Docs.getDoc(sRep, fatal: false, create_if_notexist: false);
            if (dRep == null || dRep.i0 < 2) error(errRep);
            model.total_price = 0;
            for (int iGr = 1, i = dRep.i0; i < dRep.il; i++, iGr++)
            {
                if (iGr > model.elmGroups.Count) break;   // group.Count decreased from saved Report
                var gr = model.elmGroups[iGr - 1];
                if (iGr != dRep.Body.Int(i, Decl.REPORT_N)) error(errRep);
                gr.SupplierName = dRep.Body.Strng(i, Decl.REPORT_SUPPLIER);
                gr.CompSetName = dRep.Body.Strng(i, Decl.REPORT_COMPSET);
                gr.compDescription = dRep.Body.Strng(i, Decl.REPORT_SUPL_DESCR).Trim();
                gr.totalPrice = dRep.Body.Double(i, Decl.REPORT_SUPL_PRICE);
                model.total_price += gr.totalPrice;
            }
            Log.exit();
            return model;
        }

        /// <summary>
        /// GetSavedRules(Mod mod, [init] - read Rules from Rules Sheet in TSmatchINFO.xlsx; Initiate them if init=true
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        protected Mod GetSavedRules(Mod mod, bool init = false)
        {
            Log.set("SR.getSavedRules()");
            model = mod;
            dRul = Docs.getDoc(Decl.TSMATCHINFO_RULES, create_if_notexist: false, fatal: false);
            if (dRul == null)
            { // when TXmatchINFO.xlsx/Rules unavailable - initialise them from TSmatch/InitialRules
                Docs ir = Docs.getDoc("InitialRules");
                for (int i = ir.i0; i < ir.il; i++)
                    model.Rules.Add(new Rule.Rule(ir, i));
            }
            else
            { // Rules available, read them
                model.Rules.Clear();
                for (int i = dRul.i0; i <= dRul.il; i++)
                {
                    try { model.Rules.Add(new Rule.Rule(i)); }
                    catch { continue; }
                }
            }
            if (init) foreach (var rule in model.Rules) rule.Init();
            log.Info("GetSavedRules: Rules.Count = " + model.Rules.Count
                + (init ? "" : " NOT") + "Initialized");
            if (!CheckModelIntegrity(model)) error();
            Log.exit();
            return model;
        }

        /// <summary>
        /// CheckModelIntegrity(model) - Check if model data are consistant
        /// </summary>
        public bool CheckModelIntegrity(Mod mod)
        {
            if (!FileOp.isDirExist(mod.dir)) return false;
            if (mod.date < Decl.OLD || mod.date > DateTime.Now) return false;
            if (mod.pricingDate < Decl.OLD || mod.pricingDate > DateTime.Now) return false;
            if (mod.MD5 == null || mod.MD5.Length != 32) return false;
            if (mod.pricingMD5 == null || mod.pricingMD5.Length != 32) return false;
            if (mod.elements.Count <= 0 || mod.elmGroups.Count <= 0) return false;

            if (FileOp.isFileExist(Path.Combine(mod.dir, Decl.F_TSMATCHINFO)))
            {
                dINFO = Docs.getDoc(sINFO, create_if_notexist: false, fatal: false);
                if (dINFO == null || dINFO.il < 10) return false;
                if (string.IsNullOrWhiteSpace(mod.name)) return false;
                if (isChangedStr(ref mod.name, dINFO, Decl.MODINFO_NAME_R, 2)) return false;
                //20/8/2017                if (isChangedStr(ref mod.dir, dINFO, Decl.MODINFO_DIR_R, 2)) return false;
                if (isChangedStr(ref mod.MD5, dINFO, Decl.MODINFO_MD5_R, 2)) return false;
                if (isChangedStr(ref mod.pricingMD5, dINFO, Decl.MODINFO_PRCMD5_R, 2)) return false;
                if (mod.elements.Count != dINFO.Body.Int(Decl.MODINFO_ELMCNT_R, 2)) return false;
                dRul = Docs.getDoc(sRul, create_if_notexist: false, fatal: false);
                if (dRul == null || dRul.il < dRul.i0 || dRul.il <= 2) return false;
                dRep = Docs.getDoc(sRep, create_if_notexist: false, fatal: false);
                if (dRep == null || dRep.il < dRep.i0 || dRul.il <= 2) return false;
                if (dRep.il - dRep.i0 != mod.elmGroups.Count) return false;
                if (mod.Rules.Count == 0) return false;
            }
            return true;
        }

        private bool isChangedStr(ref string str, Docs doc, int row, int col)
        {
            string strINFO = doc.Body.Strng(row, col);
            if (string.IsNullOrEmpty(str)) str = strINFO;
            return str != strINFO;
        }

        /// <summary>
        /// error() - SavedReport error handler method
        /// </summary>
        private void error(bool errRep = false)
        {
            Log.set("SR.errer()");
            log.Info("error() model.errRecover = " + model.errRecover.ToString());
            if (model.errRecover)
            {
                Msg.AskFOK("Corrupted saved report TSmatchINFO.xlsx");
                model.elements = Raw(model);
                dRep = Docs.getDoc(sRep);
                if (dRep == null || errRep) Msg.F("SavedReport recover impossible");
                GetSavedReport();
                Recover(sINFO, RecoverToDo.ResetRep);
                //21/7           Recover(mod, sRep,  RecoverToDo.ResetRep);
            }
            else model.isChanged = true;  // say, that TSmatchINFO.xlsx should be re-written
            Log.exit();
        }
        #endregion --- SetModel ---

        #region --- Reset & Recovery area ---
        public enum RecoverToDo
        {
            CreateRep, ResetRep, NewMod,
            ChangedDir,
            ChangedPricing
        }

        /// <summary>
        /// when TRUE - Reset dialog allowed
        /// </summary>
        public bool resetDialog = true;
        public void Recover(string repNm, RecoverToDo to_do)
        {
            Log.set(@"SR.Recover(" + repNm + "\")");
            if (!CheckModelIntegrity(model)) Msg.AskFOK("Recovery impossible");
            switch (to_do)
            {
                case RecoverToDo.CreateRep:
                    Msg.AskFOK("В каталоге модели нет TSmatchINFO.xlsx/" + repNm + ". Создать?");
                    resetDialog = false;
                    Docs.getDoc(repNm, reset: true, create_if_notexist: true);
                    if (!Docs.IsDocExists(repNm)) Msg.F("SaveDoc.Recover cannot create ", repNm);
                    Recover(repNm, RecoverToDo.ResetRep);
                    break;
                case RecoverToDo.ResetRep:
                    if (resetDialog) Msg.AskFOK("Вы действительно намерены переписать TSmatchINFO.xlsx/" + repNm + "?");
                    var w = new WrMod();
                    switch (repNm)
                    {
                        case Decl.TSMATCHINFO_MODELINFO:
                            w.wrModel(WrM.ModelINFO, model);
                            break;
                        case Decl.TSMATCHINFO_REPORT:
                            log.Info(">>mod.MD5=" + model.MD5 + " =?= " + model.getMD5(model.elements));
                            mh.Pricing(ref model);
                            log.Info(">>mod.MD5=" + model.MD5 + " =?= " + model.getMD5(model.elements));
                            w.wrModel(WrM.Report, model);
                            break;
                    }
                    break;
            }
            Log.exit();
        }
        #endregion --- Reset & Recovery area ---

        #region --- Raw - read/write Raw.xml area ---
        /// <summary>
        /// Raw() - read elements from Raw.xml or re-write it, if necessary 
        ///<para>
        ///re-write reasons could be: Raw.xml not exists, or error found in ModelINFO
        ///</para>
        /// </summary>
        /// <returns>updated list of elements in file and in memory</returns>
        public List<Elm> Raw(Mod mod, bool write = false)
        {
            Log.set("SR.Raw(" + mod.name + ")");
            model = mod;
            List<Elm> elms = new List<Elm>();
            if (string.IsNullOrEmpty(model.dir)) Msg.F("SR.Raw: No model.dir");
            string file = Path.Combine(model.dir, Decl.RAWXML);
            if (!write && FileOp.isFileExist(file))
            {                               // Read Raw.xml
                elms = rwXML.XML.ReadFromXmlFile<List<Elm>>(file);
                model.date = File.GetLastWriteTime(file);
            }
            else
            {                               // get from CAD and Write or re-Write Raw.xml 
                Msg.AskFOK("SR.Raw: CAD Read");
                model.Read();
                rwXML.XML.WriteToXmlFile(file, model.elements);
                elms = model.elements;
            }
            model.MD5 = model.getMD5(elms);
            log.Info("Raw.xml: { elmCount, MD5} ==" + elms.Count + ", " + model.MD5);
            Log.exit();
            return elms;
        }
        #endregion --- Raw - read/write Raw.xml area ---

        /// <summary>
        /// Save(model) - save means write model in file TSmatchINFO.xlsx
        /// </summary>
        /// <remarks>
        /// Write Documents
        /// - ModelINFO
        /// - Report
        /// - Rules
        /// as the Sheets in Excel file TSmatchINFO.xlsx
        /// </remarks>
        /// <param name="model"></param>
        public void Save(Mod model)
        {
            if (!CheckModelIntegrity(model)) model.mh.Pricing(ref model);
            var w = new WrMod();
            w.wrModel(WrM.ModelINFO, model);
            w.wrModel(WrM.Report, model);
            if (model.Rules.Count == 0) GetSavedRules(model, init: false);
            w.wrModel(WrM.Rules, model);
        }
    } // end class SavedReport
} // end namespace