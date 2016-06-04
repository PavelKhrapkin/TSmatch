/*----------------------------------------------------------------------------
 * Components -- работа с документами - прайс-листами поставщиков компонентов
 * 
 * 14.4.2016  П.Храпкин
 *
 *--- журнал ---
 * 28.2.2016 выделено из модуля Matcher
 *  5.3.2016 setComp(doc) - инициализация данных для базы компонентов из doc
 *  3.4.2016 setComp(doc_name) overload; getDoc() and Close price-list
 * 14.4.2016 Component.mat; CompSet.getMat()
 * ---------------------------------------------------------------------------
 *      МЕТОДЫ:
 * getCompSet(name, Supplier) - getCompSet by  its name in Supplier' list
 * setComp(doc) - инициальзация данных для базы компонентов в doc
 * getComp(doc) - загружает Excel файл - список комплектующих от поставщика
 * UddateFrInternet() - обновляет документ комплектующих из Интернет  
 * ----- class CompSet
 *      МЕТОДЫ:
 * getMat ()    - fill mat ftom CompSet.Components and Suplier.TOC
 * 
 *    --- Вспомогательные методы - подпрограммы ---
 * UpgradeFrExcel(doc, strToDo) - обновление Документа по правилу strToDo
 */

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using TS = TSmatch.Tekla.Tekla;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Matcher;
using Supl = TSmatch.Suppliers.Supplier;

namespace TSmatch.Components
{
    public class Component
    {
        public readonly string description; // строка вида "Угoлoк cтaльнoй paвнoпoл. 25 x 4 cт3cп/пc5"
        public readonly string mat;         // материал компонента. Извлекается из description или из прайс-листа
        public readonly double length;      // длина заготовки в м
        public readonly double weight_m;    // вес погонного метра заготовки -- пока тут пусто
        public readonly double? price;      // цена за 1 тонну в руб

        public Component(string _description, string _mat, double _length, double _weight, double? _price)
        {
            description = Lib.ToLat(_description);
            mat = _mat;
            length = _length;
            weight_m = _weight;
            price = _price;
        }
        /// <summary>
        /// setComp(doc) - fill price list of Components from doc
        /// setComp(doc_name) - overload
        /// </summary>
        /// <param name="doc">price-list</param>
        /// <returns>List of Components</returns>
        /// <history>26.3.2016
        ///  3.4.2016 - setComp(doc_name) overload
        ///  8.4.2016 - remove unnecteesary fields - bug fix
        /// 14.4.2016 - field mat = Material fill 
        ///  </history>
        public static List<Component> setComp(string doc_name)
        { return setComp(Docs.getDoc(doc_name)); }
        public static List<Component> setComp(Docs doc)
        {
            Log.set("setComp(" + doc.name + ")");
            List<int> docCompPars = Mtch.GetPars(doc.LoadDescription);
            //-- заполнение массива комплектующих Comps из прайс-листа металлопроката
            List<Component> Comps = new List<Component>();
            for (int i = doc.i0; i <= doc.il; i++)
            {
                try
                {
                    string descr = doc.Body.Strng(i, docCompPars[0]);
                    double lng = 0;
                    //-- разбор параметров LoadDescription
                    List<int> strPars = Mtch.GetPars(descr);
                    string docDescr = doc.LoadDescription;
                    int parShft = 0;
                    while (docDescr.Contains('/'))
                    {
                        string[] s = doc.LoadDescription.Split('/');
                        List<int> c = Mtch.GetPars(s[0]);
                        int pCol = c[c.Count() - 1];    // колонка - последний параметр до '/'
                        List<int> p = Mtch.GetPars(s[1]);
                        lng = strPars[p[0] - 1];    // длина заготовки = параметр в str; индекс - первое число после '/'
                        docDescr = docDescr.Replace("/", "");
                        parShft++;
                    }
                    if (lng == 0)
                        lng = doc.Body.Int(i, docCompPars[1]) / 1000;    // для lng указана колонка в LoadDescription   
                    double? price = doc.Body.Double(i, docCompPars[2] + parShft);
                    double wgt = 0.0;   //!!! времянка -- пока вес будем брать только из Tekla
                    string mat = "";    //!!! времянка -- материал нужно извлекать из description или описания - еще не написано!
                    Comps.Add(new Component(descr, mat, lng, wgt, price));
                }
                catch { Msg.F("Err in setComp", doc.name); }
            }
            Log.exit();
            return Comps; 
        }
        /// <summary>
        /// getComp(CompSet cs) - загружает Документ - прайс-лист комплектующих
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        /// <history> 21.2.2016
        /// 24.2.2016 - выделил в отдельный модуль Components
        /// 27.2.2016 - оформил внутреннюю структуру Component и встроил в getComp(doc)
        ///             для разбора, где указана длина комплектующего, используем строку
        ///             вида <col>/<№ параметра в str> 
        /// </history>
        public void getComp()
        {
            Log.set("getComp");
//            setComp(this.doc);
            try
            {

            }
            catch
            {

            }


            ////List<int> docCompPars = Mtch.GetPars(doc.LoadDescription);
            //////-- заполнение массива комплектующих Comps
            ////Comps.Clear();
            ////for (int i = doc.i0; i <= doc.il; i++)
            ////{
            ////    string str = doc.Body.Strng(i, docCompPars[0]);
            ////    double lng = 0;
            ////    //-- разбор параметров LoadDescription
            ////    List<int> strPars = Mtch.GetPars(str);
            ////    string docDescr = doc.LoadDescription;
            ////    int parShft = 0;
            ////    while (docDescr.Contains('/'))
            ////    {
            ////        string[] s = doc.LoadDescription.Split('/');
            ////        List<int> c = Mtch.GetPars(s[0]);
            ////        int pCol = c[c.Count() - 1];    // колонка - последний параметр до '/'
            ////        List<int> p = Mtch.GetPars(s[1]);         
            ////        lng = strPars[p[0] - 1];    // длина заготовки = параметр в str; индекс - первое число после '/'
            ////        docDescr = docDescr.Replace("/", "");
            ////        parShft++;
            ////    }
            ////    if (lng == 0)
            ////        lng = doc.Body.Int(i, docCompPars[1])/1000;    // для lng указана колонка в Comp    
            ////    double? price = doc.Body.Double(i, docCompPars[2] + parShft);
            ////    double wgt = 0.0;   //!!! времянка -- пока вес будем брать из Tekla 
            ////    Comps.Add(new Component(str, lng, wgt, price));
            ////}
            Log.exit();
        }
        public static Docs UpgradeFrExcel(Docs doc, string strToDo)
        {
            Log.set("UpgradeFrExcel(" + doc.name + ", " + strToDo + ")");
            if (strToDo != "DelEqPar1") Log.FATAL("не написано!");
//!!            List<string> Comp = getComp(doc);
            //////int i = doc.i0;
            //////foreach (string s in Comp)
            //////{
            //////    string str = Lib.ToLat(s);
            //////    List<int> pars = Mtch.GetPars(s);
            //////    if (pars[0] == pars[1])
            //////    {        
            //////        string toDel = pars[0].ToString() + " x ";
            //////        str = str.Replace(toDel + toDel, toDel); 
            //////    }
            //////    doc.Body[i++, 1] = str;
            //////}
            //////doc.isChanged = true;
            //////Docs.saveDoc(doc);

            //for (int i = doc.i0, iComp = 0; i <= doc.il; i++)
            //{
            //    // doc.Body.Strng(i, 1) = Copm;
            //}
            Log.exit();
            return doc;
        }
    } // end class components
    /// <summary>
    /// CompSet - набор однотипных компонентов одного поставщика, например, прайслист Стальхолдинга "Пластины"                                                          
    /// </summary>
    /// <history>27.3.2016
    /// 14.4.2016 add List<string>mats in class fields and method getMat()
    /// </history>
    public class CompSet   
    {
        static List<CompSet> CompSets = new List<CompSet>();    // список всех наборов компонентов - ..
                                                                //..возможно, с не уникальными названими
        public string name;                 // название сортамента, например, "Уголок"
        public List<Component> Components = new List<Component>();
        public List<string> mats = new List<string>();  // перечень материалов, применяемых в Components
        public Supl Supplier;               // организация - поставщик сортамента
        public Docs doc;                    // Документ, содержащий набор компонентов, прайс-лист поставщика 

