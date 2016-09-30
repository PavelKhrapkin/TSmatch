/*----------------------------------------------------------------------------
 * Suppliers - componets supplier organisations
 * 
 *  27.4.2016  Pavel Khrapkin
 *
 *--- History ---
 * 27.4.2016 - Remove List<string> doc_names from the Supplier class
 * ---------------------------------------------------------------------------
 *      METHODS:
 * Start()  - Get Supplier Listr from TSmatch.xlsx/Suppliers
 * -------------- My Report and Debugging -----------
 * SupplReport()    - Suppliers list output
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Decl = TSmatch.Declaration.Declaration;
using Msg = TSmatch.Message.Message;
using Log = match.Lib.Log;
using Lib = match.Lib.MatchLib;
using Docs = TSmatch.Document.Document;
using Cmp = TSmatch.Components.Component;
using CmpSet = TSmatch.Components.CompSet;

namespace TSmatch.Suppliers
{
    /// <summary>
    /// Suppliers - class of Component' Suppliers. The name of Suppler should be Unique
    /// </summary>
    public class Supplier : IComparable<Supplier>
    {
        private static List<Supplier> Suppliers = new List<Supplier>();

        public DateTime date;   // Last Update Supplier' Date
        public string name;
        public string Url;
        public string City;
        public string index;
        public string street;
        public string country;
        public string telephone;
        public string contact;
        public List<CmpSet> CompSets = new List<CmpSet>();

        /// <summary>
        /// Supplier Constructor
        /// </summary>
        /// <param name="date">last updated</param>
        /// <param name="name">Supplier name</param>
        /// <param name="url">hyperlink - Web page of the Suppliers </param>
        /// <param name="city">city to deliver supplies from</param>
        /// <param name="street">street address</param>
        /// <param name="index">post intex of the Supplier</param>
        /// <param name="tel">Telephone of the Supplier</param>
        /// <param name="List<CompSet> CompSets">collection of CompSet related to the Supplier</param>
        public Supplier(DateTime date, string name, string url, string city, string street, string index, string tel, List<CmpSet> CompSets)
        {
            this.date = date;
            this.name = name;
            this.Url = url;
            this.City = city;
            this.index = index;
            this.street = street;
            this.telephone = tel;
            this.CompSets = CompSets;
        }
        public Supplier(int n)
        {       // get data from TSmatch.xlsx/Suppliers
            Docs doc = Docs.getDoc(Decl.SUPPLIERS);
            this.date = Lib.getDateTime(doc.Body[n, Decl.SUPL_DATE]);
            this.name = (string)doc.Body[n, Decl.SUPL_NAME];
            this.Url = (string)doc.Body[n, Decl.SUPL_URL];
            this.City = (string)doc.Body[n, Decl.SUPL_CITY];
            this.street = (string)doc.Body[n, Decl.SUPL_STREET];
            this.index = (string)doc.Body[n, Decl.SUPL_INDEX];
            this.telephone = (string)doc.Body[n, Decl.SUPL_TEL];
                // get doc_names list from TOC
            Docs toc = Docs.getDoc(Decl.DOC_TOC);
            for(int i= toc.i0; i < toc.il; i++)
            {
                string str = toc.Body.Strng(i, Decl.DOC_SUPPLIER);
                if (name != str) continue;
                string cs_name = toc.Body.Strng(i, Decl.DOC_SHEET);
                string doc_name = toc.Body.Strng(i, Decl.DOC_NAME);
                CmpSet cs = CmpSet.setCompSet(cs_name, doc_name, this);
                this.CompSets.Add(cs);
            }
        }
        public Supplier(string _name)
        {
            this.name = _name;
            Supplier supl = Suppliers.Find(x => x.name == _name);
            if (supl == null)
            {   // new Supplier
                Msg.F("ERR __.UNKNOWN SUPPLIER", _name);
            }
            else
            {   // existing Supplier
                date      = supl.date;
                Url       = supl.Url;
                City      = supl.City;
                index     = supl.index;
                street    = supl.street;
                telephone = supl.telephone;
                CompSets  = supl.CompSets;
            }
        }
        /// <summary>
        /// CompareTo(Supplier) implements comparision of "this" with the supplier as a parametr. 
        ///     It is used to Sort Suppliers by City and level of readiness to handle in TSmatch
        /// </summary>
        /// <param name="supl"></param>
        /// <returns></returns>
        public int CompareTo(Supplier supl)
        {
            int result = this.City.CompareTo(supl.City);
            if (result == 0)
            {
                result = -this.CompSets.Count.CompareTo(supl.CompSets.Count);
            }
            return result;
        }
        /// <summary>
        /// Start() - get Supplier List from TSmatch.xlsx/Suppliers
        /// </summary>
        /// <description>
        /// 1) read List<Supplier> Suppliers from Excel Document
        /// 2) output this list with the appropriate Language Form  --!! NOT READY YET
        /// </description>
        /// <history>25.3.2016
        /// 2.4.16 fill Supliers list only if CompSets.Count > 0
        /// </history>
        public static List<Supplier> Start()
        {
            Log.set("Supplier.Start");
            Docs doc = Docs.getDoc(Decl.SUPPLIERS);
            Suppliers.Clear();
            for (int i = doc.i0; i <= doc.il; i++)
            {
                int nPriceLists = doc.Body.Int(i, Decl.SUPL_LISTCOUNT);
                if (nPriceLists <= 0) continue;
                Suppliers.Add(new Supplier(i));
            }
            Suppliers.Sort();
            //////////////-- check Suppliers/CompSets list with TOC -- Suppliers HealthCheck
            //////////////.. Initiate CompSet collections, however, not load them yet
            ////////////Docs toc = Docs.getDoc(Decl.DOC_TOC);
            ////////////for (int i = toc.i0; i <= toc.il; i++)
            ////////////{
            ////////////    if (toc.Body.Strng(i, Decl.DOC_DIR) != Decl.TEMPL_COMP) continue;
            ////////////    string supl = toc.Body.Strng(i, Decl.DOC_SUPPLIER);
            ////////////    string cs_name = toc.Body.Strng(i, Decl.DOC_SHEET);
            ////////////    Supplier sss = Suppliers.Find(x => x.name == supl);
            ////////////    if (sss == null)
            ////////////        Msg.F("Err Supplier.Start_ is inTOC not in Suppliers", supl, i, toc.Body.Strng(i, Decl.DOC_NAME));
            ////////////    else
            ////////////        sss.CompSets.Add(new CmpSet(cs_name, sss));
            ////////////}
            //(ToDo)-----------------------------------
            // тут можно вставить формирование горизонтальной цепочки ячеек - прайс-листов в Suppliers
            //NB1: можно формировать этот список не a TSmatch.xlsx/Supplers, а в С# и только проверять значение в листе Suppliers

//            Suppliers.Sort();
            //!!! сюда же позднее напишем запись списка Поставщиков в отсортированном виде обратно в Sheet Suppliers
            Log.exit();
            return Suppliers;
        }
        /// <summary>
        /// getSupplier(string name) - get supplier data from the list of supplers by the name
        /// </summary>
        /// <param name="name">name of the supplier to find</param>
        /// <returns>found supplier of null</returns>
        /// <history>27.3.2016</history>
        internal static Supplier getSupplier(string name) { return Suppliers.Find(x => x.name == name); }
        /// <summary>
        /// SupplReport() - Debugging report: output list of Supplier companies in TSmatch.xlsx/Suppliers
        /// </summary>
        public static void SupplReport()
        {
            Docs doc = Docs.getDoc("SupplReport");
            doc.Reset("Now");
            foreach (var s in Suppliers)
            {
                doc.wrDoc(1, s.date, s.name, s.Url, s.City, s.index, s.street, s.telephone, s.CompSets.Count);
                foreach (var cs in s.CompSets) 
                {
                    cs.getCompSet();
                    //////CmpSet.getCompSet(cs.name, s);
                    //////Docs w = Docs.getDoc(cs.doc.name);
                    //////cs.doc = w;
                    Docs w = cs.doc;
                    string nm = w.Wb.Name + "/" + w.Sheet.Name;
                    doc.wrDoc(2, w.name, nm, w.i0, w.il, w.LoadDescription);
                }
                foreach (var cs in s.CompSets) cs.doc.Close();
            }
            doc.saveDoc();
            doc.Close();
        }
    } // end class Supplier+		doc	null	TSmatch.Document.Document

} // end namespace Suppliers
