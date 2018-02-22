/* ----------------------------------------------------------------
 * Suppliers - List of all know Suppliers in Suppliers.AllSupliers
 * 
 * 22.02.2018 Pavel Khrapkin
 * --- History: ---
 *  5.02.18 - transfeter from Exsercizes/ReadPriceList
 *  6.02.18 - get & fill list of Suppliers not filled with pricelist yet
 * 11.02.18 - parameters for Suppliers.xml in SuppliersInit.xml file
 * 16.02.18 - DeDup for XML objects
 *
 *---- ToDo ---
 * 2017.12.26 WPF пополнение и редактирование - позже. Пока 
 *          заполняю XML из UT один за одругим поставщиком
 *  --- Unit Testing: 9.02.2018        
 *  UT_WrXML, UT_RdXML  OK
 !  UT_GetSuppliersList, UT_SetSuppliersList - закомменторованы
 * --- Methods: ---
 * Init(boot)   - initial setup from SuppliersInit.xml & Suppliers.xml
 * WrInitXML()  - write SuppliersInit.xml 
 * WrXML()      - write AllSuppliers to Suppliers.xml
 * RdXML()      - read Suppliers.xml to List<>AllSuppliers
 * FillSuppliersList(List<supl>) - fill Suppliers List with prices from Excel
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PriceMatch
{
    public class Suppliers
    {
        const string file = "Suppliers.xml";
        const string initFile = "SuppliersInit.xml";
        private Boot boot;
        private string filepath, initFilePath;
        public List<Supplier> AllSuppliers = new List<Supplier>();

        public Suppliers() { }

        public void Init(Boot boot)
        {
            this.boot = boot;
            filepath = Path.Combine(boot.DebugDir, "Prices", file);
            initFilePath = Path.Combine(boot.ExcelDir, initFile);
            //if (!boot.fo.IsFileExist(initFilePath))
            //{
            //    //             if(boot.ssInit == null) boot.ssInit = new List<SuppliersInit>()
            //    WrInitXML(boot.ssInit);
            //}
            boot.ssInit = boot.Lib.RdXML<List<SupplierInit>>(initFilePath) ??
                throw new FileLoadException("Bad \"" + initFile + "\"");
            string path = Path.Combine(boot.DebugDir, "Prices", file);
            RdXML();
            boot.Suppliers.AllSuppliers = AllSuppliers;
        }

        public List<Supplier> DeDupSupl()
        {
            var result = new List<Supplier>();
            var v = boot.Suppliers.AllSuppliers.GroupBy(x => x.name);
            foreach (var gr in v)
            {
                Supplier x = gr.First();
                result.Add(x);
            }
            return result;
        }

        public void DeDup()
        {
            var result = new List<Supplier>();
            var result_I = new List<SupplierInit>();
            var v = boot.Suppliers.AllSuppliers.GroupBy(x => x.name);
            foreach (var gr in v)
            {
                Supplier supl = gr.First();
                var p = supl.productSets.GroupBy(x => x.name);
                var pss = new List<ProductSet>();
                foreach (var pgr in p) pss.Add(pgr.First());
                supl.productSets = pss;
                result.Add(supl);

                SupplierInit supl_I = boot.ssInit.Find(x => x.name == supl.name);
                if (supl_I == null) continue;
                var pi = supl_I.pssInit.GroupBy(x => x.name);
                var psI = new List<psInit>();
                foreach (var pigr in pi) psI.Add(pigr.First());
                supl_I.pssInit = psI;
                result_I.Add(supl_I);
            }
            boot.Suppliers.AllSuppliers = result;
            boot.ssInit = result_I;
        }

        public void WrInitXML(List<SupplierInit> suplLst) =>
            boot.Lib.WrXML(initFilePath, suplLst);

        //public List<Supplier> RdInitXML() => 
        //    boot.Lib.RdXML<List<Supplier>>(initFilepath);

        public void WrXML() => boot.Lib.WrXML(filepath, this);

        public Suppliers RdXML()
        {
            AllSuppliers = boot.Lib.RdXML<Suppliers>(filepath).AllSuppliers
                ?? throw new FileLoadException("No Suppliers.xml");
            return this;
        }

        public void Update()
        {
            var ssInit = boot.Lib.RdXML<List<SupplierInit>>(initFilePath);
            // обработка SupplierInit, обновление Supplier.xml 
            boot.ssInit = ssInit;
            boot.Lib.WrXML(initFilePath, ssInit);
        }

        public List<Supplier> Update(List<Supplier> suplLst)
        {
            var result = new List<Supplier>();
            foreach (var supl in suplLst)
            {
                Supplier v = result.Find(x => x.name == supl.name);
                //18/2                if (v == null) result.Add(supl.Update());
            }
            return result;
        }
        /// <summary>
        /// GetSupplierList() - get list all Suppliers without with prices yet
        /// </summary>
        /// <returns></returns>
        public List<Supplier> GetSuppliersList()
        {
            var suplDic = new Dictionary<string, Supplier>();
            //////////////////////ResourceManager mgr = Properties.Suppliers.ResourceManager;
            //////////////////////CultureInfo culture = CultureInfo.GetCultureInfo("RU");
            //////////////////////ResourceSet set = mgr.GetResourceSet(culture, true, true);
            //////////////////////Supplier supl = new Supplier(boot);
            //////////////////////GoodSet gs = new GoodSet();
            //////////////////////foreach (System.Collections.DictionaryEntry o in set)
            //9/2 chng to XLS////////////////////{
            //////////////////////    string oKey = o.Key as string;
            //////////////////////    string[] keyParts = oKey.Split('_');
            //////////////////////    string suplName = keyParts[0];
            //////////////////////    if (suplDic.ContainsKey(suplName)) supl = suplDic[suplName];
            //////////////////////    else suplDic.Add(suplName, supl);
            //////////////////////    if (keyParts[1] == "") supl = supl.Fill(keyParts[2], o.Value as string);
            //////////////////////    else
            //////////////////////    {
            //////////////////////        string gsName = keyParts[2];

            //////////////////////    }
            //////////////////////}
            return suplDic.Values.ToList();
        }

        private void getSuppliersProperties()
        {
            //////////////////ResourceManager mgr = Properties.Suppliers.ResourceManager;
            //////////////////CultureInfo culture = CultureInfo.GetCultureInfo("RU");
            // 9/2 chng to XLS ////////////////ResourceSet set = mgr.GetResourceSet(culture, true, true);
            //////////////////foreach (var o in set)
            //////////////////{
            //////////////////    throw new NotImplementedException();
            //////////////////}
        }

        private Supplier getSupl()
        {
            var supl = new Supplier();
            throw new NotImplementedException();
            return supl;
        }

        private bool nextSupl()
        {
            throw new NotImplementedException();
        }

        public List<Supplier> GetSuppliersList(List<Supplier> LstSupl)
        {
            var result = new List<Supplier>();
            throw new NotFiniteNumberException();
            return result;
        }
    } // end Suppliers class
} // end namespace