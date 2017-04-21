/*----------------------------------------------------------------------------
 * Suppliers - componets supplier organisations
 * 
 *  16.4.2017  Pavel Khrapkin
 *
 *--- History ---
 * 27.4.2016 - Remove List<string> doc_names from the Supplier class
 * 29.11.2016 - get Supplier directly from TSmach.xlsx/Supplier, not from Suppliers List
 * 16.04.2017 - getSupplierStr() method add for Windows Form use
 * ---------------------------------------------------------------------------
 *      METHODS:
 * getSupplier(name)    - create Suplier(name), get data from TSmatch.xlsx/Supplier    
 * getSupplierStr()		- return Supplier data in string to be used with Form 
 * 
 * -------------- My Report and Debugging -----------
 * SupplReport()    - Suppliers full list output
 */
using System;
using System.Collections.Generic;

using Decl = TSmatch.Declaration.Declaration;
using Msg = TSmatch.Message.Message;
using Lib = match.Lib.MatchLib;
using Docs = TSmatch.Document.Document;
using CmpSet = TSmatch.CompSet.CompSet;

namespace TSmatch.Suppliers
{
    /// <summary>
    /// Suppliers - class of Component' Suppliers. The name of Suppler should be Unique
    /// </summary>
    public class Supplier : IComparable<Supplier>
    {
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
        /// <param name="city">city to delivery supplies from</param>
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
        public Supplier(int n) { getSupplier(n); }
 
        public Supplier(string _name)
        {
            Docs docSupl = Docs.getDoc(Decl.SUPPLIERS);
            for (int i = docSupl.i0; i <= docSupl.il; i++)
            {
                string suplName = docSupl.Body.Strng(i, Decl.SUPL_NAME);
                if (suplName != _name) continue;
                getSupplier(i);
                break;
            }
            if (this.name == "") Msg.F("No Supplier(" + _name + ")");
        }

        private void getSupplier(int n)
        {
            Docs docSupl = Docs.getDoc(Decl.SUPPLIERS);
            date = Lib.getDateTime(docSupl.Body[n, Decl.SUPL_DATE]);
            name = (string)docSupl.Body[n, Decl.SUPL_NAME];
            Url = (string)docSupl.Body[n, Decl.SUPL_URL];
            City = (string)docSupl.Body[n, Decl.SUPL_CITY];
            street = (string)docSupl.Body[n, Decl.SUPL_STREET];
            index = (string)docSupl.Body[n, Decl.SUPL_INDEX];
            telephone = (string)docSupl.Body[n, Decl.SUPL_TEL];
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
        /// getSupplier(string name) - get supplier data from the list of supplers in TSmatch.xlsx/Suppliuers by the name
        /// </summary>
        /// <param name="name">name of the supplier to find</param>
        /// <returns>found supplier of null</returns>
        /// <history>27.3.2016
        /// 29.11.2016 - re-written. return data directly from TSmatch/xlsx/Suppliers
        /// </history>
        internal static Supplier getSupplier(string name)
        {
            return new Supplier(name);
        }
        /// <summary>
        /// SupplReport() - Debugging report: output list of Supplier companies in TSmatch.xlsx/Suppliers
        /// </summary>
        public static void SupplReport()
        {
            List<Supplier> Suppliers = new List<Supplier>();
            Docs docSupl = Docs.getDoc(Decl.SUPPLIERS);
            for (int i = docSupl.i0; i <= docSupl.il; i++)
            {
                Suppliers.Add(new Supplier(i));
            }
            Docs doc = Docs.getDoc("SupplReport");
            doc.Reset("Now");
            foreach (var s in Suppliers)
            {
                doc.wrDoc(1, s.date, s.name, s.Url, s.City, s.index, s.street, s.telephone, s.CompSets.Count);
                foreach (var cs in s.CompSets)
                {
                    //11.1.17                    cs.getCompSet();
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
        public string getSupplierStr()
        {
            string str = name + "\n";
            if (!string.IsNullOrEmpty(index)) str += index + ", ";
            str += City + ", ";
            if (str.Length > 20) str += "\n";
            str += street;
            str += "\n Web: " + Url + "\n тел." + telephone;
            return str;
        }
    } // end class Supplier
} // end namespace Suppliers