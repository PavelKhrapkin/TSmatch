﻿/*-----------------------------------------------------------------------------------
 * SavedReport -- class for handle saved reports in TSmatchINFO.xlsx
 * 
 *  7.08.2017 П.Л. Храпкин
 *  
 *--- Unit Tests ---
 * UT_GetModelInfo  2-17.7.14 
 * UT_SavedReport_Raw 2017.07.23 OK 2 sec
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
 *--- Methods: -------------------      
 * GetTSmatchINFO()     - read TSmatchINFO.xlsx, check, set it as a current Model

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
using TSmatch.Document;
using TSmatch.Model;
using static TSmatch.Model.WrModelInfo.ModelWrFile;
using TSmatch.Bootstrap;

namespace TSmatch.SaveReport
{
    public class SavedReport
    {
        public static readonly ILog log = LogManager.GetLogger("SavedReport");

        #region -- field definitions
        /// <summary>
        /// Model class reference
        /// </summary>
        Mod model;
        /// <summary>
        /// Handler class reference
        /// </summary>
        MH mh;      //ref to class Model Handler
                    /// <summary>
                    /// ModelINFO Document.Name - string
                    /// </summary>
        string sINFO = Decl.TSMATCHINFO_MODELINFO;
        /// <summary>
        /// Report Document.Name - string
        /// </summary>
        string sRep = Decl.TSMATCHINFO_REPORT;
        /// <summary>
        /// Rules Document.Name - string
        /// </summary>
        string sRul = Decl.TSMATCHINFO_RULES;
        /// <summary>
        /// Rules Document
        /// </summary>
        Docs dINFO, dRep, dRul;
        #endregion -- field definitions

        #region --- SetModel ---
        /// <summary>
        /// SetModel(boot) - initialize model by reading from TSmatchINFO.xlsx ans Raw.xml or from scratch
        /// </summary>
        /// <param name="boot"></param>
        /// <returns>initialized Model</returns>
        public Mod SetModel(Boot boot)
        {
            Log.set("SR.Model(boot)");
            model = new Mod();
            SetModDir(boot);
            GetTSmatchINFO(model);
            Log.exit();
            return model;
        }

        public void SetModDir(Boot boot)
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
                model = SetFrSavedModelINFO(model);

                //////////                adrCity = m.adrCity; adrStreet = m.adrStreet;
                ////////////23/7                elementsCount = m.elementsCount;
                ////////////23/7                if (elementsCount == 0)
                ////////////23/7                    Msg.F("SavedReport doc not exists and no CAD");
                //////////                date = m.date;
                //////////                MD5 = m.MD5;
                //////////                pricingMD5 = m.pricingMD5;
                //////////                pricingDate = m.pricingDate;
                //////////                //24/4                classCAD = ifc;
            }
        }

        #endregion --- SetModel ---

        /// <summary>
        /// GetTSmatchINFO(Model) - Main SaveReport method. Read TSmatchINFO.xlsx and Raw.xml
        /// </summary>
        /// <remarks>When no TSmatchINFO.xlsx or Raw.xml files exists, create (or mark to create) them, and chech model integrity</remarks>
        public void GetTSmatchINFO(Mod mod)
        {
            Log.set("SR.GetTSmatchINFO(\"" + mod.name + "\")");
            model = mod;
            if (mh == null) mh = new MH();
            dINFO = Docs.getDoc(sINFO, fatal: false);
            if (dINFO == null) error();

            model.elements = Raw(model);

            if (model.isChanged)
            { //-- no information available from TSmatchINFO.xlsx -- doing re-Pricing
                mh.Pricing(ref model);
            }
            else
            { //- get ModelINFO and pricing from TSmatchINFO.xlsx
                model.name = getModINFOstr(Decl.MODINFO_NAME_R);
                model.setCity(dINFO.Body.Strng(Decl.MODINFO_ADDRESS_R, 2));
                model.dir = dINFO.Body.Strng(Decl.MODINFO_DIR_R, 2).Trim();
                model.date = getModINFOdate(Decl.MODINFO_DATE_R);
                model.pricingMD5 = model.get_pricingMD5(model.elmGroups);
                model.pricingDate = getModINFOdate(Decl.MODINFO_PRCDAT_R);
                model.pricingMD5 = getModINFOstr(Decl.MODINFO_PRCMD5_R, model.pricingMD5);
                GetSavedReport();
            }
            if (!CheckModelIntegrity(model)) error();
            SetSavedMod(mod);
            Log.exit();
        }

        #region ------ ModelINFO region ------
#if OLD // 14/7/17
            bool check = true;
            while (check)
            {

                //                if(!IsModINFO_OK()) { Reset(Decl.TSMATCHINFO_MODELINFO); continue; }
                if (dINFO == null || dINFO.il < 11) { Reset(Decl.TSMATCHINFO_MODELINFO); continue; }
                SetSavedMod(mod);
                if (isChangedStr(ref mod.name, dINFO, 2, 2)) { ChangedModel(); continue; }
                if (isChangedStr(ref mod.dir, dINFO, 3, 2)) { Reset(sINFO); continue; }
                if (isChangedStr(ref mod.MD5, dINFO, 6, 2)) { ChangedModel(); continue; }
                if (isChangedStr(ref mod.pricingMD5, dINFO, 9, 2)) { ChangedPricing(); continue; }
                pricingDate = Lib.getDateTime(dINFO.Body.Strng(8, 2));
                elements = Raw(mod);
                if (elements == null && !TS.isTeklaActive()) Msg.F("No Saved elements in TSmatchINFO.xlsx");
                ////////////////elmGroups = mh.getGrps(elements);
                // 27/6 ////////total_price = 0;
                ////////////////foreach (var gr in elmGroups) total_price += gr.totalPrice;  

            Log.Trace("*SR.elements=", elements.Count, " gr=", elmGroups.Count, " total price=", total_price);
                if (!Docs.IsDocExists(sRep)) { Reset(sRep); continue; }
                getSavedGroups();
                if (!Docs.IsDocExists(sRul)) { Reset(sRul); continue; }
//27/6                if (total_price <= 0) { Recover(sRep, RecoverToDo.ResetRep); continue; }
                check = false;
            }
#endif //OLD //11/7/17

        private string getModINFOstr(int iRow, string str = "")
        {
            string strINFO = dINFO.Body.Strng(iRow, 2);
            if (string.IsNullOrEmpty(strINFO)) strINFO = str;   //when value is calculated;
            if (string.IsNullOrEmpty(str)) str = strINFO;   //when value get from ModelINFO
            if (str != strINFO) error();
            return str;
        }
        private DateTime getModINFOdate(int iRow)
        {
            DateTime d = Lib.getDateTime(dINFO.Body.Strng(iRow, 2));
            if (d < Decl.OLD || d > DateTime.Now) Recover(sINFO, RecoverToDo.ResetRep);
            return d;
        }

        private bool isChangedStr(ref string str, Docs doc, int row, int col)
        {
            string strINFO = doc.Body.Strng(row, col);
            if (string.IsNullOrEmpty(str)) str = strINFO;
            return str != strINFO;
        }

        /// <summary>
        /// SetFrSavedModelINFO(ref model) - set model attributes from 
        /// TSmatchINFO.xlsx/ModuleINFO. When this documents corrupred -
        /// fatal error. This method call only when Tekla not available
        /// </summary>
        /// <param name="dir">directory, where TSmatchINFO.xlsx stored</param>
        public Mod SetFrSavedModelINFO(Mod model)
        {
            dINFO = Docs.getDoc(sINFO, fatal: false);
            if (dINFO == null || dINFO.il < 10 || !FileOp.isDirExist(model.dir)) error();
            model.name = strSub(Decl.MODINFO_NAME_R);
            model.phase = strSub(Decl.MODINFO_PHASE_R, "1");
            return model;
        }

        private string strSub(int iRow, string def = "")
        {
            string str = dINFO.Body.Strng(iRow, 2);
            if (str.Length <= 0) error();
            return str;
        }
        private DateTime dateSub(int iRow)
        {
            string str = strSub(iRow);
            DateTime _date = Lib.getDateTime(str);
            if (_date > DateTime.Now || _date < Decl.OLD) error();
            return _date;
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
        #endregion ------ ModelINFO region ------

        private void SetSavedMod(Mod mod)
        {
            Log.set("SetSavedReport");
            model = mod;

            dINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO, fatal: false);
            dRep = Docs.getDoc(Decl.TSMATCHINFO_REPORT, fatal: false);
#if OLD //23/7
            name = mod.name;
            dir = mod.dir;
            phase = mod.phase;
            date = Lib.getDateTime(dINFO.Body.Strng(Decl.MODINFO_DATE_R, 2));
            made = mod.made; MD5 = mod.MD5;
//23/7            elementsCount = mod.elementsCount;
            pricingDate = mod.pricingDate;
            pricingMD5 = mod.pricingMD5;
            mh = mod.mh;

            if (TS.isTeklaActive()) Log.Trace("Tekla active");
            else Log.Trace("No Tekla");
            Log.Trace("name =", name);
            Log.Trace("dir  =", dir);
            Log.Trace("phase=", phase);
            Log.Trace("made =", made);
            Log.Trace("date =", date);
            Log.Trace("prcDT=", pricingDate);
            Log.Trace("elCnt=", elements.Count);
            Log.Trace("strRl=", strListRules);
            Log.TraceOff();
#endif //OLD //23/7
            Log.exit();
        }

        #region ------ Reset & Recovery area ------
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
            if(!CheckModelIntegrity(model)) Msg.AskFOK("Recovery impossible");
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

        /// <summary>
        /// CheckModelIntegrity(model) - Check if model data are consistant
        /// </summary>
        public bool CheckModelIntegrity(Mod mod)
        {
            Log.set("SR.CheckModelIntegrity()");
            bool ok = true;
            if (mod.date < Decl.OLD || mod.date > DateTime.Now) ok = false;
            if (mod.pricingDate < Decl.OLD || model.pricingDate > DateTime.Now) ok = false;
            if (mod.MD5 == null || model.MD5.Length != 32) ok = false;
            if (mod.pricingMD5 == null || model.pricingMD5.Length != 32) ok = false;
            if (mod.elements.Count <= 0 || model.elmGroups.Count <= 0) ok = false;
            if (string.IsNullOrWhiteSpace(mod.name)) ok = false;
            if (string.IsNullOrWhiteSpace(mod.dir)) ok = false;

            if (FileOp.isFileExist(Path.Combine(mod.dir, Decl.F_TSMATCHINFO)))
            {
                throw new NotImplementedException();
            }
            Log.exit();
            return ok;
        }
        #endregion ------ Reset & Recovery area ------

        #region ------ Raw - read/write Raw.xml area ------
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
        #endregion ------ Raw - read/write Raw.xml area ------

        /// <summary>
        /// GetSavedReport() - read Report from TSmatchINFO.xlsx; pick-up Pricing from there, if available
        /// </summary>
        /// <returns></returns>
        public Mod GetSavedReport()
        {
            Log.set("SR.GetSavedReport");

            if (model.docReport != null)
            {
                bool errRep = true;
                if (mh == null) mh = new MH();
                Docs dRep = Docs.getDoc(sRep, fatal: false, create_if_notexist: true);
                if (dRep == null || dRep.i0 < 2) error(errRep);
                //21/7            if (dRep.il != (mod.elmGroups.Count + dRep.i0))
                //21/7            {
                //21/7                Msg.AskFOK("Saved Report should be recovered, OK?");
                //21/7                Recover(mod, sRep, RecoverToDo.ResetRep);
                //21/7            }
                model.total_price = 0;
                for (int iGr = 1, i = dRep.i0; i < dRep.il; i++, iGr++)
                {
                    if (iGr > model.elmGroups.Count) break;   // group.Count decreased from saved Report
                    var gr = model.elmGroups[iGr - 1];
                    if (iGr != dRep.Body.Int(i, Decl.REPORT_N)) error(errRep);
                    gr.SupplierName = dRep.Body.Strng(i, Decl.REPORT_SUPPLIER);
                    gr.CompSetName = dRep.Body.Strng(i, Decl.REPORT_COMPSET);
                    gr.totalPrice = dRep.Body.Double(i, Decl.REPORT_SUPL_PRICE);
                    model.total_price += gr.totalPrice;
                }
                model.pricingMD5 = model.get_pricingMD5(model.elmGroups);
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
        public Mod GetSavedRules(Mod mod, bool init = false)
        {
            Log.set("SR.getSavedRules()");
            model = mod;
            if (dINFO == null)
            {
                Docs ir = Docs.getDoc("InitialRules");
                for (int i = ir.i0; i < ir.il; i++)
                    model.Rules.Add(new Rule.Rule(ir, i));
            }
            else
            {
                model.Rules.Clear();
                Docs doc = Docs.getDoc("Rules");
                for (int i = doc.i0; i <= doc.il; i++)
                {
                    try { model.Rules.Add(new Rule.Rule(i)); }
                    catch { continue; }
                }
            }
            if (init) foreach (var rule in model.Rules) rule.Init();
            log.Info("- getSavedRules() Rules.Count = " + model.Rules.Count);
            Log.exit();
            return model;
        }

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
        internal void Save(Mod model)
        {
            var w = new WrMod();
            w.wrModel(WrM.ModelINFO, model);
            w.wrModel(WrM.Report, model);
            w.wrModel(WrM.Rules, model);
        }
    } // end class SavedReport
} // end namespace