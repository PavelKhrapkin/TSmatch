/*-----------------------------------------------------------------------------------
 * SavedReport -- class for handle saved reports in TSmatchINFO.xlsx
 * 
 *  17.04.2017 П.Л. Храпкин
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

namespace TSmatch.SaveReport
{
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

        public Model.Model getSavedReport()
        {
            if (!Docs.IsDocExists(Decl.TSMATCHINFO_MODELINFO))
            {
                docModelINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO, create_if_notexist: true);
                created_new = true;
            }
            else
            {
                docModelINFO = Docs.getDoc(Decl.TSMATCHINFO_MODELINFO);
                if (!isReportConsistent()) Msg.F("SavedReport inconsistant");
            }
            return this;
        }
        #region --- ModelJournal -- позже перенести в отдельный класс - модуль
        private string modJournal(int iModJounal, int col)
        {
            Docs docJournal = Docs.getDoc(Decl.MODELS);
            return docJournal.Body.Strng(iModJounal, col);
        }

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

        private string getModJrn(int col)
        {
            Docs doc = Docs.getDoc(Decl.MODELS);
            return doc.Body.Strng(iModJounal, col);
        }
        #endregion --- ModelJournal -- позже перенести в отдельный класс - модуль

        private bool isReportConsistent()
        {
            if (docModelINFO.il < 7) return false;
            if (isChangedStr(ref name, docModelINFO, 2, 2)) return false;
            if (isChangedStr(ref dir, docModelINFO, 3, 2)) return false;
            iModJounal = getModJournal(name);
            string dateJrn = getModJrn(Decl.MODEL_DATE);
            if (isChangedStr(ref dateJrn, docModelINFO, 5, 2)) return false;
            date = DateTime.Parse(dateJrn);
            if (date > DateTime.Now) return false;
            if (!Docs.IsDocExists(Decl.TSMATCHINFO_RAW)) return false;
            docRaw = Docs.getDoc(Decl.TSMATCHINFO_RAW);
            elmCntSav = docRaw.Body.iEOL() - docRaw.i0 +1;
            if (elmCntSav != docModelINFO.Body.Int(7, 2)) return false;
            elements = getSavedRaw();
            getGroups();
            if (!Docs.IsDocExists(Decl.TSMATCHINFO_REPORT)) return false;
            docReport = Docs.getDoc(Decl.TSMATCHINFO_REPORT);
            if (elmGroups.Count() != docReport.il - docReport.i0) return false;
            getSavedGroups();
            getSavedRules();
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

        public void SaveReport()
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
            strListRules = getModJrn(Decl.MODEL_R_LIST);
            foreach (int n in Lib.GetPars(strListRules))
                Rules.Add(new Rule.Rule(n));
            ClosePriceLists();
        }

        public void CloseReport()
        {
            docModelINFO.Close();
        }
    } // end class SavedReport
} // end namespace 
