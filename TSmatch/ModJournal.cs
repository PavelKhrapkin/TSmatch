/*--------------------------------------------------------------------------------------------
 * ModJournal : Model -- model journal management class
 * 
 *  12.05.2017 Pavel Khrapkin
 *  
 *--- History ---
 *  3.05.2017 get from SavedReport : Model code
 *--- Unit Tests --- 
 * -------------------------------------------------------------------------------------------
 *      Methods:
 * setFromModJournal(name, dir) - set Model attributes from Model Journal
 * getModJournal(name,dir)      - find Model line number in TSmatch.xlsx/Models by name and dir
 * getModJrnValue(int col)      - return string value in column col line iModJournal
 * SaveModJournal()             - When new model found, i.e. open in CAD - re-write whole Model Journal.
 * ChangeModJournal(mod)
 */
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lib = match.Lib.MatchLib;
using Decl = TSmatch.Declaration.Declaration;
using Msg = TSmatch.Message.Message;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;

namespace TSmatch.Model.Journal
{
    public class ModJournal : Mod
    {
        public static readonly ILog log = LogManager.GetLogger("ModJournal");

        List<Mod> models;   // журнал моделей
        int iModJournal;    // номер строки в Журнале Моделей по TSmatch.xlsx/Models

        public ModJournal(List<Mod> _models)
        { models = _models; }

        public Mod SetFromModJournal(string name, string dir)
        {
            Mod m = getModJournal(name, dir);
            date = m.date;
            strListRules = m.strListRules;
            return m;
        }
        /// <summary>
        /// getModJournal(name, dir) - get model from Model Journal in TSmatch.xlsx
        /// 
        /// When new model found, f.e. open in CAD - re-write whole Model Journal.
        /// </summary>
        /// <param name="name">name of model in Journal. If empty - most recent</param>
        /// <param name="dir">directory of model in Journal. If empty - by name</param>
        /// <returns>Mod in List<Mod></Mod>models</returns>
        /// <ToDo>10.4.17 реализовать default name и dir </ToDo>
        public Mod getModJournal(string name = "", string dir = "")
        {
            Docs doc = Docs.getDoc(Decl.MODELS);
            if (name == "") throw new NotImplementedException();
            Mod m = models.Find(x => x.name == name && x.dir == dir);
            if (m != null) return m;
            // new Model - add to ModJournal and save
            Mod mod = new Mod();
            mod.name = name;
            mod.dir = dir;
            mod.date = DateTime.Now;
            mod.elementsCount = elementsCount;
            models.Add(mod);
            //3/5            Msg.I("new record in Model Journul", name, dir);
            SavModJournal();
            return mod;
        }

        private string getModJrnValue(int col)
        {
            Docs doc = Docs.getDoc(Decl.MODELS);
            return doc.Body.Strng(iModJournal, col);
        }

        private string modJournal(int iModJournal, int col)
        {
            Docs docJournal = Docs.getDoc(Decl.MODELS);
            return docJournal.Body.Strng(iModJournal, col);
        }

        internal void SynchModJournal(Mod modelInCad)
        {
//16/5            throw new NotImplementedException();
        }

        public void ChangeModJournal(Mod mod)
        {
            Mod m = models.Find(x => x.name == mod.name && x.dir == mod.dir);
            throw new NotFiniteNumberException();
            //ToDo: отдельно разбираться, когда изменилось только имя или только dir
            SavModJournal();
        }

        public void SavModJournal()
        {
            if (models == null || models.Count < 1) Msg.F("ModJournal internal error");
            models.Sort();
            Docs doc = Docs.getDoc(Decl.MODELS);
            doc.Reset("Now");
            doc.wrDocSetForm("FORM_Models_in_TSmatch.xlsx");
            foreach (var mod in models)
            {
                string dt = mod.date.ToString("d.MM.yy H:mm");
                string sPhase = "'" + mod.phase;
                doc.wrDocForm(dt, mod.name, mod.dir, ifcPath,
                    mod.made, sPhase, mod.MD5, mod.strListRules);
            }
            doc.isChanged = true;
            doc.saveDoc();
        }
    } // end class ModJournal
} // end namespace 
