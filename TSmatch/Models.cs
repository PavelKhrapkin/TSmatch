/*-----------------------------------------------------------------------------------------------
 * Model -- Manage major class operaions together with the child modules Handler and SavedMdel
 * 
 * 23.07.2017 Pavel Khrapkin
 *  
 *--- History ---
 * Jan-16 - May-17 pre-history P.Khrapkin, A.Pass, A.Bobtsov
 *  6.05.2017 - fast MD5 calculation call in Read
 *  8.05.2017 - part of this code moved to child Handler module
 * 11.05.2017 - getSavedReport() inside SetModel
 * 17.05.2017 - model.Save()
 * 19.05.2017 - Model.wrModel(Rules)
 * 23.05.2017 - HighLight Invoke
 * 24.05.2017 - exclude ModJournal from Project
 * 27.05.2017 - XML read and write model.elements as Raw.xml in wrMod, modelWr bug fix 
 * 11.07.2017 - Project address fields add to class Model
 * 14.07.2017 - SetModDir setarated from SetModet for UT_
 * 23.07.2017 - exclude elementsCound from Model propertties; now account elements.Count
 * --- Unit Tests ---
 * 2017.07.6  UT_Model.UT_SetModel   OK
 * 2017.07.14 UT_SetModDir, setCity  OK 
 * ---- METHODS: -----------------
 * Read(modelName) - получение модели (списка элементов с атрибутами) из Tekla или IFC
 * getModel(name)  - ищет модель по имени name в журнале моделей
 * setModel(name)  - подготавливает обработку модели name; читает все файлы компонентов
 * setCity(adr)    - parse string adr; set model.adrCity and model.adrStreet
 * saveModel(name) - сохраняет модель с именем name
 * UpdateFrTekla() - обновление модели из данных в C# в файловую систему (ЕЩЕ НЕ ОТЛАЖЕНО!!)
 * modelListUpdate(name, dir, Made, MD5) - update list of models in TSmatch.xlsx/Models
 ! openModel()      - open model with OpenFileDialog from File System
 ! readModel(doc)   - read model (TSmatchINFO.xlsx) from dDocument
 * ReсentModel(List<Model> models) - return most recent model in list
 * IsModelCahanged - проверяет, изменилась ли Модель относительно сохраненного MD5
 ! lngGroup(atr)   - группирует элементы модели по парам <Материал, Профиль> возвращая массивы длинны 
 * ModReset()      - clear and re-Read all Rules with theit Suppliers, CompSet, and price-lists 
 */
using System;
using System.Collections.Generic;
using System.IO;

using log4net;
using Boot = TSmatch.Bootstrap.Bootstrap;
using CmpSet = TSmatch.CompSet.CompSet;
using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using ElmGr = TSmatch.ElmAttSet.Group;
using ElmMGr = TSmatch.ElmAttSet.Mgroup;
using FileOp = match.FileOp.FileOp;
using Ifc = TSmatch.IFC.IFC;
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Supplier = TSmatch.Suppliers.Supplier;
using TS = TSmatch.Tekla.Tekla;
using SR = TSmatch.SaveReport.SavedReport;

namespace TSmatch.Model
{
    public class Model : IComparable<Model>
    {
        #region --- Definitions and Constructors
        public static readonly ILog log = LogManager.GetLogger("Model");

        public DateTime date;       // дата и время последнего обновления модели
        public string name;         // название модели
        public string dir;          // каталог в файловой системе, где хранится модель
        public string ifcPath;      // полный путь к ifc-файлу, соответствующему модели
        public string made;         // атрибут процедуры TSmatch, после которой получен MD5
        public string phase;        // текущая фаза проекта. В Tekla это int
        public string MD5;          // контрольная сумма отчета по модели
        public string adrCity;      // адрес строительства по модели
        public string adrStreet;    // улица, район, возможно, страна
        public double total_price;  // общая сумма стоимости материалов по модели
        public bool isChanged = false;
        public List<Elm> elements = new List<Elm>();
        public string pricingMD5;   // контр.сумма цен и правил при расчете Report
        public DateTime pricingDate;
        public List<ElmMGr> elmMgroups = new List<ElmMGr>();
        public List<ElmGr> elmGroups = new List<ElmGr>();   // will be used in Matcher
        public HashSet<Rule.Rule> Rules = new HashSet<Rule.Rule>();
        public string strListRules;                        // список Правил в виде текста вида "5,6,8"   
        public readonly HashSet<Supplier> Suppliers = new HashSet<Supplier>();
        public List<CmpSet> CompSets = new List<CmpSet>();
        public List<Matcher.Mtch> matches = new List<Matcher.Mtch>();
        public bool wrToFile = true;    // when true- we should write into the file TSmatchINFO.xlsx, else- no changes
        public bool errRecover = false; // when true - do Recovery with reWrite TSmatchINFO, else - write later on
        public Docs docReport;
        TS ts = new TS();
        public Handler.Handler mh;
        public SR sr;