        public CompSet(string _name, List<Component> _comps, List<string> _mats, Supl _supl, Docs _doc)
        {
            this.name       = _name;
            this.Components = _comps;
            this.mats       = _mats;
            this.Supplier   = _supl;
            this.doc        = _doc;
        }
        public CompSet(string _name, Supl _supl) : this(_name, null, null, _supl, null) { }
        public CompSet(string _name, string _supplier_name) : this(_name, Supl.getSupplier(_supplier_name)) { }
        public static CompSet setCompSet(string cs_name, string doc_name, Supl supl)
        {
            CompSet cs = new CompSet(cs_name, supl);
            cs.doc = Docs.getDoc(doc_name, load: false);
            return cs;
        }
        /// <summary>
        /// getCompSet() - fill CompSet from price-list. With all overloader getCompSet() method,
        ///                only one without parameters loaded price-list. Others set cs.name only.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="supl_name"></param>
        /// <returns></returns>
        /// <history>14.4.2016</history>
        public static CompSet getCompSet(string name, string supl_name)
        { return getCompSet(name, Supl.getSupplier(supl_name)); }
        public static CompSet getCompSet(string name, Supl supplier)
        {
            CompSet cs = supplier.CompSets.Find(x => x.name == name);
            if (cs == null) Msg.F("Err getCompSet No CompSet", supplier.name, name);
            Docs toc = Docs.getDoc(Decl.DOC_TOC);
            string doc_cs_name = ""; // get doc_cs_name from TOC, however, getDoc later on
            for (int i = toc.i0; i <= toc.il; i++)
            {
                string supl = toc.Body.Strng(i, Decl.DOC_SUPPLIER);
                if (supl != supplier.name) continue;
                string cs_name = toc.Body.Strng(i, Decl.DOC_SHEET);
                if (cs_name != name) continue;
                doc_cs_name = toc.Body.Strng(i, Decl.DOC_NAME);
                break;
            }
            cs.doc = new Docs(doc_cs_name);    // don't load CompSet Document yet, setup the name only, and real getDoc when necessary
            return cs;
        }
        public CompSet getCompSet()
        {
            if (this.Components == null)
            {
                this.doc = Docs.getDoc(this.doc.name);
                this.Components = Component.setComp(doc);
                getMat();
            }
            return this;
        }
        /// <summary>
        /// getMat() - setup mats - List of materials used in Components
        /// </summary>
        /// <history>14.4.2016</history>
        /// <description>
        /// mats taken from Component.description
        /// </description>
        public void getMat()
        {
            Log.set("CompSet.getMat()");
            foreach (var cs in Components)
            {
                string s = cs.description;
            }
            Log.exit();
        }
    } // end class CompSet
} // end namespace
