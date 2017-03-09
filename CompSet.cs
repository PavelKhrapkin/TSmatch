/*----------------------------------------------------------------------------
 * CompSet -- Set of Components got from the Supplier' price-list
 * 
 * 31.12.2016  P.Khrapkin
 *
 * -- ToDo
 * 31.12.16 использовать Rule.FPs и LoadDescription при конструировании CompSet
 * --- history ---
 * 30.11.2016 made from previous edition of module Components
 * 31.12.2016 Rule.FPs accounted 
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
using log4net;

using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using TS = TSmatch.Tekla.Tekla;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Mtch;
using Supl = TSmatch.Suppliers.Supplier;
using TSmatch.Suppliers;
using TSmatch.Component;
using TSmatch.ElmAttSet;
using TSmatch.Rule;
using FP = TSmatch.FingerPrint.FingerPrint;

namespace TSmatch.CompSet
{
    public class CompSet   
    {
        public static readonly ILog log = LogManager.GetLogger("CompSet");

        internal string name;       // название сортамента, например, "Уголок"
        internal List<Component.Component> Components = new List<Component.Component>();
        internal Supl Supplier;     // организация - поставщик сортамента
        internal Docs doc;          // Документ, содержащий набор компонентов, прайс-лист поставщика 
        internal List<FP> csFPs = new List<FP>();    // parsed LoadDescriptor of price list Document

        ////////////////public CompSet(string _name, List<Component.Component> _comps, Supl _supl, Docs _doc, List<FP> _csFPs)
        ////////////////{
        ////////////////    this.name       = _name;
        ////////////////    this.Components = _comps;
        ////////////////    this.Supplier   = _supl;
        ////////////////    this.doc        = _doc;
        ////////////////    this.csFPs      = _csFPs;
        ////////////////}
        public CompSet(string _name, Supl _supl, Rule.Rule _rule) //: this {_name, null, _supl, null, null }
        {
            name = _name;
            Supplier = _supl;
            //-- get cs doc from TOC by cs_name and Supplier in Rule
            Docs toc = Docs.getDoc();
            for (int i = toc.i0; i <= toc.il; i++)
            {
                string suplName = toc.Body.Strng(i, Decl.DOC_SUPPLIER);
                string csSheet = toc.Body.Strng(i, Decl.DOC_SHEET);
                if (suplName != Supplier.name || csSheet != name) continue;
                string docName = toc.Body.Strng(i, Decl.DOC_NAME);
                doc = Docs.getDoc(docName);
                break;
            }
            csFPs = _rule.Parser(FP.type.CompSet, doc.LoadDescription);
            for (int i=doc.i0; i < doc.il; i++)
            {
                Component.Component comp = new Component.Component(doc, i, csFPs);
                Components.Add(comp);
            }
        }

        ////////internal Component.Component CompMatch(ElmAttSet.Group gr)
        ////////{
        ////////    foreach (var comp in Components)
        ////////    {
        ////////    }
        ////////    throw new NotImplementedException();
        ////////}

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
//////////////        public static CompSet getCompSet(string cs_name, Supl supplier)
//////////////        {
//////////////            Docs toc = Docs.getDoc(Decl.DOC_TOC);
//////////////            string doc_cs_name = "";
//////////////            for (int i = toc.i0; i <= toc.il; i++)
//////////////            {
//////////////                if (toc.Body.Strng(i, Decl.DOC_SUPPLIER) != supplier.name) continue;
//////////////                if (toc.Body.Strng(i, Decl.DOC_SHEET) != cs_name) continue;
//////////////                doc_cs_name = toc.Body.Strng(i, Decl.DOC_NAME);
//////////////                break;
//////////////            }
//////////////            CompSet cs = new CompSet(cs_name, supplier, doc_cs_name);
//////////////            return cs;
//////////////        }
//////////////        public CompSet getCompSet()
//////////////        {
//////////////            if (this.Components == null)
//////////////            {
//////////////                this.doc = Docs.getDoc(this.doc.name);
////////////////30/11                this.Components = Component.setComp(doc);
//////////////                getMat();
//////////////            }
//////////////            return this;
//////////////        }
//////////////        /// <summary>
//////////////        /// getMat() - setup mats - List of materials used in Components
//////////////        /// </summary>
//////////////        /// <history>14.4.2016</history>
//////////////        /// <description>
//////////////        /// mats taken from Component.description
//////////////        /// </description>
//////////////        public void getMat()
//////////////        {
//////////////            Log.set("CompSet.getMat()");
//////////////            foreach (var cs in Components)
//////////////            {
//////////////                string s = cs.description;
//////////////            }
//////////////            Log.exit();
//////////////        }
    } // end class CompSet
} // end namespace CompSet
