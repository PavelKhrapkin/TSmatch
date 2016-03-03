/*------------------------------------------------------------------------------------------
 * Model -- класс управления моделями, ведет Журнал Моделей и управляет их сохранением
 * 
 *  2.3.2016 П.Храпкин
 *  
 *--- журнал ---
 * 18.1.2016 заложено П.Храпкин, А.Пасс, А.Бобцов
 * 29.2.2016 bug fix in getGroup
 * 02.3.2016 список Правил в стрке Модели
 * -----------------------------------------------------------------------------------------
 *      КОНСТРУКТОРЫ: загружают Журнал Моделей из листа Models в TSmatch или из параметров
 * Model(DateTime, string, string, string, string md5, List<Mtch.Rule> r)   - простая инициализация
 * Model( .. )      - указаны все данные модели, кроме даты - записываем в список моделей TSmatch Now
 * Model(doc, n)    - инициализируем экземпляр модели из строки n Документа doc
 *
 *      МЕТОДЫ:
 * Start()         - инициирует начальную загрузку Журнала Моделей; возвращает список имен моделей
 * getModel(name)  - ищет модель по имени name в журнале моделей
 * saveModel(name) - сохраняет модель с именем name
 * UpdateFrTekla   - обновление модели из данных в C# в файловую систему (ЕЩЕ НЕ ОТЛАЖЕНО!!)
 * IsModelCahanged - проверяет, изменилась ли Модель относительно сохраненного MD5
 * lngGroup(atr)   - группирует элементы модели по парам <Материал, Профиль> возвращая массивы длинны 
 */
using System.Collections.Generic;
using System;
//!!using System.Text.RegularExpressions;

using Decl = TSmatch.Declaration.Declaration;
using Lib = match.Lib.MatchLib;
using Docs = TSmatch.Document.Document;
using Mtch = TSmatch.Matcher.Matcher;
using TS = TSmatch.Tekla.Tekla;
using Log = match.Lib.Log;

using FileOp = match.FileOp.FileOp;

namespace TSmatch.Model
{
    public class Model
    {
        static List<Model> Models = new List<Model>();
        static List<TS.AttSet> mod_att = new List<TS.AttSet>();

        private DateTime date;      // дата и время последнего обновления модели
        public string name;         // название модели
        private string dir;         // каталог в файловой системе, где хранится модель
        private string Made;        // выполненная процедуры TSmatch, после которой получен MD5
        private string MD5;         // контрольная сумма отчета по модели
        public readonly List<Mtch.Rule> Rules; // список Правил, используемых с данной моделью 

