/* -------------------------------------------------------------------
 * Supplier - class support Goods suppliers 22.02.2018 Pavel Khrapkin
 * 
 * --- History: ---
 * 2015-2017 development in TSmatch
 *  8.02.2018 Fill Supplier fields
 *  9.02.2018 Refactoring: rename Good -> Product, GoodSet -> ProductSet
 * 17.02.2018 Remove SupplierInit.Init to fix warning,
 *            Add WrXLMfrExcel reading argument from SuppliersInit.xml
 * 18.02.2018 Remove Init(Boot), remove Supplier.Update()
 * 19.02.2018 Replace() & Update() methods
 * 
 * UT_Supplier: 
 * ---Methods: ---
 * Init()
 * WrXML()
 * RdXML()
 * Fill(field, value)   - fill Supplier string fields
 * Update()
 * Replace()    - replace Supplier or SupplierInit instance in AllSupliers or ssInit
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace PriceMatch
{
    [Serializable]
    public class Supplier
    {
        protected Boot boot;
        public string name { get; set; }
        public string URL { get; set; }
        public string Country { get; set; }
        public string Index { get; set; }
        public string Tel { get; set; }
        public string city { get; set; }
        public string streetAdr { get; set; }
        public string Note { get; set; }
        public List<ProductSet> productSets = new List<ProductSet>();

        public Supplier() { }
        public Supplier(Boot boot) { this.boot = boot; }

        public void Init(Boot boot) { this.boot = boot; }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Update(string suplName, string psName)
        {
            SupplierInit supl_I = boot.ssInit.Find(x => x.name == suplName) ??
                throw new ArgumentException($"SupplierInit {suplName} not found");
            psInit psi = supl_I.pssInit.Find(x => x.name == psName) ??
                throw new ArgumentException($"psInit {psName} not found for SupplierInit=\"{suplName}\"");
            string LoadDescription = psi.LoadDescriptor;
            Supplier supl = boot.Suppliers.AllSuppliers.Find(x => x.name == suplName) ??
                throw new ArgumentException($"Supplier {suplName} not found");
            ProductSet ps = supl.productSets.Find(x => x.name == suplName) ??
                throw new ArgumentException($"ProductSet=\"{psName}\" not found for Supplier=\"{suplName}\"");
            int indx = supl.productSets.IndexOf(ps);
            string docName = suplName + "_" + psName;
//17/2           boot.Doc.Init(docName, boot.ExcelDir, );
                

            int suplIndex = boot.Suppliers.AllSuppliers.IndexOf(supl);
 //17/2           boot.Suppliers.AllSuppliers[suplIndex].productSets[indx] = newPS;


            throw new NotImplementedException();

        }
        public void WrXMLfrExcel(string pathExcel, string fileExcel, string SheetExcel,
            string LoadDescriptor, string pathXML, string fileXML = "")
        {
            const string EXC = "ExcelDoc";
            if (string.IsNullOrEmpty(fileXML)) fileXML = Path.ChangeExtension(fileXML, "xml");
            if (!boot.Doc.Init(EXC, pathExcel, fileExcel, SheetExcel, FirstRow: 8))
                throw new Exception("No Excel price-list");
            var ps = new ProductSet();
            //25/12           ps.Init();
            var doc = boot.Doc.GetDoc(EXC);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                var p = new Product();
                p.name = doc.Str(i, 1);
                p.price = doc.Dec(i, 4);
                ps.Products.Add(p);
            }
        }

        /// <summary>
        /// Fill put in property "field" value v. Argument could be shorter,
        ///     than full property name in C#
        /// </summary>
        /// <param name="field"></param>
        /// <param name="v"></param>
        /// <returns>Supplier after change</returns>
        public Supplier Fill(string field, string v)
        {
            field = Lib.ToLat(field.ToLower());
            string[] f = field.Split('_');
            field = f[0];
            foreach (PropertyInfo suplField in GetType().GetProperties())
            {
                int lng = Math.Min(field.Length, suplField.Name.Length);
                string ff = field.Substring(0, lng);
                string fs = suplField.Name.Substring(0, lng);
                if (ff != fs) continue;
                suplField.SetValue(this, v);
                break;
            }
            return this;
        }

        public void Replace()
        {
            if (this is Supplier)
            {
                var AllSuppliers = boot.Suppliers.AllSuppliers;
                int indx = AllSuppliers.FindIndex(x => x.name == name);
                AllSuppliers[indx] = this;
            }
            else
            {
                int indx = boot.ssInit.FindIndex(x => x.name == name);
                boot.ssInit[indx] = this as SupplierInit;
            }
        }

        ////////////////public Supplier Update()
        ////////////////{
        ////////////////    if (name == null) throw new NullReferenceException();
        ////////////////    Supplier suplOld = boot.Suppliers.AllSuppliers.Find(x => x.name == name);
        ////////////////    if (suplOld == null)
        ////////////////    { // New Supplier, no old one exist
        ////////////////        suplOld.Init(boot);
        ////////////////        return suplOld;
        // 18/2 ////////    }
        ////////////////    if (productSets == null || productSets.Count == 0)
        ////////////////        throw new ArgumentNullException("Update Supplier has no ProductSet");
        ////////////////    foreach (var ps in productSets)
        ////////////////    {
        ////////////////        throw new NotFiniteNumberException();
        ////////////////        ps.Update(this, "");
        ////////////////    }
        ////////////////    //           productSets = pss;
        ////////////////    return this;
        ////////////////}

    } // end class Supplier

    public class SupplierInit : Supplier
    {
        public List<psInit> pssInit = new List<psInit>();
        private Boot boot;
        private List<SupplierInit> ssInit;

        public SupplierInit() { }
        public SupplierInit(Boot boot) { this.boot = boot; }
        public SupplierInit Copy(Supplier supl)
        {
            name = supl.name;
            return this;  
        }
    } // end SupplierInit class
} // end namwspace 