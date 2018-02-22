/* --------------------------------------------
 * UT_ProductSet 22.2.2018 Pavel Khrapkin
 * ----- History: -----------------
 * 24.01.2018 - UT of the protected setField and setFields
 * 18.02.2018 - перенос в PriceMatch, UT_Update, UT_Boot
 * ----- UTs: ---------------------
 * UT_Boot      - проверка, что класс правильно инициализирован
 * UT_Update    - тест обновления прайс-листа из Excel
 * UT_RdExcel   - тест чтения прайс-листа из Excel
 * 
 ! UT_setFields - запись атрибутов в List<Good>
 ! UT_setField  - запись одного поля с масштабированием
 * 
 * UT_Init_CSharp - test GS.Init GoodSet from C#
 * UT_Init_XML   - test GS.Init GoodSet from XML
 * UT_Init_Excel - test Init from Excel price-list in Excel
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace PriceMatch.Tests
{
    [TestClass()]
    public class UT_ProductSet
    {
        Boot boot = new Boot();
        ProductSet ps;
        psInit psi;
        _UT_ProductSet _U;
        _UT_psInit _I;
        string path;

        [TestInitialize]
        public void Initialyze()
        {
            path = Path.Combine(boot.DebugDir, "Prices");
            ps = new ProductSet(boot);
            psi = new psInit(boot);
            _U = new _UT_ProductSet(boot);
            _I = new _UT_psInit(boot);

            ////////////if (boot.Suppliers == null) System.Threading.Thread.Sleep(1000);
            //18/2//////Assert.IsNotNull(boot.Suppliers);
            ////////////gs.Init(boot);
        }

        [TestMethod()]
        public void UT_Boot()
        {
            Assert.IsNotNull(_U.checkBoot);
            Assert.IsNotNull(_I.checkBoot);
            Assert.IsTrue(_I._ssi().Count > 5);
        }

        [TestMethod()]
        public void UT_Update()
        {
  //          ps.Update("СтальХолдинг");

            string suplName = "СтальХолдинг";
            string psName = "Лист";
            var AllSuppliers = boot.Suppliers.AllSuppliers;
            Supplier supl = AllSuppliers.Find(x => x.name == suplName);
            ps = supl.productSets.Find(x => x.name.Contains(psName));
            ps.SuplName = supl.name;
            ps.Init(boot);
            var supl_I = boot.ssInit.Find(x => x.name == suplName);
            var psi = supl_I.pssInit.Find(x => x.name.Contains(psName));

            ps.Update(psi);

            Assert.AreEqual(ps.name, psName);
            Assert.AreEqual(ps.SuplName, suplName);
            Assert.IsTrue(ps.Products.Count > 1);

        }
    } // end class

    class _UT_ProductSet : ProductSet
    {
        public _UT_ProductSet(Boot boot) : base(boot) { }

        internal Boot checkBoot => boot;
    }
    class _UT_psInit : psInit
    {
        public _UT_psInit(Boot boot) : base(boot) { }

        internal Boot checkBoot => boot;
        internal List<SupplierInit> _ssi() => ssi;
    }
} // end namespace
#if fff

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Decl = TSmatch.Declaration.Declaration;
using Sec = TSmatch.Section.Section;

namespace PriceResx.Tests
{
    [TestClass()]
    public class UT_GoodSet
    {
        Boot boot = new Boot();
        Supplier supl = new Supplier();
        GoodSet gs = new GoodSet();
        const string file = "UT_Supplier_Spar.xml";
        string path;

        [TestInitialize]
        public void Initialyze()
        {
            boot.Init();
            path = Path.Combine(boot.DebugDir, "Prices");
            if (boot.Suppliers == null) System.Threading.Thread.Sleep(1000);
            Assert.IsNotNull(boot.Suppliers);
            gs.Init(boot);
        }

        [TestMethod()]
        public void UT_setFields()
        {
            string pathExcel = boot.TOCdir + @"\База комплектующих";
            string fileExcel = "СЗМ_black_M.xlsx";
            string sheetExcel = "Арматура";
            string docName = "Арматура_СЗМ";
            string ld = "профиль: 1; цена: 4; руб / т:; Длина заготовки м: 5х1000";
            Assert.IsTrue(boot.Doc.Init(docName, pathExcel, fileExcel, sheetExcel, LoadDescription: ld));
            var _gs = new _UTgs(boot);
            _gs.__set_product(new Good());
            _gs.__set_doc(boot.Doc.GetDoc(docName));
            var sec = boot.Sect.Get(ld, Decl.STypeLNG);

            var v = _gs._setFields(14, sec, Decl.STypeLNG);

            Assert.AreEqual(1, v.Count);
            Assert.AreEqual(11700.0, v[0]);
        }

        [TestMethod()]
        public void UT_setField()
        {
            var _gs = new _UTgs(boot);
            _gs.__set_product(new Good());

            // test 0: простое занесение текстовой строки
            string str = "текстик";
            Assert.AreEqual(str, _gs._setField(str));

            // test 1: масштаб в LoadDescription Section не указан => строка
            str = "3.8510";
            var v = _gs._setField(str);
            Assert.AreEqual(str, _gs._setField(str));

            // test 2: масштаб 1000, sv="11,7" => строка
            str = "11,7";
            var sec = boot.Sect.Get("Длина заготовки м: 5х1000");
            v = _gs._setField(str, sec.body);
            Assert.AreEqual(11700.0, v);

            // test 3: масштаб 1000, sv="11.7" => строка
            str = "11.7";
            //           var sec = boot.Sect.Get("Длина заготовки м: 5х1000");
            v = _gs._setField(str, sec.body);
            Assert.AreEqual(11700.0, v);
        }

        [TestMethod()]
        public void UT_Init_CSharp()
        {
            //-- Arrange
            gs.name = "Овощи и фрукты";
            gs.Date = new DateTime(2017, 12, 10);

            gs.RuleText = new List<string>() { "руб./кг:; " };

            // инициализация Goods
            gs.Goods.Add(new Good("Яблоки", 119));
            gs.Goods.Add(new Good("Апельсины", 55));
            gs.Goods.Add(new Good("Груша китайская", 73.90));
            gs.Goods.Add(new Good("Виноград черный", 99.90));

            //-- Assert
            Assert.IsFalse(string.IsNullOrEmpty(gs.name));
            Assert.IsTrue(gs.Date > (new DateTime(2010, 1, 1)) && gs.Date < DateTime.Now);
            Assert.AreEqual(4, gs.Goods.Count);
            foreach (var prod in gs.Goods)
            {
                Assert.IsTrue(prod.name.Length > 0);
                Assert.AreEqual(prod.name.GetType(), typeof(string));
                Assert.IsTrue(prod.price > 0);
                Assert.AreEqual(prod.price.GetType(), typeof(double));
            }

            // проверка сериализуемости GoodSet
            path = Path.Combine(boot.DebugDir, "UT");
            boot.Lib.WrXML(path, "UT_GoodSet_Init_CSharp.xml", gs);
        }

        [TestMethod()]
        public void UT_Init_XML()
        {
            //-- Arrange
            Supplier supl = boot.Suppliers.AllSuppliers.Find(x => x.name == "ГК Монолит СПб");
            gs = supl.goodSets[0];

            //-- Assert
            Assert.IsFalse(string.IsNullOrEmpty(gs.name));
            Assert.IsTrue(gs.Date > (new DateTime(2010, 1, 1)) && gs.Date < DateTime.Now);
            Assert.IsTrue(gs.Goods.Count > 0);
            foreach (var prod in gs.Goods)
            {
                Assert.IsTrue(prod.name.Length > 0);
                Assert.AreEqual(prod.name.GetType(), typeof(string));
                Assert.IsTrue(prod.price > 0);
                Assert.AreEqual(prod.price.GetType(), typeof(double));
            }

            // проверка сериализуемости GoodSet
            path = Path.Combine(boot.DebugDir, "UT");
            boot.Lib.WrXML(path, "UT_GoodSet_Init_XML.xml", gs);
        }

        [TestMethod()]
        public void UT_Init_FromExcel()
        {
            //-- Arrange
            Supplier supl = boot.Suppliers.AllSuppliers.Find(x => x.name == "База СЕВЗАПМЕТАЛЛ");
            var gsSZM = supl.goodSets.Find(x => x.name == "Швеллер_СЗМ");

            string pathExcel = boot.TOCdir + @"\База комплектующих";
            string fileExcel = "СЗМ_black_M.xlsx";
            string sheetExcel = "Швеллер";
            string ld = "M:1; опис:3; профиль:2; цена: 6; Ед: руб/т";
            bool isFile = boot.Doc.Init(gsSZM.name, pathExcel, fileExcel, sheetExcel,
                "22.12.2015", "8.3.2016", 7, LoadDescription: ld);
            var doc = boot.Doc.GetDoc(gsSZM.name);
            Assert.AreEqual(7, doc.i0);
            Assert.AreEqual(40, doc.il);
            gs.Init(boot);
            gs = gs.Init(gsSZM.name, doc, "1.12.2015");

            //-- Assert
            Assert.AreEqual(gs.name, gsSZM.name);
            Assert.AreEqual(gs.Date, gsSZM.Date);
            for (int i = 0; i < gs.Goods.Count; i++)
            {
                try
                {
                    string pPrf = gs.Goods[i].Fields[0].ToString();
                    string zPrf = gsSZM.Goods[i].Fields[0].ToString();
                    Assert.AreEqual(pPrf, zPrf);
                    string pName = gs.Goods[i].name;
                    string zName = gsSZM.Goods[i].name;
                    Assert.AreEqual(pName, zName);
                }
                catch { };
            }
            Assert.AreEqual(gs.Goods.Count, gsSZM.Goods.Count);
        }

        [TestMethod()]
        public void UT_Fill()
        {
            // test 0: "name"
            GoodSet x = gs.Fill("name", "bb");
            Assert.AreEqual("bb", x.name);

            // test 1: "Date"
            x = gs.Fill("Date", "12.3.2016");
            Assert.AreEqual(new DateTime(2016, 3, 12), x.Date);
        }
    } // end class

    internal class _UTgs : GoodSet
    {
        public _UTgs(Boot boot) { Init(boot); }

        public object _setField(string str, string SType = "")
        {
            setField(str, SType);
            return product.Fields[product.Fields.Count - 1];
        }

        public List<object> _setFields(int i, Sec sec, string SType = "")
        {
            setFields(i, sec);
            return product.Fields;
        }

        internal void __set_product(Good good) => product = good;
        internal void __set_doc(Document d) => doc = d;
    } // end _UTgs class
} // end namespace
#endif