        public Model(DateTime t, string n, string d, string m, string md5, List<Mtch.Rule> r)
        {
            this.date = t;
            this.name = n;
            this.dir = d;
            this.Made = m;
            this.MD5 = md5;
            this.Rules = r;
        }
        public Model(string _name, string _dir, string _made, string _md5, List<Mtch.Rule> _ruleList)
            : this(DateTime.Now, _name, _dir, _made, _md5, _ruleList) { }
        public Model(Docs doc, int i)
        {
            this.date = Lib.getDateTime(doc.Body[i, Decl.MODEL_DATE]);
            this.name = (string)doc.Body[i, Decl.MODEL_NAME];
            this.dir = (string)doc.Body[i, Decl.MODEL_DIR];
            this.Made = (string)doc.Body[i, Decl.MODEL_MADE];
            this.MD5 = (string)doc.Body[i, Decl.MODEL_MD5];
            // преобразуем список Правил из вида "5,6,8" в List<Rule>
            List<Mtch.Rule> _rules = new List<Mtch.Rule>();
            string lstr = doc.Body.Strng(i, Decl.MODEL_R_LIST);
            foreach (int n in Mtch.GetPars(lstr))
                _rules.Add(new Mtch.Rule(n));
            this.Rules = _rules;
        }
        /// <summary>
        /// Model.Start() - начинает работу со списком моделей, инициализирует структуры данных
        /// </summary>
        /// <returns></returns>
        /// <journal>12.2.2016<\journal>
        public static List<string> Start()
        {
            Log.set("Model.Start");
            Docs doc = Docs.getDoc(Decl.MODELS);
            for (int i = 4; i <= doc.Body.iEOL(); i++)
                if( doc.Body[i, Decl.MODEL_NAME] != null ) Models.Add(new Model(doc, i));
            List<string> strLst = new List<string>();
            foreach (var m in Models) strLst.Add(m.name);
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
            jrn.Body[1, 1] = Lib.timeStr();
            //Model mod = getModel(name);
            //bool found = false;
            //for (int i = 4; i < jrn.Body.iEOL(); i++)
            //{
            //    if ((string)jrn.Body[i, Decl.MODEL_NAME] == name)
            //    {
            //        found = true;
            //        modJrnLine(jrn, i, name, mod.dir, mod.Made, mod.MD5);
            //        break;
            //    }
            //}
            //if( !found ) modJrnLine(jrn, name, mod.dir, mod.Made, mod.MD5);
            int i = 4;
            foreach (var m in Models)
            {
                string t = Lib.timeStr(m.date);
                jrn.Body.AddRow(new object[] { t, m.name, m.dir, m.Made, m.MD5 });
            }
            jrn.isChanged = true;
            Docs.saveDoc(jrn);
            Log.exit();
            return getModel(name);
        }
        /// <summary>
        /// UpdateFrTekla() - обновление модели из данных из C# в файловую систему
        /// </summary>
        /// <journal>13.2.2016<\journal>
        static List<TS.AttSet> Elements = new List<TS.AttSet>();
        public static void UpdateFrTekla()
        {
            Log.set(@"UpdateFrTekla()");
            Elements = TS.Read();
            new Log(@"Модель = " + TS.ModInfo.ModelName + "\t" + Elements.Count + " компонентов.");
            string MD5 = TS.ModAtrMD5();
            Model mod = getModel(TS.ModInfo.ModelName);
            List<Mtch.Rule> modRules = mod.Rules;

            // mod==null - значит это новая модель. 
            if (mod == null || !FileOp.isFileExist(mod.dir + "\\" + mod.name) 
                            || TS.MyName != mod.Made || TS.ModelMD5 != mod.MD5)
            {
                new Log("=== Это новая или измененная модель. Запишем ее в файл. ===");
                Models.Remove(mod);
                Models.Add(new Model(TS.ModInfo.ModelName, TS.ModInfo.ModelPath, TS.MyName, TS.ModelMD5, modRules));
                mod = getModel(TS.ModInfo.ModelName);
                Docs raw = Docs.getDoc(Decl.SH2_RAW);

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
                Docs modInfo = Docs.getDoc(Decl.SH1_MODELINFO);
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
            }
            else new Log("------- Эта модель уже есть в TSmatch. Ничего не записываем --------");
            Elements.Clear();
            Log.exit();
        } // end update

        public struct Group
        {
            public string mat, prf;
            public double lng, wgt, vol;

            public Group(string m, string p, double l, double w, double v) : this()
            { mat = Lib.ToLat(m); prf = Lib.ToLat(p); lng = l; wgt = w; vol = v; }
        }
        public static List<Group> Groups = new List<Group>();
        public static List<Group> getGroups()
        {
            string mat = "", prf = "";
            double lng = 0.0, wgt = 0.0, vol = 0.0;
            foreach (var v in Elements)
            {
                if (v.lng == 0.0) continue;
                if (mat == v.mat && prf == v.prf)
                {
                    lng += v.lng;
                    wgt += v.weight/1000;
                    vol += v.volume/1000/1000/1000;
                }
                else
                {
                    if (lng > 0.0) Groups.Add(new Group(mat, prf, lng, wgt, vol));
                    mat = v.mat; prf = v.prf; lng = v.lng; wgt = v.weight/1000;
                    vol = v.volume/1000/1000/1000;
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
