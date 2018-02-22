using PriceMatch;
/* --------------------------------------------------------
* UT_Supplier 22.02.2018 Pavel Khrapkin
* 
* --- History: ---
* 17.02.18 audit, UT_Init add
* 22.02.18 UT_SupplierIni/UT_Copy
* --- тесты: ---
* UT_Init       - тест инициализации - проверяем, что Supplier.boot != null
! UT_WrXMLfrExcel - 
* UT_Fill       - простой тест - вызов заполнения полей Supplier.Fields
* UT_Copy       -
* UT_Update     - обновление поставщика для SupplierInit.xml
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace PriceMatch.Tests
{
    [TestClass()]
    public class UT_Supplier
    {
        Boot boot = new Boot();
        Supplier supl = new Supplier();
        _UT_Supplier _U;
        string path;
        const string file = "UT_Supplier_Spar.xml";

        [TestInitialize]
        public void Initialyze()
        {
            supl.Init(boot);
            _U = new _UT_Supplier(boot);
            //           boot.ST.Init(Decl.STypeMAT, "Наим");
            //           boot.ST.Init(Decl.STypePRICE, "Цена");
            //           var stDic = boot.ST.Init(Decl.STypeUNIT_WGT, "руб / кг");
            //31/12    var stDic = boot.ST.Merge();
            //17/2 !! supl.Init("Spar", "www.sparspb.ru", "Санкт-Петербург", "Художников 9");
            path = Path.Combine(boot.DebugDir, "UT");
        }
        [TestMethod()]
        public void UT_Init()
        {
            // test 0: проверяем, что Supplier.boot != null
            Assert.IsNotNull(_U.checkBoot);
        }

        [TestMethod()]
        public void UT_Update()
        {
            supl.Update("База СЕВЗАПМЕТАЛЛ", "Балка_СЗМ");
            Assert.Fail();
        }

        [TestMethod()]
        public void UT_WrXMLfrExcel()
        {
            string pathExcel = boot.TOCdir + @"\База комплектующих";
            string fileExcel = "БетонГКмонолитСПБ.xlsx";
            string sheetExcel = "Товарный бетон";
            string ld = "Мат: 1; Описание: 3; Цена: 4; Ед: руб/м3";
            Assert.IsTrue(boot.fo.IsFileExist(pathExcel, fileExcel));
            supl.Init(boot);
            supl.WrXMLfrExcel(pathExcel, fileExcel, sheetExcel, ld, path);

            string file = Path.ChangeExtension(fileExcel, "xml");
            //           Assert.IsTrue(boot.fo.IsFileExist(path, file));
        }

        [TestMethod()]
        public void UT_Fill()
        {
            Supplier x = supl.Fill("name", "bb");
            Assert.AreEqual("bb", x.name);
        }

        [TestMethod()]
        public void UT_Copy()
        {
            supl = boot.ssInit[2];  
            Assert.AreEqual("", supl.name);
            var supl_I = new SupplierInit(boot);

            var v = supl_I.Copy(supl);
        }
    }

    class _UT_Supplier : Supplier
    {
        public _UT_Supplier(Boot boot) : base(boot) { }

        internal Boot checkBoot => boot;
    }
}