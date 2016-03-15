/*------------------------------------------------------------------------------------------
 * Model -- класс управления моделями, ведет Журнал Моделей и управляет их сохранением
 * 
 *  7.3.2016 П.Храпкин
 *  
 *--- журнал ---
 * 18.1.2016 заложено П.Храпкин, А.Пасс, А.Бобцов
 * 29.2.2016 bug fix in getGroup
 *  6.3.2016 список Правил в стрке Модели, setModel(name); openModel,readModel
 * -----------------------------------------------------------------------------------------
 *      КОНСТРУКТОРЫ: загружают Журнал Моделей из листа Models в TSmatch или из параметров
 * Model(DateTime, string, string, string, string md5, List<Mtch.Rule> r)   - простая инициализация
 * Model( .. )      - указаны все данные модели, кроме даты - записываем в список моделей TSmatch Now
 * Model(doc, n)    - инициализируем экземпляр модели из строки n Документа doc
 * Model(n)         - инициализируем экземпляр модели из строки n TSmatch.xlsx/Models
 *
 *      МЕТОДЫ:
 * Start()         - инициирует начальную загрузку Журнала Моделей; возвращает список имен моделей
 * getModel(name)  - ищет модель по имени name в журнале моделей
 * setModel(name)  - подготавливает обработку модели name; читает все файлы компонентов
 * saveModel(name) - сохраняет модель с именем name
 * UpdateFrTekla   - обновление модели из данных в C# в файловую систему (ЕЩЕ НЕ ОТЛАЖЕНО!!)
 * modelListUpdate(name, dir, Made, MD5) - update list of models in TSmatch.xlsx/Models
 ! openModel()      - open model with OpenFileDialog from File System
 ! readModel(doc)   - read model (TSmatchINFO.xlsx) from dDocument
 * ReсentModel(List<Model> models) - return most recent model in list
 * IsModelCahanged - проверяет, изменилась ли Модель относительно сохраненного MD5
 * lngGroup(atr)   - группирует элементы модели по парам <Материал, Профиль> возвращая массивы длинны 
 */
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Linq;
//!!using System.Text.RegularExpressions;

using Decl = TSmatch.Declaration.Declaration;
using Lib = match.Lib.MatchLib;
using Docs = TSmatch.Document.Document;
using Mtch = TSmatch.Matcher.Matcher;
using Cmp = TSmatch.Component.Component;
using TS = TSmatch.Tekla.Tekla;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;

using FileOp = match.FileOp.FileOp;

namespace TSmatch.Model
{
    public class Model : IComparable<Model>
    {
        static List<Model> Models = new List<Model>();
        static List<TS.AttSet> Elements = new List<TS.AttSet>();    //stored in TSmatchINFO.xlsx/Models
        //-- stored in TSmatch.xlsx/Models fields; Rules as line number list f.e."6,7,8,11"
        private DateTime date;      // дата и время последнего обновления модели
        public string name;         // название модели
        private string dir;         // каталог в файловой системе, где хранится модель
        private string Made;        // выполненная процедуры TSmatch, после которой получен MD5
        private string MD5;         // контрольная сумма отчета по модели
        public readonly List<Mtch.Rule> Rules;  // список Правил, используемых с данной моделью
        private string strListRules;            // список Правил в виде текста вида "5,6,8"

        public int CompareTo(Model mod) { return mod.date.CompareTo(date); }    //to Sort Models by time

