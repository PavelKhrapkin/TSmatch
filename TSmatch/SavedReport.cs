/*-----------------------------------------------------------------------------------
 * SavedReport -- class for handle saved reports in TSmatchINFO.xlsx
 * 
 *  20.07.2017 П.Л. Храпкин
 *  
 *--- Unit Tests ---
 * UT_GetModelInfo  2-17.7.14 
 * UT_SavedReport_Raw 2017.05.27 11 sec
 *--- History  ---
 * 17.04.2017 выделен из модуля Model
 *  1.05.2017 with Document Reset and ReSave
 *  7.05.2017 написал SetFrSavedModelINFO(), переписал isReportConsystant()
 * 27.05.2017 - XML read and write model.elements as Raw.xml in Raw() 
 *  5.06.2017 - bug fix in SetFrSavedModel - recoursive call after Reset
 * 19.07.2017 - Audit SavedReport
 *--- Methods: -------------------      
 * bool GetTSmatchINFO()    - read TSmatchINFO.xlsx, set it as a current Model
 *                            return true if name, dir, quantity of elements is
 *                            suit to the current model
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
using TSmatch.Document;
using TSmatch.Model;
using static TSmatch.Model.WrModelInfo.ModelWrFile;

namespace TSmatch.SaveReport
{
    public class SavedReport : Mod
    {
        public static readonly ILog log = LogManager.GetLogger("SavedReport");

        string sINFO = Decl.TSMATCHINFO_MODELINFO;
        string sRep = Decl.TSMATCHINFO_REPORT;
        string sRul = Decl.TSMATCHINFO_RULES;
        Docs dINFO, dRep, dRul;
        private Mod ModelInCad;

        public void GetTSmatchINFO(Mod mod)
        {
            Log.set("SR.GetSavedReport(\"" + mod.name + "\")");
            dINFO = GetModelINFO(mod);
            elements = Raw(mod);
            GetSavedReport(mod);
            CheckModelIntegrity(mod);
            SetSavedMod(mod);
            Log.exit();
        }

        #region ------ ModelINFO region ------
        public Docs GetModelINFO(Mod mod)
        {
            dINFO = Docs.getDoc(sINFO, fatal: false);
            if (dINFO == null)
            {
                Msg.F("No saved TSmatchINFO.xlsx");
                Recover(mod, sINFO, RecoverToDo.CreateRep);
                GetModelINFO(mod);
            }
            CheckModINFO(mod, mod.name, Decl.MODINFO_NAME_R);
            string adr = dINFO.Body.Strng(Decl.MODINFO_ADDRESS_R, 2);
            mod.adrStreet = setCity(adr);
            mod.adrStreet = adrStreet;
            mod.dir = dINFO.Body.Strng(Decl.MODINFO_DIR_R, 2).Trim();
            mod.date = Lib.getDateTime(dINFO.Body.Strng(Decl.MODINFO_DATE_R, 2));
            if (mod.date > DateTime.Now || mod.date < Decl.OLD) error(mod);
//17/7            CheckModINFO(mod, mod.dir, Decl.MODINFO_DIR_R);
            CheckModINFO(mod, mod.MD5, Decl.MODINFO_MD5_R);
            CheckModINFO(mod, mod.pricingMD5, Decl.MODINFO_PRCMD5_R);
            mod.pricingDate = Lib.getDateTime(dINFO.Body.Strng(Decl.MODINFO_PRCDAT_R, 2));
            if (mod.pricingDate > DateTime.Now || mod.pricingDate < Decl.OLD) error(mod);
            return dINFO;
        }
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

        private void CheckModINFO(Mod mod, string str, int iRow)
        {
            string strINFO = dINFO.Body.Strng(iRow, 2);
            if (string.IsNullOrEmpty(str)) str = strINFO;
            if (str == strINFO) return;
            Recover(mod, sINFO, RecoverToDo.ResetRep);
            GetModelINFO(mod);
        }

        private bool isChangedStr(ref string str, Docs doc, int row, int col)
        {
            string strINFO = doc.Body.Strng(row, col);
            if (string.IsNullOrEmpty(str)) str = strINFO;
            return str != strINFO;
        }

        /// <summary>
        /// SetFrSavedModelINFO(string dir) - set model attributes from 
        /// TSmatchINFO.xlsx/ModuleINFO. When this documents corrupred -
        /// fatal error. This method call only when Tekla not available
        /// </summary>
        /// <param name="dir">directory, where TSmatchINFO.xlsx stored</param>
        public Mod SetFrSavedModelINFO(string dir)
        {
            dINFO = Docs.getDoc(sINFO, fatal: false);
            if (dINFO == null || dINFO.il < 10) error(this);
            name = strSub(Decl.MODINFO_NAME_R);
            string directory = strSub(Decl.MODINFO_DIR_R);
            if (dir != directory) error(this);
            //14/7            adrCity = setCity(strSub(Decl.MODINFO_ADDRESS_R));
            phase = strSub(Decl.MODINFO_PHASE_R);
            //14/7            date = dateSub(Decl.MODINFO_DATE_R);
            //14/7            MD5 = strSub(Decl.MODINFO_MD5_R);
            elementsCount = Convert.ToInt32(strSub(Decl.MODINFO_ELMCNT_R));
            //14/7            pricingDate = dateSub(Decl.MODINFO_PRCDAT_R);
            //14/7            pricingMD5 = strSub(Decl.MODINFO_PRCMD5_R);
            return this;
        }

        private string strSub(int iRow)
        {
            string str = dINFO.Body.Strng(iRow, 2);
            if (str.Length <= 0) error(this);
            return str;
        }
        private DateTime dateSub(int iRow)
        {
            string str = strSub(iRow);
            DateTime _date = Lib.getDateTime(str);
            if (_date > DateTime.Now || _date < Decl.OLD) error(this);
            return _date;
        }

        private void error(Mod mod, bool errRep = false)
        {
            Msg.AskFOK("Corrupted saved report TSmatchINFO.xlsx");
            elements = Raw(mod);
            dRep = Docs.getDoc(sRep);
            if (dRep == null ||errRep) Msg.F("SavedReport recover impossible");
            GetSavedReport(mod);
            Recover(mod, sINFO, RecoverToDo.ResetRep);
            Recover(mod, sRep,  RecoverToDo.ResetRep);
        }
        #endregion ------ ModelINFO region ------

        private void SetSavedMod(Mod mod)
        {
            Log.set("SetSavedReport");
            ModelInCad = mod;

            dINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO, fatal: false);
            dRep = Docs.getDoc(Decl.TSMATCHINFO_REPORT, fatal: false);

            name = mod.name;
            dir = mod.dir;
            phase = mod.phase;
            date = Lib.getDateTime(dINFO.Body.Strng(Decl.MODINFO_DATE_R, 2));
            made = mod.made; MD5 = mod.MD5;
            elementsCount = mod.elementsCount;
            pricingDate = mod.pricingDate;
            pricingMD5 = mod.pricingMD5;
            mh = mod.mh;

            Log.TraceOn();
            if (TS.isTeklaActive()) Log.Trace("Tekla active");
            else Log.Trace("No Tekla");
            Log.Trace("name =", name);
            Log.Trace("dir  =", dir);
            Log.Trace("phase=", phase);
            Log.Trace("made =", made);
            Log.Trace("date =", date);
            Log.Trace("prcDT=", pricingDate);
            Log.Trace("elCnt=", elementsCount);
            Log.Trace("strRl=", strListRules);
            Log.TraceOff();
            Log.exit();
        }

        #region ------ Reset & Recovery area ------
        public enum RecoverToDo
        {
            CreateRep, ResetRep, NewMod,
            ChangedDir,
            ChangedPricing
        }

        public bool resetDialog = true;
        public void Recover(Mod mod, string repNm, RecoverToDo to_do)
        {
            switch (to_do)
            {
                case RecoverToDo.CreateRep:
                    Msg.AskFOK("В каталоге модели нет TSmatchINFO.xlsx/" + repNm + ". Создать?");
                    resetDialog = false;
                    Docs.getDoc(repNm, reset: true, create_if_notexist: true);
                    if (!Docs.IsDocExists(repNm)) Msg.F("SaveDoc.Recover cannot create ", repNm);
                    Recover(mod, repNm, RecoverToDo.ResetRep);
                    break;
                case RecoverToDo.ResetRep:
                    if (resetDialog) Msg.AskFOK("Вы действительно намерены переписать TSmatchINFO.xlsx/" + repNm + "?");
                    var w = new WrMod();
                    switch (repNm)
                    {
                        case Decl.TSMATCHINFO_MODELINFO:
                            CheckModelIntegrity(mod);
                            w.wrModel(WrM.ModelINFO, mod);
                            break;
                        case Decl.TSMATCHINFO_REPORT:
                            mh.Pricing(ref mod);
                            CheckModelIntegrity(mod);
                            w.wrModel(WrM.Report, mod);
                            break;
                    }
                    break;
            }
        }

        private void CheckModelIntegrity(Mod mod)
        {
            if (mod.elementsCount != mod.elements.Count) error(mod);
            if (mod.MD5 != mod.getMD5(mod.elements)) error(mod);
            if (mod.pricingMD5 != get_pricingMD5(elmGroups)) error(mod);
        }
        #endregion ------ Reset & Recovery area ------

        #region ------ Raw - read/write Raw.xml area ------
        /// <summary>
        /// Raw() - read elements from Raw.xml or re-write it, if necessary 
        ///<para>
        ///re-write reasons: Raw.xml not exists, MD5 or elementsCount != ones in ModelINFO
        ///</para>
        /// </summary>
        /// <returns>updated list of elements in file and in memory</returns>
        public List<Elm> Raw(Mod mod, bool write = false)
        {
            Log.set("SR.Raw(" + mod.name + ")");
            List<Elm> elms = new List<Elm>();
            string file = Path.Combine(mod.dir, Decl.RAWXML);
            if (!write && FileOp.isFileExist(file))
            {                               // Read Raw.xml
                elms = rwXML.XML.ReadFromXmlFile<List<Elm>>(file);
            }
            else
            {                               // get from CAD and Write or re-Write Raw.xml 

                Msg.AskFOK("Файл Raw.xml не доступен."
                    + " Вы действительно хотите получить его из САПР заново?");
                mod.Read();
                rwXML.XML.WriteToXmlFile(file, mod.elements);
                elms = mod.elements;
            }
            if (mod.elementsCount != elms.Count) elms = Raw(mod, write: true);
            mod.MD5 = mod.getMD5(elms);
            Log.Trace("{ elmCount, MD5} ==", elms.Count, mod.MD5);
            Log.exit();
            return elms;
        }
        #endregion ------ Raw - read/write Raw.xml area ------

        public void GetSavedReport(Mod mod)
        {
            bool errRep = true;
            if (mh == null) mh = new Model.Handler.ModHandler();
            elmGroups = mh.getGrps(mod.elements, errDialog: false);
            Docs dRep = Docs.getDoc(sRep, fatal: false, create_if_notexist: true);
            if (dRep == null || dRep.i0 < 2) error(mod, errRep);
            if (dRep.il != (mod.elmGroups.Count + dRep.i0))
            {
                Msg.AskFOK("Saved Report should be recovered, OK?");
                Recover(mod, sRep, RecoverToDo.ResetRep);
            }
            total_price = 0;
            for (int iGr = 1, i = dRep.i0; i < dRep.il; i++, iGr++)
            {
                var gr = elmGroups[iGr - 1];
                if (iGr != dRep.Body.Int(i, Decl.REPORT_N)) error(mod, errRep);
                gr.SupplierName = dRep.Body.Strng(i, Decl.REPORT_SUPPLIER);
                gr.CompSetName = dRep.Body.Strng(i, Decl.REPORT_COMPSET);
                gr.totalPrice = dRep.Body.Double(i, Decl.REPORT_SUPL_PRICE);
                total_price += gr.totalPrice;
            }
            mod.pricingMD5 = get_pricingMD5(mod.elmGroups);
        }

        public void getSavedRules(bool init = false)
        {
            Log.set("SR.getSavedRules()");
            Rules.Clear();
            Docs doc = Docs.getDoc("Rules");
            for (int i = doc.i0; i <= doc.il; i++)
            {
                try { Rules.Add(new Rule.Rule(i)); }
                catch { continue; }
                //////////////////date = Lib.getDateTime(doc.Body.Strng(i, 1));
                //////////////////if (date > DateTime.Now || date < Decl.OLD) continue;
                //////////////////string sSupl = doc.Body.Strng(i, 2);
                //////////////////string sCS = doc.Body.Strng(i, 3);
                // 7/6/17 ////////string sR = doc.Body.Strng(i, 4);
                //////////////////if (string.IsNullOrEmpty(sSupl)
                //////////////////    || string.IsNullOrEmpty(sCS)
                //////////////////    || string.IsNullOrEmpty(sR)) continue;
                //////////////////var rule = new Rule.Rule(date, sSupl, sCS, sR);
                //////////////////Rules.Add(rule);
            }
            log.Info("- getSavedRules() Rules.Count = " + Rules.Count);
            Log.exit();
        }

        internal void Save(Mod model, bool isRuleChanged = false)
        {
            var w = new WrMod();
            w.wrModel(WrM.ModelINFO, model);
            w.wrModel(WrM.Report, model);
            if (isRuleChanged) w.wrModel(WrM.Rules, model);
        }
    } // end class SavedReport
} // end namespace