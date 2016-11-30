/*----------------------------------------------------------------------------
 * CompSet -- Set of Components got from the Supplier' price-list
 * 
 * 30.11.2016  P.Khrapkin
 *
 *--- history ---
 * 30.11.2016 made from previous edition of module Components
 * ---------------------------------------------------------------------------
 *      Methods:
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

using System;
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
using TSmatch.Suppliers;

namespace TSmatch.CompSet
{
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
        public List<Component.Component> Components = new List<Component.Component>();
        public List<string> mats = new List<string>();  // перечень материалов, применяемых в Components
        public Supl Supplier;               // организация - поставщик сортамента
        public Docs doc;                    // Документ, содержащий набор компонентов, прайс-лист поставщика 
        private string doc_cs_name;

        public CompSet(string _name, List<Component.Component> _comps, List<string> _mats, Supl _supl, Docs _doc)
        {
            this.name       = _name;
            this.Components = _comps;
            this.mats       = _mats;
            this.Supplier   = _supl;
            this.doc        = _doc;
        }
        public CompSet(string _name, Supl _supl) : this(_name, null, null, _supl, null) { }
        public CompSet(string _name, string _supplier_name) : this(_name, Supl.getSupplier(_supplier_name)) { }
        public CompSet(string _name, Supl _supl, string doc_cs_name) : this(_name, _supl)
        {
            this.doc_cs_name = doc_cs_name;
        }

        //////////////////public static CompSet setCompSet(string cs_name,  Supl supl, string doc_name)
        //////////////////{
        ///// 30/11 //////    CompSet cs = new CompSet(cs_name, supl);
        //////////////////    cs.doc = Docs.getDoc(doc_name, load: false);
        //////////////////    return cs;
        //////////////////}
        /// <summary>
        /// getCompSet() - fill CompSet from price-list. With all overloader getCompSet() method,
        ///                only one without parameters loaded price-list. Others set cs.name only.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="supl_name"></param>
        /// <returns></returns>
        /// <history>14.4.2016
        /// 29.11.2016 - re-worked.
        /// </history>
        ////////////////public static CompSet getCompSet(string name, string supl_name)
        ////////////////{ return getCompSet(name, Supl.getSupplier(supl_name)); }
        public static CompSet getCompSet(string cs_name, Supl supplier)
        {
            Docs toc = Docs.getDoc(Decl.DOC_TOC);
            string doc_cs_name = "";
            for (int i = toc.i0; i <= toc.il; i++)
            {
                if (toc.Body.Strng(i, Decl.DOC_SUPPLIER) != supplier.name) continue;
                if (toc.Body.Strng(i, Decl.DOC_SHEET) != cs_name) continue;
                doc_cs_name = toc.Body.Strng(i, Decl.DOC_NAME);
                break;
            }
            CompSet cs = new CompSet(cs_name, supplier, doc_cs_name);
            return cs;
            Docs doc_cs = Docs.getDoc(doc_cs_name);
                throw new NotImplementedException();




            //////////CompSet cs = supplier.CompSets.Find(x => x.name == name);
            //////////if (cs == null) Msg.F("Err getCompSet No CompSet", supplier.name, name);
            //////////Docs toc = Docs.getDoc(Decl.DOC_TOC);
            //////////string doc_cs_name = ""; // get doc_cs_name from TOC, however, getDoc later on
            //////////for (int i = toc.i0; i <= toc.il; i++)
            //////////{
            //////////    string supl = toc.Body.Strng(i, Decl.DOC_SUPPLIER);
            //////////    if (supl != supplier.name) continue;
            //////////    string cs_name = toc.Body.Strng(i, Decl.DOC_SHEET);
            //////////    if (cs_name != name) continue;
            //////////    doc_cs_name = toc.Body.Strng(i, Decl.DOC_NAME);
            //////////    break;
            //////////}
            //////////cs.doc = new Docs(doc_cs_name);    // don't load CompSet Document yet, setup the name only, and real getDoc when necessary
            //////////return cs;
        }
        public CompSet getCompSet()
        {
            if (this.Components == null)
            {
                this.doc = Docs.getDoc(this.doc.name);
//30/11                this.Components = Component.setComp(doc);
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
} // end namespace CompSet