        public int CompareTo(Model mod) { return mod.date.CompareTo(date); }    //to Sort Models by time

        public Model() { }

        #region --- Setup and Read CAD methods
        public void SetModel(Boot boot)
        {
            Log.set("SetModel");
            //create child class references mh, sr
            mh = new Handler.Handler();
            sr = new SR();
            SetModDir(boot);
            sr.GetTSmatchINFO(this);

            //23/7            date = sr.date;
            //23/7            elements = sr.elements;
            //23/7            elmGroups = sr.elmGroups;
            //23/7            pricingDate = sr.pricingDate;
            Log.exit();
        }

        public void SetModDir(Boot boot)
        {
            if (boot.isTeklaActive)
            {   // if Tekla is active - get Path of TSmatch
                name = Path.GetFileNameWithoutExtension(TS.ModInfo.ModelName);
                dir = TS.GetTeklaDir(TS.ModelDir.model);
                phase = TS.ModInfo.CurrentPhase.ToString();
                //6/4/17                        macroDir = TS.GetTeklaDir(TS.ModelDir.macro);
                HighLightClear();
            }
            else
            {   // if Tekla not active - get model attributes from TSmatchINFO.xlsx in ModelDir
                dir = boot.ModelDir;
                if (!FileOp.isDirExist(dir)) Msg.F("No Model Directory", dir);
                if (!Docs.IsDocExists(Decl.TSMATCHINFO_MODELINFO)) Msg.F("No TSmatchINFO.xlsx file");
                if (sr == null) sr = new SR();
                Model m = sr.SetFrSavedModelINFO(this);
                name = m.name;
                phase = m.phase;

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

        public void setCity(string adr)
        {
            string[] adrs = adr.Split(',');
            adrCity = adrs[0].Trim();
            int indx = adr.IndexOf(',');
            if (indx > 0) adrStreet = adr.Substring(indx + 1).Trim();
            else adrStreet = string.Empty;
        }

        /// <summary>
        /// Read() - получение модели (списка элементов с атрибутами) из Tekla или IFC
        /// </summary>
        /// <returns>Model со списком прочитанных элементов в model.elements</returns>
        public Model Read()
        {
            elements.Clear();
            if (TS.isTeklaActive()) elements = ts.Read();
            else elements = Ifc.Read(ifcPath);
            //23/7            elementsCount = elements.Count;
            string newMD5 = getMD5(elements);
            if (newMD5 != MD5)
            {
                isChanged = true;
                MD5 = newMD5;
                date = DateTime.Now;
                if (sr == null) sr = new SR();
                sr.Save(this);
            }
            return this;
        }

        public string getMD5(List<Elm> elements)
        {
            var s_e = new List<Serialized_element>();
            foreach (var elm in elements) { s_e.Add(new Serialized_element(elm)); }
            return MD5gen.MD5HashGenerator.GenerateKey(s_e);
        }

        public string get_pricingMD5(List<ElmGr> elmGr)
        {
            var s_g = new List<Serialized_Group>();
            foreach (var gr in elmGr) { s_g.Add(new Serialized_Group(gr)); }
            return MD5gen.MD5HashGenerator.GenerateKey(s_g);
        }

#if OLD

            log.Info("TRACE: Read(\"" + modelName + "\")");
            Model mod = (modelName == "" || name == modelName)? this : getModel(modelName);
// 6/4/17 в дальнейшем надо завести поле CAD в Bootsrap и присваивать ему ts или Ifc
            if (TS.isTeklaActive())
            {
                readCAD readElements = new readCAD(ts.Read);
                elements = readElements(Path.Combine(dir, name));
            }
            else elements = Ifc.Read(mod.ifcPath);
                        log.Info(@"TRACE: Модель = " + name + "\t" + Elm.Elements.Count + " компонентов.");

        void getModelFrTekla()
        {
            //7/4                    var ts = boot.classCAD;
            ts = new TS();
            elements = ts.Read();
        }

        void getModelFrIfc()
        {
            throw new NotImplementedException();
            elements = Ifc.Read(ifcPath);
        }
#endif //OLD
        /// <summary>
        /// newModelOpenDialog(models) -- run when new Model must be open not exists in Model Journal models
        /// </summary>
        /// <param name="models">List<Models> to be updated after dialog</param>
        /// <returns>List<Model></Model>sorted models list -- models[0] - just opened Model</returns>
        /// <history>6.08.2016</history>
        internal static Model newModelOpenDialog(out List<Model> models)
        {
            Model result = null;
            string newFileDir = "", newFileName = "";

            throw new NotImplementedException();

            while (result != null)
            {
                if (FileOp.isFileExist(newFileDir, newFileName))
                {
                }
            }
            return result;
        }
        #endregion -=-=- unclear region to be clean-up
        #endregion --- Read CAD methods

        #region --- HighLight methods
        public enum HighLightMODE { NoPrice, Guids }
        public void HighLightElements(HighLightMODE mode, List<string> guids = null)
        {
            DateTime t0 = DateTime.Now;
            HighLightClear();
            var elms = new Dictionary<string, Elm>();
            foreach (var elm in elements) elms.Add(elm.guid, elm);
            var toBeHighghlited = new Dictionary<string, Elm>();
            switch (mode)
            {
                case HighLightMODE.NoPrice:
                    foreach (var gr in elmGroups)
                    {
                        if (gr.totalPrice != 0) continue;
                        foreach (string id in gr.guids)
                            toBeHighghlited.Add(id, elms[id]);
                    }
                    //30/5                    foreach (var elm in elements)
                    //30/5                        if (elm.price == 0) toBeHighghlited.Add(elm.guid, elm);
                    break;
                case HighLightMODE.Guids:
                    if (guids == null || guids.Count == 0) return;
                    foreach (string id in guids)
                        foreach (var elm in elements)
                            if (elm.guid == id) toBeHighghlited.Add(elm.guid, elm);
                    break;
            }
            HighLightElements(toBeHighghlited);
            log.Info("HighLight(" + mode.ToString() + ") "
                + toBeHighghlited.Count + " elements "
                + (DateTime.Now - t0));
        }
        public void HighLightElements(Dictionary<string, Elm> elms)
        {
            //23/5            if (TS.isTeklaActive()) ts.HighlightElements(elms);
            if (TS.isTeklaActive())
            {
                ts.HighlightElements(elms);

                //////////Type type = typeof(InvHLclass);
                // 31.5///MethodInfo info = type.GetMethod("InvHL");
                //////////info.Invoke(null, new object[] { elms });
            }
            else { } // in future here put the code for highlight in another Viewer
        }
        public void HighLightElements(ElmGr group, List<Elm> elements)
        {
            Dictionary<string, Elm> elms = new Dictionary<string, Elm>();

            foreach (string guid in group.guids)
            {
                Elm elm = elements.Find(x => x.guid == guid);
                elms.Add(guid, elm);
            }
            HighLightElements(elms);
        }

        internal void Highlight(List<ElmGr> grLst)
        {
            if (!TS.isTeklaActive()) return;
            var elms = new Dictionary<string, Elm>();
            foreach (var gr in grLst)
            {
                foreach (var id in gr.guids)
                {
                    elms.Add(id, elements.Find(elm => elm.guid == id));
                }
            }
            ts.HighlightElements(elms);
        }
        internal void Highlight(ElmGr group)
        {
            if (!TS.isTeklaActive()) return;
            var elms = new Dictionary<string, Elm>();
            foreach (var id in group.guids)
            {
                elms.Add(id, elements.Find(elm => elm.guid == id));
            }
            ts.HighlightElements(elms);
        }

        public void HighLightClear()
        {
            if (TS.isTeklaActive()) ts.HighlightClear();
        }
        static class InvHLclass
        {
            public static void InvHL(Dictionary<string, Elm> elms)
            {
                TS ts = new TS();
                ts.HighlightElements(elms);
            }
        }
        #endregion --- HighLight methods

        #region -=-=- unclear region 2 to be audited
#if OLD
        /// <summary>
        /// getModel(name) - get Model by name in TSmatch.xlsx/Model Journal  
        /// </summary>
        /// <param name="name">Model name; by default get most recent model of model from Tekla</param>
        /// <returns>found Model</returns>
        /// <history>5.6.2016
        /// 6.8.2016 - non static method</history>
        public Model getModel(string name)
        {
            Log.set("Model(\"" + name + "\")");
            Model result = null;
            if (string.IsNullOrEmpty(name)) result = RecentModel();
            else
            {
                List<Model> models = Start();
                result = models.Find(x => x.name == name);
                while (result == null)
                {
                    result = newModelOpenDialog(out models);
                }
            }
            Log.exit();
            return result;
        }

        public void ModReset()
        {
            Rules.Clear();
            foreach (int n in Lib.GetPars(strListRules))
                Rules.Add(new Rule.Rule(n));
            ClosePriceLists();
        }
        /// <summary>
        /// saveModel(Model md)  - записываем измененную модель в файловую систему
        /// </summary>
        /// <param name="name">имя модели для записи.</param>
        /// <history>6.8.16 -- nonstatic method</history>
        public Model saveModel(string name)
        {
            Log.set("saveModel(\"" + name + "\")");
            Docs doc = Docs.getDoc(Decl.MODELS);
            doc.Reset("Now");
            Models.Sort();
            foreach (var m in Models)
            {
                string t = Lib.timeStr(m.date);
                doc.wrDoc(1, t, m.name, m.dir, m.Made, m.Phase, m.MD5, m.strListRules);
            }
            doc.isChanged = true;
            Docs.saveDoc(doc);
            Log.exit();
            return getModel(name);
        }
//#if FOR_FUTORE  //6/4/2017
        public Model UpdateFrTekla()
        {
            Log.set(@"UpdateFrTekla()");
            //            Elm.Elements = TS.Read();
            Model mod = Read();
 //6/4           List<Elm> elements = TS.Read();
            new Log(@"Модель = " + TS.ModInfo.ModelName + "\t" + Elm.Elements.Count + " компонентов.");
            string mod_name = TS.ModInfo.ModelName;
            string mod_dir = TS.ModInfo.ModelPath;
            string mod_phase = TS.ModInfo.CurrentPhase.ToString();
//6/4            Model mod = modelListUpdate(mod_name, mod_dir, TS.MyName, Elm.ElementsMD5(), mod_phase);
//6/4            mod.elements = elements;
            if (mod.wrToFile) 
            {
                mod.wrModel(Decl.TSMATCHINFO_RAW);
                mod.wrModel(Decl.TSMATCHINFO_MODELINFO);
                ////ElmGr.setGroups();            // Group Elements by Materials and Profile
                ////ElmMGr.setMgr();        // Additionally group Groups by Material 
                setModel(mod.name);     // Load price-list for the model
//revision 2016.12.05                Mtch.UseRules(mod);     // Search for Model Groups matching Components
                mod.wrModel(Decl.TSMATCHINFO_REPORT);
                mod.wrModel(Decl.TSMATCHINFO_SUPPLIERS);
                saveModel(mod.name);    // а теперь запишем в Журнал Моделей обновленную информацию
            }
            else new Log("------- Эта модель уже есть в TSmatch. Ничего не записываем --------");
            Elm.Elements.Clear();
            mod.wrToFile = false;
            Log.exit();
            return mod;
        } // end update

        /// <summary>
        /// modelListUpdate(name, dir, Made, MD5) - update list of models in TSmatch.xlsx/Models
        /// </summary>
        /// <param name="name">Model name</param>
        /// <param name="dir">Model path in File sistem</param>
        /// <param name="Made">version name of TS.Read - important as AttSet field list identifier</param>
        /// <param name="MD5">checksum of all Model parts</param>
        /// <returns>Model, updated in the list of models in TSmatch</returns>
        /// <history> 6.3.2016 PKh
        /// 15.3.16 return Model instead of null in case of completely new model; wrToFile handle
        ///  5.4.16 Current Phase handling
        ///  6.8.16 non static method
        /// </history>
        Model modelListUpdate(string name, string dir = null, string Made = null,
                                     string MD5 = null, string Phase = null, string str = null)
        {
            Log.set("modelListUpdate");
            Models.Clear(); Start();        // renowate Models list from TSmatch.xlsx
            Model mod = getModel(name);
            if (mod == null)    // mod==null - means this is completely new model
            {
                Models.Add(new Model(name, dir, ifcPath, Made, Phase, MD5));
                mod = getModel(name);
                mod.wrToFile = true;
            }
            else
            {
                if (dir != null) mod.dir = dir;
                if (Made != null) mod.Made = Made;
                if (Phase != null) mod.Phase = Phase;
                if (MD5 != null) mod.MD5 = MD5;
                if (str != null)
                {
                    mod.strListRules = str;
                    foreach (int n in Lib.GetPars(str))
                        mod.Rules.Add(new TSmatch.Rule.Rule(n));
                }
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!! ЗДЕСЬ
                // 1) проверить, доступен ли каталог dir? Если нет -> запустить FileWindowsDialog, потом рекурсивно вызвать modelListUpdate
                // 2) проверить, изменился ли MD5 и список Правил str? Если нет -> список моделей не переписываем, оставляем прежднюю дату
                // 3) читать ModelINFO / MD5 в файле, чтобы понять, нужно ли в него переписать модель (установить флаг wrToFile)

            }
            Log.exit();
            return mod;
        }
        /// <summary>
        /// setModel(name) - подготавливает обработку модели name; читает все файлы компонентов
        /// </summary>
        /// <param name="name">имя модели</param>
        public void setModel(string name)
        {
            Log.set(@"setModel(" + name + ")");
            Model mod = getModel(name);
            //-- setComp for all Rules of the Model
            foreach (var r in mod.Rules)
            {
                //11.1.17                CmpSet cs = r.CompSet.getCompSet();
                //11.1.17                mod.CompSets.Add(cs);
                if (!mod.Suppliers.Contains(r.CompSet.Supplier)) mod.Suppliers.Add(r.Supplier);
            }
            foreach (var v in mod.CompSets) v.doc.Close();
            Log.exit();
        }
        public void setModel()
        {
            log.Info("TRACE: setModel(\"" + name + "\")");
            //!! временно для отладки запишем в mod.Sopplers ВСЕХ поставщиков. Потом - только тех, кто в RuleList
            //!!           this.Suppliers = TSmatch.Suppliers.Supplier.    //TSmatch.Startup.Bootstrap.init(Decl.SUPPLIERS);
        }

        /// <summary>
        /// OpenModel(name) - open model name from Excel file, Tekla, or ifc file. Selection, when necessary
        /// </summary>
        /// <param name="name">Model name. If empty - open most recent model; when not found -- dialog browth</param>
        /// <history> 5.6.2016 </history>
        const string RECENT_MODEL = "My most recent model";
        public void openModel(string name = RECENT_MODEL)
        {
            Log.set("openModel(\"" + name + "\")");
            Model mod = null;
            bool ok = false;
            if (TS.isTeklaActive())
            {
                //!! here we could upload over API in Tekla another model it differ from requested name
                //!! if (TS.isTeklaModel(mod.name))
                //!! implement it later on 23/6/2016
                UpdateFrTekla();
            }
            if (name == RECENT_MODEL) mod = RecentModel();
                                 else mod = getModel(name);

            string dir = mod.dir;
            string FileName = "TSmatchINFO.xlsx";
            if (mod.Made == "IFC") FileName = "out.ifc";
            if (mod != null) ok = FileOp.isFileExist(dir, FileName);

            if (!ok)
            {           //-- Folder or File Browth dialog
                FolderBrowserDialog ffd = new FolderBrowserDialog();
                dir = ffd.SelectedPath = mod.dir;
                DialogResult result = ffd.ShowDialog();
                if (result == DialogResult.OK) dir = ffd.SelectedPath;
                do
                {
                    if (!FileOp.isFileExist(dir, FileName))
                    {
                        Msg.W("W20.4_opMod_NO_TSmINFO", dir);
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.InitialDirectory = dir;
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            FileName = ofd.FileName;
                            // !!!!!!                       dir = ofd.Sel
                        }
                    }
                    ok = readModel(dir, FileName);
                    //!!!                if(!ok) Msg.Ask(Еще раз?) break;
                } while (!ok);
            }
            else
            {
                string ext = Path.GetExtension(FileName);
                if ( ext == ".ifc") Ifc.Read(dir, FileName); //!!
                if (ext == ".xlsx") dir= "";//!! readModel();
            }
            Log.exit();
        }
        private static bool readModel(string dir = null, string FileName = "TSmatchINFO.xlsx")
        {
            Log.set("readModel(" + dir + ", " + FileName + ")");
            bool ok = false;
//!!            Docs.setDocTemplate(Decl.TEMPL_TMP, dir);
            Docs tmp = Docs.getDoc(Decl.TMP_RAW);
            Docs tmpINFO = Docs.getDoc(Decl.TMP_MODELINFO);
//!!            List<TS.AttSet> diff = new List<TS.AttSet>();
            throw new NotImplementedException(); //!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!!            elm = diff;
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Log.exit();
            return ok;
        }
#endif //FOR_FUTURE 6/4/2017
        #endregion -=-=- unclear region 2 to be audited

        #region -=-=- unclear region 3 to be audited
#if OLD
        /// <summary>
        /// ReсentModel(List<Model> models) -- return most recent model in list
        /// </summary>
        /// <param name="models">model list</param>
        /// <returns>most recently saved Model in the list</returns>
        /// <history>
        /// 2016.11.21 - get RecentModel from TOC, not from memory
        /// </history>
        public Model ReсentModel(List<Model> models)
        {
            Log.set("ReсentModel");
            Model mod = null;
            if (models.Count > 0)
            {
                models.Sort();
                mod = models[0];
            }
            else mod = newModelOpenDialog(out models);
            Log.exit();
            return mod;
        }
        public static Model RecentModel()
        {
            string date = "1.1.52";
            int iMod = 0;
            Docs doc = Docs.getDoc(Decl.MODELS);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                if (doc.Body[i, Decl.MODEL_NAME] != null)
                {
                    string modDate = doc.Body.Strng(i, Decl.MODEL_DATE);
                    if (Lib.getDateTime(modDate) > Lib.getDateTime(date))
                    {
                        date = modDate;
                        iMod = i;
                    }
                }
            }
            return new Model(iMod, doInit: false);
        }
        public string ModelDir()
        {
            return this.dir;
        }
        public static string RecentModelDir() { return RecentModel().dir; }
#endif //OLD
        #endregion -=-=- unclear region 3 to be audited
#if OLD //23/7
        public void setElements(List<ElmAttSet.ElmAttSet> els)
        {
            elements.Clear();
            foreach (var elm in els) elements.Add(elm);
            elementsCount = elements.Count;
        }
        public void setElements(Dictionary<string, Elm> els)
        {
            elements.Clear();
            foreach (var elm in els) elements.Add(elm.Value);
            elementsCount = elements.Count;
        }