        public Model(DateTime t, string n, string d, string m, string md5, List<Mtch.Rule> r, string s)
        {
            this.date = t;
            this.name = n;
            this.dir = d;
            this.Made = m;
            this.MD5 = md5;
            this.Rules = r;
            this.strListRules = s;
        }
        public Model(string _name, string _dir, string _made, string _md5)
            : this(DateTime.Now, _name, _dir, _made, _md5, new List<Mtch.Rule>(), "")
        { }
        public Model(string _name, string _dir, string _made, string _md5
            , List<Mtch.Rule> _rules, string _strRuleList)
           : this(DateTime.Now, _name, _dir, _made, _md5, _rules, _strRuleList)
        { }
        public Model(Docs doc, int i)
        {
            this.date = Lib.getDateTime(doc.Body[i, Decl.MODEL_DATE]);
            this.name = (string)doc.Body[i, Decl.MODEL_NAME];
            this.dir = (string)doc.Body[i, Decl.MODEL_DIR];
            this.Made = (string)doc.Body[i, Decl.MODEL_MADE];
            this.MD5 = (string)doc.Body[i, Decl.MODEL_MD5];
            // преобразуем список Правил из вида "5,6,8" в List<Rule>
            List<Mtch.Rule> _rules = new List<Mtch.Rule>();
            this.strListRules = doc.Body.Strng(i, Decl.MODEL_R_LIST);
            foreach (int n in Mtch.GetPars(this.strListRules))
                _rules.Add(new Mtch.Rule(n));
            this.Rules = _rules; 
        }
        public Model(int i) : this(Docs.getDoc(Decl.MODELS), i) { }
        /// <summary>
        /// Model.Start() - начинает работу со списком моделей, инициализирует структуры данных
        /// </summary>
        /// <returns></returns>
        /// <journal>12.2.2016<\journal>
        public static List<string> Start()
        {
            Log.set("Model.Start");
            Docs doc = Docs.getDoc(Decl.MODELS);
            for (int i = doc.i0; i <= doc.il; i++)
                if( doc.Body[i, Decl.MODEL_NAME] != null ) Models.Add(new Model(doc, i));
            List<string> strLst = new List<string>();
            foreach (var m in Models) strLst.Add(m.name);
            strLst.Sort();
            Log.exit();
            return strLst;
        }
        /// <summary>
        /// getModel(name) - ищет модель по имени name в журнале моделей 
        /// </summary>
        /// <param name="name">имя искомой модели</param>
        /// <returns>найденную в журнале Модель</returns>
        public static Model getModel(string name)
        {
            Log.set("Model(\"" + name + "\")");
            Model result = null;
            foreach (var md in Models)
                if (md.name == name) { result = md; break; }
            Log.exit();
            return result;
        }
        /// <summary>
        /// saveModel(Model md)  - записываем измененную модель в файловую систему
        /// </summary>
        /// <param name="name">имя модели для записи.</param>
        /// используются вспемогательные перегруженные методы modJrnLine для записи в
        /// существующую строку Models и для добавления новой модели
        public static Model saveModel(string name)
        {
            Log.set("saveModel(\"" + name + "\")");
            Docs jrn = Docs.getDoc(Decl.MODELS);
            jrn.Reset();
            foreach (var m in Models)
            {
                string t = Lib.timeStr(m.date);
                jrn.Body.AddRow(new object[] { t, m.name, m.dir, m.Made, m.MD5, m.strListRules });
            }
            jrn.isChanged = true;
            Docs.saveDoc(jrn);
            Log.exit();
            return getModel(name);
        }
        /// <summary>
        /// UpdateFrTekla() - обновление модели из данных из C# в файловую систему
        /// </summary>
        /// <journal>13.2.2016
        ///  5.3.16 setComp in UpgradeFrTekla
        ///  <\journal>
        public static void UpdateFrTekla()
        {
            Log.set(@"UpdateFrTekla()");
            Elements = TS.Read();
            new Log(@"Модель = " + TS.ModInfo.ModelName + "\t" + Elements.Count + " компонентов.");

            Model mod = modelListUpdate(TS.ModInfo.ModelName, TS.ModInfo.ModelPath, TS.MyName, TS.ModAtrMD5());

            if (mod == null || !FileOp.isFileExist(mod.dir + "\\" + mod.name) 
                            || TS.MyName != mod.Made || TS.ModelMD5 != mod.MD5)
            {
                new Log("=== Это новая или измененная модель. Запишем ее в файл. ===");
                if (mod == null)
                    Models.Add(new Model(TS.ModInfo.ModelName, TS.ModInfo.ModelPath, TS.MyName, TS.ModelMD5));
                else
                {
                    Models.Remove(mod);
                    Models.Add(new Model(TS.ModInfo.ModelName, TS.ModInfo.ModelPath
                        , TS.MyName, TS.ModelMD5, mod.Rules, mod.strListRules));
                }
                mod = getModel(TS.ModInfo.ModelName);
                Docs raw = Docs.getDoc(Decl.RAW);

                int B = 1000, ii = 0, tostr = 1; DateTime t0 = DateTime.Now;
                while (ii < Elements.Count)
                {
                    var elm = Elements[ii++];
                    raw.Body.AddRow(new object[]
                        { elm.mat, elm.prf, elm.lng, elm.weight/1000, elm.volume/1000/1000/1000 });
                    if (ii % B == 0)
                    {
                        FileOp.saveRngValue(raw.Body, tostr);
                        tostr += raw.Body.iEOL();
                        elm = Elements[ii++];
                        raw.Body.Init(new object[] 
                            { elm.mat, elm.prf, elm.lng, elm.weight, elm.volume });
                    }
                }
                FileOp.saveRngValue(raw.Body, tostr);
                new Log("Время записи в файл (помимо чтения из Tekla) t=" + (DateTime.Now - t0).ToString() + " сек");
                raw.isChanged = true;

                Docs.saveDoc(raw, BodySave:false, MD5:mod.MD5, EOL:Elements.Count+1);
                //--- запишем ModelINFO
                Docs modInfo = Docs.getDoc(Decl.MODELINFO);
                modInfo.Body.AddRow(new object[] { "Model Name =", mod.name });
                modInfo.Body.AddRow(new object[] { "Model Directory =", mod.dir });
                modInfo.Body.AddRow(new object[] { "Current Phase =", TS.ModInfo.CurrentPhase });
                modInfo.Body.AddRow(new object[] { "Date =", Lib.timeStr(mod.date) });
                modInfo.Body.AddRow(new object[] { mod.Made, mod.MD5 });
                modInfo.Body.AddRow(new object[] { "Total elements=", Elements.Count });
                modInfo.isChanged = true;
                Docs.saveDoc(modInfo);
                saveModel(mod.name);    // а теперь запишем в Журнал Моделей обновленную информацию
                getGroups();
                setModel(mod.name);
            }
            else new Log("------- Эта модель уже есть в TSmatch. Ничего не записываем --------");
            Elements.Clear();
            Log.exit();
        } // end update
        /// <summary>
        /// modelListUpdate(name, dir, Made, MD5) - update list of models in TSmatch.xlsx/Models
        /// </summary>
        /// <param name="name">Model name</param>
        /// <param name="dir">Model path in File sistem</param>
        /// <param name="Made">version name of TS.Read - important as AttSet field list identifier</param>
        /// <param name="MD5">checksum of all Model parts</param>
        /// <returns>Model, updated in the list of models in TSmatch</returns>
        /// <journal> 6.3.2016 PKh </journal>
        static Model modelListUpdate(string name, string dir=null, string Made=null, string MD5=null, string str=null)
        {
            Log.set("modelListUpdate");
            Models.Clear();  Start();        // renowate Models list from TSmatch.xlsx
            Model mod = getModel(name);
            if (mod == null)    // mod==null - means this is completely new model
                Models.Add(new Model(name, dir, Made, MD5));
            else
            {
                if (dir  != null) mod.dir  = dir;
                if (Made != null) mod.Made = Made;
                if (MD5  != null) mod.MD5  = MD5;
                if (str != null)
                {
                    mod.strListRules = str;
                    foreach (int n in Mtch.GetPars(str))
                        mod.Rules.Add(new Mtch.Rule(n));
                }
//!!!!!!!!!!!!!!!!!!!!!!!!!!!! ЗДЕСЬ
// 1) проверить, доступен ли каталог dir? Если нет -> запустить FileWindowsDialog, потом рекурсивно вызвать modelListUpdate
// 2) проверить, изменился ли MD5 и список Правил str? Если нет -> список моделей не переписываем, оставляем прежднюю дату

            }
            Log.exit();
            return mod;
        }
        /// <summary>
        /// setModel(name) - подготавливает обработку модели name; читает все файлы компонентов
        /// </summary>
        /// <param name="name">имя модели</param>
        public static void setModel(string name)
        {
            Log.set(@"setModel(" + name + ")");
            Model mod = getModel(name);
            //-- setComp for all Rules of the Model
            foreach (var r in mod.Rules)
            {
                Cmp.setComp(Docs.getDoc(r.doc));
            }
            Log.exit();
        }
        public static void openModel()
        {
            Log.set("openModel");
            FolderBrowserDialog ffd = new FolderBrowserDialog();
            string dir = ffd.SelectedPath = ReсentModel(Models).dir;
            DialogResult result = ffd.ShowDialog();
            if (result == DialogResult.OK) dir = ffd.SelectedPath;

            string FileName = "TSmatchINFO.xlsx";
            bool ok = false;
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
                ok = readModel(out Elements, dir, FileName);
//!!!                if(!ok) Msg.Ask(Еще раз?) break;
            } while (!ok);
            Log.exit();
        }
        /// <summary>
        /// readModel(dir, FileName) - чтение модели из файла в Документ TMP в Elements
        ///                            список компонентов, отличающихся от одноименной модели,
        ///                            записывается к List<TS.AttSet>Elements.
        /// </summary>
        /// <param name="dir">каталог модели</param>
        /// <param name="FileName">имя файла для чтения</param>
        /// <returns>true - модель загружена в Elements</returns>
        /// <journal> 8.3.2016 </journal>
        private static bool readModel(out List<TS.AttSet> elm, string dir = null, string FileName = "TSmatchINFO.xlsx")
        {
            Log.set("readModel(" + dir + ", " + FileName + ")");
            bool ok = false;
            Docs.setDocTemplate(Decl.TOC_TEMPL_TMP, dir);
            Docs tmp = Docs.getDoc(Decl.TMP_RAW);
            Docs tmpINFO = Docs.getDoc(Decl.TMP_MODELINFO);
            List<TS.AttSet> diff = new List<TS.AttSet>();
            throw new NotImplementedException(); //!!!!!!!!!!!!!!!!!!!!!!!!!!!
            elm = diff;
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Log.exit();
            return ok;
        }
        //private List<TS.AttSet> Diff(List<TS.AttSet> lst1, List<TS.AttSet> lst2)
        //{
        //    Log.set("Diff");
        //    //bool y = lst1.Equals(lst2);
        //    //List<TS.AttSet> diff = new List<TS.AttSet>();
        //    //diff = lst1.Except(lst2);
        //    IEnumerable<TS.AttSet> dif = lst1.Except(lst2);
        //    //int c1 = lst1.Count;
        //    //int maxCnt = Math.Max(lst1.Count, lst2.Count);
        //    //for (int i1 = 0, i2 = 0, i = 0; i < maxCnt; i++, i1++, i2++)
        //    //{
        //    //    if (lst1[i].Equals(lst2[i])) continue;
        //    //    // try to skip elements in lst1
        //    //    int d1 = 0, d2 = 0, n1 = i1 + d1, n2 = i2 + d2;
        //    //    do
        //    //    {
        //    //        n1 = i1 + d1++;
        //    //        if (n1 == lst1.Count) break;
        //    //    } while (!lst1[n1].Equals(lst2[n2]));
        //    //    int dif1 = --d1;
        //    //    // try to skip elements in lst2
        //    //    d1 = 0; d2 = 0; n1 = i1 + d1; n2 = i2 + d2;
        //    //    do
        //    //    {
        //    //        n2 = i2 + d2++;
        //    //        if (n2 == lst2.Count) break;
        //    //    } while (!lst1[n1].Equals(lst2[n2]));
        //    //    int dif2 = --n2;

