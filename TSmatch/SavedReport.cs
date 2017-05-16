/*-----------------------------------------------------------------------------------
 * SavedReport -- class for handle saved reports in TSmatchINFO.xlsx
 * 
 *  12.05.2017 П.Л. Храпкин
 *  
 *--- Unit Tests ---
 *--- History  ---
 * 17.04.2017 выделен из модуля Model
 *  1.05.2017 with Document Reset and ReSave
 *  7.05.2017 написал SetFrSavedModelINFO(), переписал isReportConsystant()
 * 12.05.2017 audit
 *--- Methods: -------------------      
 * SaveReport() - NotImplemented yet - Saved current Model in TSmatchINFO.xlsx
 * bool GetSavedReport()    - read TSmatchINFO.xlsx, set it as a current Model
 *                            return true if name, dir, quantity of elements is
 *                            suit to the current model
 * IsModelCahanged - проверяет, изменилась ли Модель относительно сохраненного MD5
 ! lngGroup(atr)   - группирует элементы модели по парам <Материал, Профиль> возвращая массивы длинны 
 */
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

using Log = match.Lib.Log;
using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using Mod = TSmatch.Model.Model;
using Boot = TSmatch.Bootstrap.Bootstrap;
using TS = TSmatch.Tekla.Tekla;
using TSmatch.Model;

namespace TSmatch.SaveReport
{
    /// <summary>
    /// SavedReport Model content in what is stored in TSmatchINFO.xlsx.
    /// It is not the same, stored in Model.Journal, but in process of synch.
    /// </summary>
    public class SavedReport : Mod
    {
        public static readonly ILog log = LogManager.GetLogger("SavedReport");

        string sINFO = Decl.TSMATCHINFO_MODELINFO;
        string sRaw = Decl.TSMATCHINFO_RAW;
        string sRep = Decl.TSMATCHINFO_REPORT;
        Docs dINFO, dRaw, dRep;
        private Mod ModelInCad;

        public void GetSavedReport(Mod mod)
        {
            Log.TraceOn();
            Log.set("SR.GetSavedReport(\"" + mod.name + "\")");
            SetSavedMod(mod);
            bool check = true;
            while (check)
            {
                if (dINFO == null && !TS.isTeklaActive()) Msg.F("SavedReport doc not exists and no CAD");
                if (dINFO == null || dINFO.il < 9) { Reset(Decl.TSMATCHINFO_MODELINFO); continue; }
                if (isChangedStr(ref mod.name, dINFO, 2, 2)) { ChangedModel(); continue; }
                if (isChangedStr(ref mod.dir, dINFO, 3, 2)) { Reset(sINFO); continue; }
                if (isChangedStr(ref mod.MD5, dINFO, 6, 2)) { ChangedModel(); continue; }
                if (isChangedStr(ref mod.pricingMD5, dINFO, 9, 2)) { ChangedPricing(); continue; }
                pricingDate = Lib.getDateTime(dINFO.Body.Strng(8, 2));

                elements = getSavedRaw();
                if (elements == null && !TS.isTeklaActive()) Msg.F("No Saved elements in TSmatchINFO.xlsx");

                mh.getGroups(elements);
                elmGroups = mh.elmGroups;
                elmMgroups = mh.elmMgroups;

                Log.Trace("*SR.elements=", elements.Count, " gr=", elmGroups.Count);
                //12/5                mj = new ModelJournal.ModJournal(boot.models);
                //14/5                Mod m = mj.getModJournal(name);
                //16/5---------------- перенести это в Pricing() ---------------
                // пока почему-то нужно вызывать Handling -- делается Reset(Report)
                getSavedRules();
                mh.Handler(this);
                // если здесь isChanged=true -- mj.SaveModJournal
                //13/5              strListRules = m.strListRules;
                elmGroups = mh.elmGroups;
                Log.Trace("*SR.elements=", elements.Count, " gr=", elmGroups.Count);
                //16/5-----------------------------------------------------------
                getSavedGroups();

                check = false;
            }
            //14/5            mj.SynchModJournal(ModelInCad);
            Log.exit();
            Log.TraceOff();
        }

        private void SetSavedMod(Mod mod)
        {
            Log.set("SetSavedReport");
            ModelInCad = mod;

            dINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO, fatal: false);
            dRaw = Docs.getDoc(Decl.TSMATCHINFO_RAW, fatal: false);
            dRep = Docs.getDoc(Decl.TSMATCHINFO_REPORT, fatal: false);

            name  = mod.name;
            dir   = mod.dir;
            phase = mod.phase;
            date  = mod.date;
            made  = mod.made; MD5 = mod.MD5;
            elementsCount = mod.elementsCount;
            pricingDate = mod.pricingDate;
            pricingMD5  = mod.pricingMD5;

