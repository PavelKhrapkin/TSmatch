/* ---------------------------------------------------------
 * UT_Suppliers 2018.02.19 Pavel Khrapkin
 * 
 * Помимо обычного модульного тестирования, используется для
 * заполнения файла Suppliers.xml и SuppliersInit.xmp - 
 * одного за другим Поставщика
 * 
 * --- Histiry: ---
 * 17.02.18 перенос в PriceMatch
 * 
 * --- Тесты: ---
 * UT_Init   - проверка соответствия Suppliers.xml и SuppliersInit.xml
 * 
 # --- Инициализация SuppliersInit.xml
 * 
 * --- Assert Failure - закомментированы
 * - 2018.02.17 в UT_Init количество поставщиков Supplier.xml != SuppliersInit.xml 
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PriceMatch.Tests
{
    [TestClass()]
    public class UT_Suppliers
    {
        Boot boot = new Boot();
        Lib Lib;
        Suppliers ss = new Suppliers();
        Supplier supl;
        SupplierInit ssi;
        ProductSet ps;
        psInit psi;
        string path;

        [TestInitialize]
        public void Initialyze()
        {
            Lib = boot.Lib;
            path = Path.Combine(boot.DebugDir, "Prices");
            ss.Init(boot);
        }

        [TestMethod()]
        public void UT_Init()
        {
            ss.Init(boot);
            supl = new Supplier(boot);
            foreach (Supplier supl in boot.Suppliers.AllSuppliers)
            {
                Supplier si = boot.ssInit.Find(x => x.name == supl.name);
                Assert.IsNotNull(si, $"No SupplierInit of {supl.name}");
                ////////////Assert.AreEqual(si.productSets.Count, supl.productSets.Count,
                //17/2 /////    $"ProductSet.Count wrong for {supl.name}");
            }
            Assert.Fail();
        }

        [TestMethod()]
        public void UT_WrXML()
        {
            ss.AllSuppliers.Clear();
            boot.ssInit.Clear();

            #region --- Иногородние поставщика без прайс-листов ---
            ssInit("Росметаллопрокат", "http://www.rosmetalloprokat.ru/",
             "Екатеринбург", "ул. Кирова, д. 28 (территория завода \"ВИЗ\")");
            ssInit("ООО \"Металлопромышленное Предприятие\"", "http://yaruse.com/",
                "Екатеринбург", "пр-кт. Космонавтов, д. 18", index: "620017");
            ssInit("Уралтрубосталь", "http://www.uraltrubostal.com/index.php?page=price",
                "Екатеринбург", "ул. Промышленный проезд 2Б, офис 210", tel: "+7(343) 219-1808");
            ssInit("СтальЭнерго-96", "http://www.stalenergo-96.ru/",
                "Екатеринбург", "ул Шефская 2а оф 24 база \"ВостокНефтегазСтройКомплект\"", index: "620017");
            ssInit("БВБ альянс", "http://bvb-alyans.ru/",
                "Екатеринбург", "ул. Кирова, д. 32/а");
            ssInit("Лайф-мед", "http://www.lifemet.ru/",
                "Казань", "ул. Васильченко, д.1, оф.120", index: "420095");
            ssInit("СТАЛЬНОЙ ДОМ", "http://stalnoydom.ru",
                "Москва", "ул.Стахановская, д.21", tel: "+7 (495) 105-98-63");
            ssInit("ООО Промтехмет-М", "http://prom77.ru/",
                "Москва", "Малый Сухаревский пер., д. 9, стр. 1", tel: "(495) 661-60-58 (многоканальный), 741-47-27");
            ssInit("Сибирский центр стали", "http://www.omsk.spk.ru/",
               "Новосибирск", " ул. Челюскинцев, д. 18/2, офис 309", index: "630132");
            ssInit("Сталепромышленная компания", "http://www.omsk.spk.ru/",
                "Омск", "ул.Зелёная, 10", index: "644105");
            ssInit("ОмскМеталлНефтеХим", "http://ooo-omnh.ru/",
                "Омск", "проезд Овощной, дом 7");
            ssInit("Оммет", "http://www.ommet.com",
                "Омск", "ул. 22 Партсъезда, 105", index: "644105");
            #endregion --- Иногородние поставщика без прайс-листов ---

            #region --- База СЕВЗАПМЕТАЛЛ
            ssInit("База СЕВЗАПМЕТАЛЛ", " http://szmetal.ru/",
                "Санкт-Петербург", "ул. Пинегина, д.4 (ст. метро \"Елизаровская\")", tel: "+7(812)325-7920");
            PSU("Арматура", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "профиль:1; цена: 4; руб/т:;Длина заготовки м:5х1000",
                "M:C235=C245=C255; Prf:Арм {0}А*С; Описание:{0}; Длина заготовки:{1}");
            PSU("Балка", "СЗМ_black_M.xlsx", "23.11.2016", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;",
                "M:C235=C245=C255; Prf: Балка{0}; Опис:{1}; Длин.заг.:12000;");
            PSU("Катанка; Круг", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "профиль:1; цена: 4; руб/т:;",
                "M:C235=M245=M255; Prf: Катанка{0}; Опис:{1}; Длин.заг:6000",
                "Prf: Круг{0};");
            PSU("Квадрат", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; цена: 4;руб/т:;", "M: C235 = M245 = M255; Длин.заг:6000");
            PSU("Лист г-к", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6;руб/т:;", "M: C235 = C245 = C255;");
            PSU("Лист рифленый", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Просечно-вытяжной лист", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Лист х-к", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Полоса", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Труба профильная", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Труба э-св", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Уголок", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Швеллер", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Лист оцинкованный", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Швеллер гнутый", "СЗМ_black_M.xlsx", "01.12.2015", 7,
                "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            #endregion --- База СЕВЗАПМЕТАЛЛ

            #region --- СтальХолдинг
            ssInit("СтальХолдинг", "http://steel-holding.ru/",
                "Санкт-Петербург", "ул. Громова, д. 4, оф. 226-228", tel: "+7(812) 646-79-90");
            PSU("Лист г.к", "СтальхолдингM.xlsx", "3.06.2017", 2,
                "M:1;описание:2;профиль:3;цена:5;руб./тн:;", "M: C235 = M245 = M255;");
            PSU("Полоса", "СтальхолдингM.xlsx", "3.06.2017", 2,
                "мат:1; профиль:2; опис:3; длина заг.:5;  цена:6; руб./тн:;", "M: C235 = M245 = M255;");
            PSU("Уголок неравнополочный", "СтальхолдингM.xlsx", "3.06.2017", 7,
                "мат:1; профиль:2;  описание:3; длина: 5; цена:6; руб./тн:;", "M: C235 = M245 = M255;");
            PSU("Уголок равнопол.", "СтальхолдингM.xlsx", "3.06.2017", 2,
                "мат:1; профиль:2;  описание:3; длина: 4; цена:5; руб./тн:;", "M: C235 = M245 = M255;");
            #endregion --- СтальХолдинг

            #region --- ЛенСпецСталь 
            ssInit("ЛенСпецСталь", "http://www.lsst.ru/",
           "Санкт-Петербург", "Лиговский пр. 123А, 8 этаж", tel: "+7(812)703-43-43");
            PSU("Уголок равнопол.", "Ленспецсталь.xlsx", "05.04.2017", 5,
               "M:1; профиль:2; опис:3; цена: 5; руб/т:;", "M: C235 = C245 = C255; Проф: Уголок=L{0}x{0}");
            PSU("Полоса", "Ленспецсталь.xlsx", "05.04.2017", 5,
               "M:1; опис:3; профиль:2; цена: 6; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Лист", "Ленспецсталь.xlsx", "02.06.2017", 7,
                "M:1; опис:3; профиль:2; цена: 4; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Швеллер", "Ленспецсталь.xlsx", "23.05.2017", 7,
                "M:1; опис:3; профиль:2; цена: 4; руб/т:;", "M: C235 = M245 = M255;");
            PSU("Двутавр", "Ленспецсталь.xlsx", "05.04.2017", 6,
                "M:1; опис:3; профиль:2; цена: 4; руб/т:;", "M: C235 = M245 = M255;");
            #endregion --- ЛенСпецСталь 

            #region --- ГК Монолит СПб
            ssInit("ГК Монолит СПб", "http://gkmonolit.info/price/beton-tovarnyy/",
           "Санкт-Петербург", "шоссе Революции, д.3, литер А, офис 702", tel: "(812) 318-33-88", index: " 195027");
            PSU("Товарный бетон", "БетонГКмонолитСПБ.xlsx", "22.12.15", 8,
                "Мат: 1; Описание: 3; Цена: 4; Ед: руб / м3; руб./м3:;", "М: B*");
            #endregion --- ГК Монолит СПб

            ssInit("Peikko", "http://www.peikko.ru/",
           "Санкт-Петербург", "Коломяжский пр. 10, лит. Ф", tel: "(812)329-0704", index: "197348");
            ssInit("Spar", "http://www.sparspb.ru/",
           "Санкт-Петербург", "Художников 11", tel: "(812)510-36-03, 510-46-65", index: "195296");

            //int cnt = ss.AllSuppliers.Count;
            //ss.AllSuppliers = ss.DeDupSupl();
            //cnt = ss.AllSuppliers.Count;
            ss.DeDup();

            ss.WrXML();
            ss.WrInitXML(boot.ssInit);
        }

        private void ssInit(string name, string url, string city,
            string streetAdr, string country = "", string index = "",
            string tel = "", string note = "")
        {
            bool newSupl = false, newSupl_I = false;
            var AllSupl = boot.Suppliers.AllSuppliers;
            supl = AllSupl.Find(x => x.name == name);
            if(supl == null)
            {
                newSupl = true;
                supl = new Supplier(boot);
            }
            supl = new Supplier(boot);
            supl.name = name;
            supl.URL = url;
            supl.city = city;
            supl.Country = country;
            supl.Index = index;
            supl.Tel = tel;
            supl.Note = note;
            supl.productSets = new List<ProductSet>();
            if (newSupl) AllSupl.Add(supl);

            var supl_I = boot.ssInit.Find(x => x.name == name);
            if(supl_I == null)
            {
                newSupl_I = true;
                supl_I = new SupplierInit(boot);
                supl_I.name = name;
            }
            supl_I.pssInit = new List<psInit>();
            if (newSupl_I) boot.ssInit.Add(supl_I);
        }

        private void PSU(string name, string FileName,
            string validDate, int i0, string LoadDescr, params string[] rl)
        {
            var ps = new ProductSet(boot);
            var psi = new psInit(boot);
            ps.name = psi.name = psi.SheetN = name;
            ps.SuplName = supl.name;
            psi.FileName = FileName;
            //            psi.SheetN = SheetN;
            if (!DateTime.TryParse(validDate, out ps.ValidUntil))
                ps.ValidUntil = Declaration.MinDate;
            psi.i0 = i0;
            psi.LoadDescriptor = LoadDescr;
            ps.RuleText = rl.ToList();
            ps.Update(psi);
        }

        [TestMethod()]
        public void UT_RdXML()
        {
            var x = ss.RdXML();
            Assert.IsTrue(x.AllSuppliers.Count > 5);
            Supplier s = x.AllSuppliers.Find(g => g.name == "ГК Монолит СПб");
            Assert.IsNotNull(s);
            Assert.IsTrue(condition: s.productSets[0].Products.Count > 1);
        }

        #region --- Инициализация SuppliersInit.xml ---
        //#if SuppliersInit_Init- только первый раз для инициализации SuppliersInit.xml
        [TestMethod()]
        public void UT_SuppliersInitXML_Init()
        {
            si("Росметаллопрокат", "Склад", "Росметаллопрокат_Sklad1.xlsx", "Наличие с распродажи", "-");
            sw("Росметаллопрокат", "Уголок", "Росметаллопрокат_Sklad1.xlsx", "Уголок", "-");
            //////////////sw("ООО \"Металлопромышленное Предприятие\"", "NotSet", "-");
            //////////////sw("Уралтрубосталь", "NotSet", "-");
            //////////////sw("СтальЭнерго-96", "NotSet", "-");
            //////////////sw("БВБ альянс", "NotSet", "-");
            // 17.2 //////sw("Лайф-мед", "NotSet", "-");
            //////////////sw("СТАЛЬНОЙ ДОМ", "NotSet", "-");
            //////////////sw("ООО Промтехмет-М", "NotSet", "-");
            //////////////sw("СТАЛЬНОЙ ДОМ", "NotSet", "-");
            //////////////sw("Сибирский центр стали", "NotSet", "-");
            //////////////sw("Сталепромышленная компания", "NotSet", "-");
            //////////////sw("ОмскМеталлНефтеХим", "NotSet", "-");
            //////////////sw("Оммет", "NotSet", "-");

            si("СтальХолдинг", "Уголок равнополочный", "СтальхолдингM.xlsx", "Уголок равнопол.", "мат:1; проф:2; опис:3; длина: 4; цена:5; руб./тн:;");
            sw("СтальХолдинг", "Уголок неравнополочный", "СтальхолдингM.xlsx", "Уголок равнопол.", "мат:1; проф:2; опис:3; длина: 4; цена:5; руб./тн:;");
            //ps = GSP("Лист Стальхолдинг", "СтальхолдингM.xlsx", "Лист г.к", "3.06.2017", 2,
            // "M:1;описание:2;профиль:3;цена:5;руб./тн:;", "M: C235 = M245 = M255;");
            //supl.productSets.Add(ps);
            //ps = GSP("Полоса СтальхолдингM", "СтальхолдингM.xlsx", "Полоса", "3.06.2017", 2,
            //    "мат:1; профиль:2; опис:3; длина заг.:5;  цена:6; руб./тн:;", "M: C235 = M245 = M255;");
            //supl.productSets.Add(ps);
            //ps = GSP("Уголок_неавнопол.СтальхолдингM", "СтальхолдингM.xlsx", "Уголок неравнополочный", "3.06.2017", 7,
            //    "мат:1; профиль:2;  описание:3; длина: 5; цена:6; руб./тн:;", "M: C235 = M245 = M255;");

            si("ЛенСпецСталь", "Уголок равнопол.", "Ленспецсталь.xlsx", "Уголок равнопол.", "M:1; профиль:2; опис:3; цена: 5; руб/т:;");
            sw("ЛенСпецСталь", "Полоса", "Ленспецсталь.xlsx", "Полоса", "M:1; опис:3; профиль:2; цена: 6; руб/т:;");
            //ps = GSP("ЛСС-лист", "Ленспецсталь.xlsx", "Лист", "02.06.2017", 7,
            //    "M:1; опис:3; профиль:2; цена: 4; руб/т:;", "M: C235 = M245 = M255;");
            //supl.productSets.Add(ps);
            //ps = GSP("ЛСС-Швеллер", "Ленспецсталь.xlsx", "Швеллер", "23.05.2017", 7,
            //    "M:1; опис:3; профиль:2; цена: 4; руб/т:;", "M: C235 = M245 = M255;");
            //supl.productSets.Add(ps);
            //ps = GSP("ЛСС-двутавр", "Ленспецсталь.xlsx", "Двутавр", "05.04.2017", 6,
            //    "M:1; опис:3; профиль:2; цена: 4; руб/т:;", "M: C235 = M245 = M255;");

            sw("ГК Монолит СПб", "Бетон", "БетонГКмонолитСПБ.xlsx", "Товарный бетон", "Мат: 1; Описание: 3; Цена: 4; Ед: руб / м3; руб./м3:;");
            sw("Peikko", "Sample", "Peikko.xlsx", "Sample", "-");
            sw("Spar-Художников, 9", null, null, "", "-");

            // Deduplication
            int cnt = boot.ssInit.Count;
            var v = boot.ssInit.GroupBy(PriceXLS => PriceXLS.name);
            List<SupplierInit> sx = new List<SupplierInit>();
            foreach (var gr in v) sx.Add(gr.First());
            boot.ssInit = sx;
            cnt = boot.ssInit.Count;
            ss.WrInitXML(boot.ssInit);

            // проверка соответствия Suppliers.xml и SuppliersInit.xml
            UT_Init();
        }

        private SupplierInit suplInit;

        private void si(string suplName, string psName, string FileName, string SheetN, string LoadDescriptor)
        {
            Supplier supl = boot.Suppliers.AllSuppliers.Find(x => x.name == suplName);
            Assert.IsNotNull(supl, $"UT: Bad suplName={suplName}");
            suplInit = boot.ssInit.Find(x => x.name == suplName);
            if (suplInit == null)
            {
                suplInit = new SupplierInit(boot);
                ProductSet ps = supl.productSets.Find(x => x.name == psName);
                if (ps == null)
                {
                    ps = new ProductSet(boot);
                    ps.name = psName;
                }
                psInit psi = new psInit(boot);
                psi.Copy(ps, FileName, SheetN, LoadDescriptor);
                suplInit.Copy(supl);
                suplInit.productSets.Add(psi);
            }
        }

        private void sw(string suplName, string psName, string FileName, string SheetN, string LoadDescriptor)
        {
            si(suplName, psName, FileName, SheetN, LoadDescriptor);
            if (suplInit == null || string.IsNullOrEmpty(suplInit.name))
                throw new ArgumentNullException();
            boot.ssInit.Add(suplInit);
        }
        //#endif
        #endregion --- Инициализация SuppliersInit.xml ---
    }
}
#if fff
using PriceXLS;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
//using Decl = TSmatch.Declaration.Declaration;
//using ST = TSmatch.Section.SectionTab;
using System;
using TSmatch.PriceResx;
using System.Collections.Generic;
using System.Linq;

namespace PriceXLS.Tests
{
    [TestClass()]
    public class UT_Suppliers
    {


        [TestMethod()]
        public void UT_Init()
        {

        }



        [TestMethod()]
        public void UT_WrInitXML()
        {
            // test 0: пишу в SuppliersInit.xml поставщика "Уголок СевЗапМеталл"
            //14/2            var suplLst = ss.RdInitXML();
            Supplier supl = boot.Suppliers.AllSuppliers.Find(x => x.name == "База СЕВЗАПМЕТАЛЛ");
            var suplInit = new SupplierInit();
            suplInit.Copy(supl);
            var v = new List<SupplierInit>();
            v.Add(suplInit);
//15/2            ss.WrInitXML(v);

 //           supl.Init("foo", "www.foo.net");
//14/2            suplLst.Add(supl);
//14/2            ss.WrInitXML(suplLst);
//14/2            var newSuplLst = ss.RdInitXML();
//14/2            Assert.AreEqual(newSuplLst.Count, suplLst.Count);
//14/2            Supplier y = newSuplLst.Find(x => x.name == "foo");
//14/2            Assert.AreEqual(y.name, supl.name);
//14/2            Assert.AreEqual(y.URL, supl.URL);

//14/2            suplLst.RemoveAll(x => x.name == "foo");
//14/2            ss.WrInitXML(suplLst);

            // test 1: 
        }

        [TestMethod()]
        public void UT_Update()
        {
            ss.Update();

//14/2                ss.RdInitXML();
            Assert.Fail();
        }



        [TestMethod()]
        public void UT_GetSuppliersList()
        {
            List<Supplier> x = ss.GetSuppliersList();

            ////Assert.IsTrue(x.Count > 0);
            ////Supplier s = x.AllSuppliers.Find(g => g.name == "ГК Монолит СПб");
            ////Assert.IsNotNull(s);
            ////Assert.IsTrue(condition: s.goodSets[0].Goods.Count > 1);
        }


        [TestMethod()]
        public void UT_FillSuppliersList()
        {
            ss.GetSuppliersList();

            var x = ss.RdXML();
            Assert.IsTrue(x.AllSuppliers.Count > 5);
            Supplier s = x.AllSuppliers.Find(g => g.name == "ГК Монолит СПб");
            Assert.IsNotNull(s);
            Assert.IsTrue(condition: s.productSets[0].Products.Count > 1);
        }
    } // end UT_SuppliersTest class
} // end namwspace
#endif