        //    //    if (dif1 <= dif2)
        //    //    {
        //    //        for(int j1 = 0; j1 < dif1; j1++) diff
        //    //        i1 += dif1; 

        //    //    diff.Add(lst1[i]);
        //    //}

        //    Log.exit();
        //    return diff;
        //}
        /// <summary>
        /// ReсentModel(List<Model> models) -- return most recent model in list
        /// </summary>
        /// <param name="models">model list</param>
        /// <returns>most recently saved Model in the list</returns>
        public static Model ReсentModel(List<Model> models)
        {
            Log.set("ReсentModel");
            Model mod = null;
            if (models.Count > 0)
            {
                models.Sort();
                mod = models[0];
            }
            Log.exit();
            return mod;
        }
        public struct Group
        {
            public string mat, prf;
            public double lng, wgt, vol;
            public readonly List<string> GUIDs; // List of ID Parts in the Group

            public Group(string _mat, string _prf,
                         double _lng, double _wgt, double _vol,
                         List<string> _guids) : this()
            {
                mat = Lib.ToLat(_mat); prf = Lib.ToLat(_prf);
                lng = _lng; wgt = _wgt; vol = _vol;
                GUIDs = _guids;
            }
        }
        public static List<Group> Groups = new List<Group>();
        public static List<Group> getGroups()
        {
            string mat = "", prf = "";
            double lng = 0.0, wgt = 0.0, vol = 0.0;
            List<string> guids = new List<string>();
            foreach (var v in Elements)
            {
                if (v.lng == 0.0) continue;
                if (mat == v.mat && prf == v.prf)
                {
                    lng += v.lng;
                    wgt += v.weight/1000;
                    vol += v.volume/1000/1000/1000;
                    guids.Add(v.guid);
                }
                else
                {
                    if (lng > 0.0) Groups.Add(new Group(mat, prf, lng, wgt, vol, guids));
                    mat = v.mat; prf = v.prf; lng = v.lng; wgt = v.weight/1000;
                    vol = v.volume/1000/1000/1000;
                    guids.Clear(); guids.Add(v.guid);
                }
            }
            return Groups;
        }
        //public static void lngGroup(dynamic atr)
        //{
        //    Log.set("lngGroup");
        //    if (atr.GetType() != typeof(List<TS.AttSet>)) Log.FATAL("ПОКА Я УМЕЮ РАБОТАТЬ ТОЛЬКО С TSread, но вскоре...");
        //    List<TS.AttSet> Elements = atr;
        //    Elements.Sort();
        //    foreach (var elm in Elements)
        //    {
        //        Group grp = new Group(elm.mat, elm.prf);
        //    }
        //    Log.exit();
        //}
    } // end class Model
} // end namespace Model