        private void getSuppliers()
        {
            //29.11            Suppliers = Supplier.Start(); //!! времянка - список всех Поставщиков
        }
#endif //OLD 23/7
        /// <summary>
        /// ifWrToFile() возвращает true, если модель изменилась относительно имеющейся в журнале записи о ней
        /// </summary>
        /// <returns></returns>
        private bool ifWrToFile()
        {
            //TODO 21/08/16 здесь вычислять MD5 прочитанной модели, сопоставлять результат с mod.MD5, если не совпадают - wrToFile = true;
            //              если wrToFile = false - больше ничего не делать 
            //////////////var controlSun = mod.elements.Sum(); для вычисления Sum надо реализовать IQuerible() с ComputeMD5()
            //////////////string md5 = mod.elements.MD5();    
            return true;    // пока не реализовано TODO - для отладки
        }
#if OLD //13/7/17
        internal void Report()
        {
            if (!wrToFile) return;
            wrModel(WrMod.ModelINFO);   // общая информация о модели: имя, директория, MD5 и др
            wrModel(WrMod.Raw);         // элементы с атрибутами, как они прочитаны из модели
            wrModel(WrMod.Materials);   // сводка по материалам, их типам (бетон, сталь и др)
            wrModel(WrMod.Suppliers);   // сводка по поставщикам проекта (контакты, URL прайс-листа, закупки)
            wrModel(WrMod.Rules);       // перечень Правил, используемых для обработки модели
            wrModel(WrMod.Report);      // отчет-сопоставление групп <материал, профиль> c прайс-листами поставщиков      
        }
#endif // OLD // 13/7/17
        /// <summary>
        /// Exit from TSmatch application
        /// </summary>
        internal void Exit()
        {
            //21/5            isRuleChanged = true; // для отладки
            bool modIsChanged = false;
            bool ruleIsChanged = false;

            if (modIsChanged && Msg.AskYN("Модель или цены изменились. Запишем изменения в файл?"))
            {
                var sr = new SaveReport.SavedReport();
                sr.Save(this, ruleIsChanged);
            }
            HighLightClear();
            FileOp.AppQuit();
        }
    } // end class Model
} // end namespace Model