            mj = mod.mj;
            mh = mod.mh;

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
            Log.exit();
        }
#if OLD
            if (isReportConsistent()) return;

            // сюда мы вообще-то не должны приходить - Recovery или Fatal происходят при проверке isReportConsistent()

            TS ts = new TS();
            //ToDo 21/4/17: когда буду делать САПР помимо Tekla, здесь переписать!
            if (!TS.isTeklaActive()) Msg.F("SavedReport inconsistant and no Tekla");
            name = TS.getModInfo();
            dir = TS.ModInfo.ModelPath;
            Mod m = mj.getModJournal(name, dir);
            date = m.date;
            dINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO
                , create_if_notexist: true, reset: true);
            wrModel(WrMod.ModelINFO);
            Read();
            getSavedRules();
            Handler();
            wrModel(WrMod.Report);
            if (!isReportConsistent()) Msg.F("internal error");
        }

        private bool isReportConsistent()
        {
#endif  //OLD
        private void Pricing()
        {
            if (elements.Count == 0) Msg.F("elements.Count == 0");
            Docs dRep = Docs.getDoc(Decl.TSMATCHINFO_REPORT);
            dRep.Reset();
//12/5            mj = new ModelJournal.ModJournal(boot.models);
            Mod m = mj.getModJournal(name);
            strListRules = m.strListRules;
            getSavedRules();
//12/5            mh = new ModelHandler.ModHandler();
            mh.Handler(this);
            getSavedRules();

            return;
            throw new NotImplementedException();
            //8/5            getGroups();
            //12/5 getSavedGroups();
            //6/5           getSavedRules();
            //6/5           if (R
           //12/5 ules.Count <= 0) return false;
            //7/5            if (docReport.Body.Double(docReport.il, Decl.REPORT_SUPL_PRICE) <= 0.0) return false;
        }

        private void ChangedPricing()
        {
            throw new NotImplementedException();
        }

        public void ChangedModel()
        {
            Msg.AskFOK("Нет сохраненной корректной модели. Читаем модель заново?");
            Reset(sINFO);
            Reset(sRaw);
            Reset(sRep);
        }

        private void Reset(string doc_name)
        {
            if (string.IsNullOrEmpty(name)) Msg.F("SavedReport doc not exists and no CAD");
            if (!Docs.IsDocExists(doc_name)) Recover(doc_name, RecoverToDo.CreateRep);
            Recover(doc_name, RecoverToDo.ResetRep);
        }

        public enum RecoverToDo
        {
            CreateRep, ResetRep, NewMod,
            ChangedDir,
            ChangedPricing
        }
        public void Recover(string repNm, RecoverToDo to_do)
        {
            switch (to_do)
            {
                case RecoverToDo.CreateRep:
                    Docs.getDoc(repNm, reset: true, create_if_notexist: true);
                    if (!Docs.IsDocExists(repNm)) Msg.F("SaveDoc.Recover cannot create ", repNm);
                    Recover(repNm, RecoverToDo.ResetRep);
                    break;
                case RecoverToDo.ResetRep:
                    switch (repNm)
                    {
                        case Decl.TSMATCHINFO_MODELINFO:
                            wrModel(WrMod.ModelINFO);
                            mj.ChangeModJournal(ModelInCad);
                            break;
                        case Decl.TSMATCHINFO_RAW:
                            Read(); //wrModel(WrMod.Raw) is inside Read()
                            break;
                        case Decl.TSMATCHINFO_REPORT:
                            wrModel(WrMod.Report);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// SetFrSavedModelINFO(string dir) - set model attributes from 
        /// TSmatchINFO.xlsx/ModuleINFO. When this documents corrupred - Recover it
        /// </summary>
        /// <param name="dir">directory, where TSmatchINFO.xlsx stored</param>
        public Mod SetFrSavedModelINFO(string dir)
        {
            dINFO = Docs.getDoc(sINFO, fatal: false);
            if (dINFO == null || dINFO.il < 9
                || isChangedStr(ref name, dINFO, 2, 2)) Reset(sINFO);
            name = dINFO.Body.Strng(2, 2);
            phase = dINFO.Body.Strng(4, 2);
            date = DateTime.Parse(dINFO.Body.Strng(5, 2));
            MD5 = dINFO.Body.Strng(6, 2);
            if (elementsCount == 0 && !TS.isTeklaActive())
                elementsCount = dINFO.Body.Int(7, 2);
            pricingDate = DateTime.Parse(dINFO.Body.Strng(8, 2));
            pricingMD5 = dINFO.Body.Strng(9, 2);
            Mod m = mj.SetFromModJournal(name, dir);
            strListRules = m.strListRules;
            return this;
#if ToReview    //8/5
            //7/5            iModJounal = getModJournal(name);
            //7/5            string dateJrn = getModJrnValue(Decl.MODEL_DATE);
            //7/5            if (isChangedStr(ref dateJrn, dINFO, 5, 2)) goto Rec;
            //7/5            date = DateTime.Parse(dateJrn);
            //8/5            if (date > DateTime.Now || date < old) goto Err;
            Err:
                Msg.F("SavedReport doc not exists", dir);
            Rec:
                Recover(dINFO.name, RecoverToDo.ChangedMod);
            SetFrSavedModelINFO(dir);
#endif
        }

        /// <summary>
        /// getSavedRaw() - read elements from TSmatchINFO/Raw; 
        ///     When Lines Count in this file != elementsCount = re-write it.
        /// </summary>
        /// <returns>updated list of elements in file and in memory</returns>
        public List<Elm> getSavedRaw()
        {
            Log.set("SR.getSavedRaw()");
            Docs docRaw = Docs.getDoc(sRaw, create_if_notexist: false);
            if (docRaw == null) Reset(sRaw);
            if (TS.isTeklaActive())
            {
                var ts = new TS();
                elementsCount = ts.elementsCount();
            }
            int cnt = docRaw.il - docRaw.i0 + 1;
            if (cnt != elementsCount) Reset(sRaw);

            List<Elm> elms = new List<Elm>();

            for (int i = docRaw.i0; i <= docRaw.il; i++)
            {
                string guid = docRaw.Body.Strng(i, 1);
                string mat = docRaw.Body.Strng(i, 2);
                string mat_type = docRaw.Body.Strng(i, 3);
                string prf = docRaw.Body.Strng(i, 4);
                double lng = docRaw.Body.Double(i, 5);
                double weight = docRaw.Body.Double(i, 6);
                double vol = docRaw.Body.Double(i, 7);
                Elm elm = new Elm(guid, mat, mat_type, prf, lng, weight, vol);
                elms.Add(elm);
            }
            MD5 = getMD5(elms);
            Docs docModelINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO);
            if (MD5 != docModelINFO.Body.Strng(6, 2)
                || elementsCount != docModelINFO.Body.Int(7, 2)) Reset(sINFO);
            Log.Trace("{ elmCount, MD5} ==", elms.Count, MD5);
            Log.exit();
            return elms;
        }

        public void WriterSavRep()
        {
            throw new NotImplementedException();
        }

        private bool isChangedStr(ref string str, Docs doc, int row, int col)
        {
            string strINFO = doc.Body.Strng(row, col);
            if (string.IsNullOrEmpty(str)) str = strINFO;
            return str != strINFO;
        }
        private bool isChangedInt(ref int n, Docs doc, int row, int col)
        {
            int nINFO = doc.Body.Int(row, col);
            if (n == 0) n = nINFO;
            return n != nINFO;
        }

        public void getSavedGroups()
        {
            if (elmGroups.Count == 0) Msg.F("SavedReport.getSavedGroup: elmGroups.Count = 0");
            string sRep = Decl.TSMATCHINFO_REPORT;
            Docs dRep = Docs.getDoc(sRep);
            if (dRep == null || dRep.il != (elmGroups.Count + dRep.i0 + 1)) Reset(sRep);
            double totalPrice = dRep.Body.Double(dRep.il, Decl.REPORT_SUPL_PRICE);
            if (totalPrice == 0) Pricing();
            int gr_n = dRep.i0;
            foreach (var gr in elmGroups)
            {
                string grPrice = dRep.Body.Strng(gr_n, Decl.REPORT_SUPL_PRICE);
                gr.totalPrice = Lib.ToDouble(grPrice);
                gr.SupplierName = dRep.Body.Strng(gr_n, Decl.REPORT_SUPPLIER);
                gr.CompSetName = dRep.Body.Strng(gr_n, Decl.REPORT_COMPSET);
                gr_n++;
            }
            pricingMD5 = get_pricingMD5(elmGroups);
        }
        public void getSavedRules()
        {
            Log.set("SR.getSavedRules(\"" + strListRules + "\")");
            strListRules = "17, 4, 5, 6, 7";    // 13/5 заглушка
            Log.set("SR.getSavedRules(\"" + strListRules + "\")");
            //7/5            strListRules = getModJrnValue(Decl.MODEL_R_LIST);
            foreach (int n in Lib.GetPars(strListRules))
                Rules.Add(new Rule.Rule(n));
            //8/5            ClosePriceLists();
            Log.exit();
        }

        public void CloseReport()
        {
            dINFO.Close();
        }
    } // end class SavedReport
} // end namespace