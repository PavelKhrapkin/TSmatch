/*-----------------------------------------------------------------------------------
 * SavedReport -- class for handle saved reports in TSmatchINFO.xlsx
 * 
 *  24.04.2017 П.Л. Храпкин
 *  
 *--- Unit Tests ---
 *--- History  ---
 * 17.04.2017 выделен из модуля Model
 * -------------- TODO --------------
 * 17.04.2017 - !ПОЗЖЕ! вынести методы ModelJourlal в отдельный класс
 * ---------------------------------------------------------------------------------
 *      Methods:
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

using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using Mod = TSmatch.Model.Model;
using TS = TSmatch.Tekla.Tekla;

namespace TSmatch.SaveReport
{
    delegate void BadSavRep();

    class BadRepFound
    {
        public event BadSavRep UserEvent;
        public void OnUserEvent() { UserEvent(); }
    }

    public class SavedReport : Mod
    {
        public static readonly ILog log = LogManager.GetLogger("SavedReport");

        Docs docModelINFO;
        Docs docRaw;
        int elmCntSav;
        string md5Sav;
        int iModJounal;
        bool created_new = false;

        public SavedReport()
        { }

        public void SavRepHandler()
        {
            // write / reset TSmatchINFO.xlsx -- which Sheet?
            throw new NotFiniteNumberException();
        }

        public void getSavedReport()
        {
            if (isReportConsistent()) return;
            TS ts = new TS();
            //ToDo 21/4/17: когда буду делать САПР помимо Tekla, здесь переписать!
            if (!TS.isTeklaActive()) Msg.F("SavedReport inconsistant and no Tekla");
            name = TS.getModInfo();
            dir = TS.ModInfo.ModelPath;
            iModJounal = getModJournal(name, dir);
            date = Lib.getDateTime(getModJrnValue(Decl.MODEL_DATE));
            docModelINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO
                , create_if_notexist: true, reset: true);
            wrModel(WrMod.ModelINFO);
            Read();
            getSavedRules();
            Handler();
            wrModel(WrMod.Report);
            if (!isReportConsistent()) Msg.F("internal errr");
        }

        private void BadReport_UserEvent()
        {
            throw new NotImplementedException();
        }
        #region --- ModelJournal -- позже перенести в отдельный класс - модуль
        /// <summary>
        /// getModJournal(name, dir) - get model from Model Journal in TSmatch.xlsx
        /// </summary>
        /// <param name="name">name of model in Journal. If empty - most recent</param>
        /// <param name="dir">directory of model in Journal. If empty - by name</param>
        /// <returns>line number in TSmatch.xlsx/Models</returns>
        /// <ToDo>10.4.17 реализовать default name и dir </ToDo>
        public int getModJournal(string name = "", string dir = "")
        {
            Docs doc = Docs.getDoc(Decl.MODELS);
            if (name == "") throw new NotImplementedException();
            for (int i = doc.i0; i <= doc.il; i++)
            {
                if (dir != "" && dir != doc.Body.Strng(i, Decl.MODEL_DIR)) continue;
                if (name != doc.Body.Strng(i, Decl.MODEL_NAME)) continue;
                return i;
            }
            throw new NotFiniteNumberException();
            // тут надо записывать в Журнал Моделей
            Msg.I("new record in Model Journul", name, dir);
            return -1;
        }

        private string getModJrnValue(int col)
        {
            Docs doc = Docs.getDoc(Decl.MODELS);
            return doc.Body.Strng(iModJounal, col);
        }

        private string modJournal(int iModJounal, int col)
        {
            Docs docJournal = Docs.getDoc(Decl.MODELS);
            return docJournal.Body.Strng(iModJounal, col);
        }
        #endregion --- ModelJournal -- позже перенести в отдельный класс - модуль

        private bool isReportConsistent()
        {
            BadRepFound badRep = new BadRepFound();
            badRep.UserEvent += BadReport_UserEvent;
            badRep.OnUserEvent();

            string repNm = Decl.TSMATCHINFO_MODELINFO;
            if (!Docs.IsDocExists(repNm)) return false;
            docModelINFO = Docs.getDoc(repNm);
            if (docModelINFO.il < 7) return false;
            if (isChangedStr(ref name, docModelINFO, 2, 2)) return false;
            //24/4            if (isChangedStr(ref dir, docModelINFO, 3, 2)) return false;  //разрешить менять dir
            iModJounal = getModJournal(name);
            string dateJrn = getModJrnValue(Decl.MODEL_DATE);
            if (isChangedStr(ref dateJrn, docModelINFO, 5, 2)) return false;
            date = DateTime.Parse(dateJrn);
            if (date > DateTime.Now) return false;
            if (!Docs.IsDocExists(Decl.TSMATCHINFO_RAW)) return false;
            docRaw = Docs.getDoc(Decl.TSMATCHINFO_RAW);
            elmCntSav = docRaw.Body.iEOL() - docRaw.i0 + 1;
            if (elementsCount == 0) elementsCount = elmCntSav;
            if (elementsCount != elmCntSav) return false;
            if (elmCntSav != docModelINFO.Body.Int(7, 2)) return false;
            elements = getSavedRaw();
            getGroups();
            if (!Docs.IsDocExists(Decl.TSMATCHINFO_REPORT)) return false;
            docReport = Docs.getDoc(Decl.TSMATCHINFO_REPORT);
            if (elmGroups.Count() != docReport.il - docReport.i0) return false;
            getSavedGroups();
            getSavedRules();
            if (Rules.Count <= 0) return false;
            if (docReport.Body.Double(docReport.il, Decl.REPORT_SUPL_PRICE) <= 0.0) return false;
            return true;
        }

        private List<Elm> getSavedRaw()
        {
            Docs docRaw = Docs.getDoc(Decl.TSMATCHINFO_RAW, create_if_notexist: false);
            List<Elm> elms = new List<Elm>();
            int cnt = docRaw.Body.iEOL();
            for (int i = docRaw.i0; i <= cnt; i++)
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

        private void getSavedGroups()
        {
            int gr_n = docReport.i0;
            foreach (var gr in elmGroups)
            {
                string grPrice = docReport.Body.Strng(gr_n, Decl.REPORT_SUPL_PRICE);
                gr.totalPrice = Lib.ToDouble(grPrice);
                gr.SupplierName = docReport.Body.Strng(gr_n, Decl.REPORT_SUPPLIER);
                gr.CompSetName = docReport.Body.Strng(gr_n, Decl.REPORT_COMPSET);
                gr_n++;
            }
        }
        public void getSavedRules()
        {
            strListRules = getModJrnValue(Decl.MODEL_R_LIST);
            foreach (int n in Lib.GetPars(strListRules))
                Rules.Add(new Rule.Rule(n));
            ClosePriceLists();
        }

        ////////////////public event WriterSavRep BadSavRep;

        ////////////////protected virtual void OnBadSavRep(EventArgs e)
        // 28/04 ///////{
        ////////////////    EventHandler handler = BadSavRep;
        ////////////////    if (handler != null) handler(this, e);
        ////////////////}

        public void CloseReport()
        {
            docModelINFO.Close();
        }
    } // end class SavedReport
} // end namespace 
