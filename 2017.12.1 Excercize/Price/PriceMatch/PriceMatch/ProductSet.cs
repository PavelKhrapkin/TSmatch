/* -----------------------------------------------------------------
 * ProductSet - Set of Products, with their Rules, and PriceList
 * 
 * 22.02.2018 Pavel Khrapkin
 * 
 * --- History --------
 * 2018.01.09   - exclude Rules, use List<string>RuleText & SectionTab instead
 * 2018.01.10   - exclude SectionTab from GoodSet fields
 * 2018.01.14   - GetFieldList function
 * 2018.01.22   - load doc from Excel with the length scale
 * 2018.02.06   - transfer to PiceResx
 * 2018.02.08   - Fill(field, v)
 * 2018.02.09   - Refactoring GoodSet => ProductSet
 * 2018.02.12   - ProductSet.Update, ValidUntil property add;
 * 2018.02.13   - class psInit
 * 2018.02.16   - transfer to PriceMatch
 * 2018.02.18   - add FileName and SheetN fields to psInit, chech IfExist..()
 * 2018.02.22   - SuplName field, Update() methods
 * 
 * UT_ProductSet: UT_Boot, UT_Update, UTsetFields, 
 *                UT_Init_CShars, UT_Init_XML, UT_Init_Excel
 * --- Methods: ------
 * Init(boot)   - add boot reference for pablic data access
 * Init(name, doc,[date])   - initiate PS from price-list document in Excel
 * Update()     - update this ProductSet
 * Update(psi)  - update this with psi --- может быть сделать private??
 * Update(SuplName,[psName])
 *
 - protected setFields(), setField  - internal method for Goods initialization
 - protected colLoadDescriptor(Section) -
 * Fill(field, v) - parse field and fill property with name in Field
 * 
 * class psInit - used for SuppliersInit
 * Copy(...)
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Decl = PriceMatch.Declaration;
using Sec = PriceMatch.Section;

namespace PriceMatch
{
    [Serializable]
    public class ProductSet
    {
        public string name { get; set; }
        public DateTime Date;
        public DateTime ValidUntil;
        public List<string> RuleText;
        public List<Product> Products = new List<Product>();
        public string SuplName;
        public string PSid;
        public string psMD5;

        protected Boot boot;
        protected Product product;
        protected Document doc;

        public ProductSet() { }
        public ProductSet(Boot boot) { this.boot = boot; }
        public void Init(Boot boot) { this.boot = boot; }

        public ProductSet Init(string name, Document doc, string date = "")
        {
            this.name = name;
            DateTime.TryParse(date, out this.Date);
            this.doc = doc ?? throw new Exception("No Doc in Excel");
            string ld = doc.LoadDescription;
            var lds = boot.Sect.Parse(ld);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                product = new Product();
                foreach (var sec in lds)
                {
                    if (setFields(i, sec)) continue;
                    if (setFields(i, sec)) continue;
                    setFields(i, sec);
                }
                Products.Add(product);
            }
            RuleText = doc.RuleText;
            psMD5 = MD5gen.MD5HashGenerator.GenerateKey(this);
            return this;
        }

        #region ---- Update area ----
        /// <summary>
        /// Update - read updated pricelist from Excel and put it to AllSuppliers
        /// </summary> 
        public void Update(psInit psi)
        {
            SupplierInit supl_I = boot.ssInit.Find(x => x.name == SuplName);
            int indx = supl_I.pssInit.FindIndex(x => x.name == name);
            if (indx > 0 && indx < supl_I.pssInit.Count)
                supl_I.pssInit[indx] = psi;
            else supl_I.pssInit.Add(psi);
            var doc = boot.Doc;
            string docName = SuplName + " " + name;
            doc.Init(docName, boot.ExcelDir, psi.FileName, psi.SheetN,
                LoadDescription: psi.LoadDescriptor, FirstRow: psi.i0,
                RuleText: RuleText.ToArray());
            doc = doc.GetDoc(docName);
            Init(name, doc);
            var ss = boot.Suppliers.AllSuppliers;
            Supplier supl = ss.Find(x => x.name == SuplName);
            ProductSet ps = supl.productSets.Find(x => x.name == name);
            if (ps == null) ps = this;
            else supl.productSets.Add(this);
        }
        public void Update()
        {
            if (boot == null) throw new Exception("Not Initialized Boot");
            SupplierInit supl_I = boot.ssInit.Find(x => x.name == SuplName);
            if (supl_I == null) throw new Exception("No Supplier to Update");
            psInit psi = supl_I.pssInit.Find(x => x.name == name);
            if (psi == null)
                throw new Exception($"No ProductSet to update in Supplier \"{SuplName}\"");
            Update(psi);
        }
        public void Update(string SuplName, string psName = "")
        {
            var AllSupl = boot.Suppliers.AllSuppliers;
            Supplier supl = AllSupl.Find(x => x.name == SuplName);
            if (supl == null) throw new Exception($"No Supplier=\"{SuplName}\" to update");
            if (psName == "")
            {
                foreach (var prSet in supl.productSets)
                    Update(supl.name, prSet.name);
                return;
            }
            ProductSet ps = supl.productSets.Find(x => x.name == psName);
            if (ps == null)
                throw new Exception($"No ProductSet=\"{psName}\" is Supplier=\"{SuplName}\"");
            ps.Init(boot);
            ps.SuplName = SuplName;
            ps.Update();
        }
        #endregion ---- Update area ----

        #region ---- Set Fields ----
        /// <summary>
        /// setFields - write values in Product Fields from cell [i, col(sec)]
        /// </summary>
        /// <param name="i">column number</param>
        /// <param name="sec">Section of the column</param>
        /// <returns>true, when Field value succesfully set, else - false</returns>
        protected bool setFields(int i, Sec sec)
        {
            int col = colLoadDescriptor(sec);
            if (col < 0) return false;
            if (sec.SType == Decl.STypePRICE)
            {
                product.price = doc.Dec(i, col);
                return true;
            }
            string sv = doc.Str(i, col);
            if (sv == null) return false;
            if (sec.SType == Decl.STypeMAT) product.name = sv;
            else setField(sv, sec.body);
            return true;
        }

        /// <summary>
        /// setField - set product.Field from sv with the scale
        /// </summary>
        /// <param name="sv">string to put it is product.Field. Both decimal '.' and ',' allowed</param>
        /// <param name="secScale">string like "x1000" - sv scale</param>
        protected void setField(string sv, string secScale = "")
        {
            secScale = Lib.ToLat(secScale.ToLower());
            var match = Regex.Match(secScale, @"\d+(x|\*)\d+");
            if (!match.Success || string.IsNullOrEmpty(secScale))
            {
                product.Fields.Add(sv);
                return;
            }
            match = Regex.Match(match.Value, @"\d+");
            match = match.NextMatch();
            double scale = (double)Convert.ToDecimal(match.Value);
            sv = sv.Replace(".", ",");
            double v = 0.0;
            if (Double.TryParse(sv, out v)) v *= scale;
            product.Fields.Add(v);
            // x10I - int = doc.Int(sv) * 10
            // x10D - double = Dec(sv) * 10
            // x10 == x10D
        }

        protected int colLoadDescriptor(Sec sec)
        {
            var match = Regex.Match(sec.body, @"\d+");
            int result = match.Success ? Convert.ToInt32(match.Value) : -1;
            if (result < 1 || result > doc.Body.iEOC()) result = -1;
            return result;
        }

        /// <summary>
        /// Fill(field, v) - parse Field, and set v there
        /// </summary>
        /// <param name="field"></param>
        /// <param name="v"></param>
        /// <returns>GoodSet</returns>
        public ProductSet Fill(string field, string v)
        {
            field = Lib.ToLat(field.ToLower());
            string[] f = field.Split('_');
            field = f[0];
            if (field == "date")
            {
                DateTime.TryParse(v, out Date);
                return this;
            }
            foreach (PropertyInfo gsField in GetType().GetProperties())
            {
                int lng = Math.Min(field.Length, gsField.Name.Length);
                string ff = field.Substring(0, lng);
                string fs = gsField.Name.Substring(0, lng);
                if (ff != fs) continue;
                gsField.SetValue(this, v);
                break;
            }
            return this;
        }
        #endregion ---- Set Fields ----
    } // end GoodSet class

    [Serializable]
    public class psInit : ProductSet
    {
        public string FileName;
        public string SheetN;
        public string LoadDescriptor;
        public int i0;
        public int il;
        protected List<SupplierInit> ssi;
        protected Boot boot;

        public psInit() { }
        public psInit(Boot boot)
        {
            this.boot = boot;
            ssi = boot.ssInit;
        }

        public void Copy(ProductSet ps, string FileName, string SheetN,
            string LoadDescriptor, int i0 = -1, int il = -1)
        {
            name = ps.name;
            ValidUntil = ps.ValidUntil;
            Date = ps.Date;
            SuplName = ps.SuplName;
            PSid = ps.PSid;
            RuleText = ps.RuleText;
            psMD5 = ps.psMD5;
            this.FileName = FileName;
            string filepath = Path.Combine(boot.ExcelDir, FileName);
            if (!boot.fo.IsFileExist(filepath))
                throw new ArgumentException($"Нет файла \"{filepath}\"");
            this.SheetN = SheetN;
            ////////////////if(!boot.Doc.Init(name, boot.ExcelDir, FileName, SheetN))
            ////////////////{ }
            ////////////////var doc = boot.Doc.GetDoc(name);
            // 18/2 ////////bool b = doc.IsDocExist(name);
            ////////////////boot.fo.FileOpen(boot.ExcelDir, FileName);
            ////////////////if(!boot.fo.IsSheetExist(SheetN))
            ////////////////    throw new ArgumentException($"В файле \"{filepath}\" нет листа \"{SheetN}\"");
            this.LoadDescriptor = LoadDescriptor;
            if (string.IsNullOrEmpty(LoadDescriptor))
                throw new ArgumentNullException("пустой LoadDescriptor");
        }
    } // end psInit class
} // end namespace