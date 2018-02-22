/* -----------------------------------------------------------
 * UT_Document 17.02.2018 Pavel Khrapkin 
 * 
 * --- Тесты: -----
 * UT_Init  -  проверка doc.Init - инициализация и потом
 *             test0 и test1 чтение прайс-листов ГК Монолит и ЛенСпецСталь
 * UT_GetDoc - проверка получения документа
 * UT_IsDocExist - тест проверки наличия документа
 * 
 * UT_tic_init - проверка инициализации toc
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PriceMatch.Tests
{
    [TestClass()]
    public class UT_Document
    {
        Boot boot = new Boot();
        Document Doc;

        [TestInitialize]
        public void Initialyze() { Doc = boot.Doc; }

        [TestMethod()]
        public void UT_Init()
        {
            string MONOLIT = "ГК Монолит";

            // test 0: Init документ из прайс-листов поставщиков
            string pathExcel = boot.TOCdir + @"\База комплектующих";
            string fileExcel = "БетонГКмонолитСПБ.xlsx";
            string sheetExcel = "Товарный бетон";
            string ld = "Мат: 1; Описание: 3; Цена: 4; Ед: руб/м3";

            bool b = boot.Doc.Init(MONOLIT, pathExcel, fileExcel, sheetExcel, FirstRow: 8, LoadDescription: ld);

            Assert.IsTrue(b);
            Document doc = Doc.GetDoc(MONOLIT);
            Assert.IsNotNull(doc);
            Assert.AreEqual(MONOLIT, doc.name);
            Assert.IsTrue(doc.IsOpen);


            // test 1: Init(ЛСС-Уголок, RuleText)
            fileExcel = "Ленспецсталь.xlsx";
            sheetExcel = "Уголок равнопол.";
            ld = "M:1; профиль:2; опис:3; цена: 5; руб/т:;";
            string rl = "M: C235 = C245 = C255; Проф: Уголок=L{0}x{0}";
            string date = "05.04.2017";
            string name = "ЛСС-уголок";
            b = boot.Doc.Init(name, pathExcel, fileExcel, sheetExcel, date, date, 5, null, ld, rl);
            Assert.IsTrue(b);
            doc = boot.Doc.GetDoc(name);
            Assert.AreEqual(name, doc.name);
            Assert.AreEqual(sheetExcel, doc.SheetN);
            Assert.AreEqual(ld, doc.LoadDescription);
            Assert.AreEqual(1, doc.RuleText.Count);
            Assert.AreEqual(rl, doc.RuleText[0]);
        }

        [TestMethod()]
        public void UT_GetDoc()
        {
            string MONOLIT = "ГК Монолит";

            // test 1: "ГК Монолит"
            if (!Doc.IsDocExist(MONOLIT)) UT_Init();
            Document doc = Doc.GetDoc(MONOLIT);
            Assert.IsNotNull(doc);
            Assert.AreEqual(MONOLIT, doc.name);
            Assert.IsNotNull(doc.Body);
            Assert.AreEqual(8, doc.i0);
            Assert.IsTrue(doc.il > doc.i0);
            Assert.IsTrue(doc.LoadDescription.Length > doc.i0);
        }

        [TestMethod()]
        public void UT_IsDocExist()
        {
            // test 0: TOC всегда существует, если было boot.Init();
            Assert.IsTrue(Doc.IsDocExist("TOC"));

            // test 1: заведомо не существующий документ
            Assert.IsFalse(Doc.IsDocExist("bla-bla"));

            // test 2: вначале "ModelINFO" пустой, он есть только в TOC, потом его открывают
            //         но он всегда существует, есть он в каталоге модели или нет
            string name = "ModelINFO";
            bool ok = Doc.IsDocExist(name);
            Assert.IsTrue(ok);
            Document dINFO = Doc.GetDoc(name);
            Assert.AreEqual(name, dINFO.SheetN);

            //!! переписать 3ю12ю17 !!
            ////////////if (ok)
            ////////////{
            ////////////    Document rep = Document.getDoc("Report", create_if_notexist: false, fatal: false);
            ////////////    Assert.IsNotNull(rep);
            ////////////}
            ////////////else
            ////////////{
            ////////////    name = "UT_DEBUG";
            ////////////    Document doc = Document.getDoc(name, create_if_notexist: true, fatal: false, reset: true);
            ////////////    Assert.IsTrue(Document.IsDocExists(name));
            ////////////    doc.Close();
            ////////////    //31/7                FileOp.Delete(doc.FileDirectory, name + ".xlsx");
            ////////////    Assert.IsFalse(Document.IsDocExists(name));
            ////////////}
        }

        [TestMethod()]
        public void UT_Doc_IntStr()
        {

            //var doc = Doc.GetDoc("Suppliers");

            //string supl1 = doc.Str(4, 2);
            //Assert.AreEqual("Росметаллопрокат", supl1);

            //int index = doc.Int(15, 5);
            //Assert.AreEqual(644105, index);

            //double x = doc.Dec(15, 5);
            //Assert.AreEqual(644105.0, x);
        }
        [TestMethod()]
        public void UT_toc_init()
        {
            // test 0: documents должен быть инициализирован даже при самом первом обращении
            //         т.к. он заполнен в классе Document.toc во время boot.Init();
            Document v = Doc.GetDoc();

            Assert.AreEqual("TOC", v.name);
            Assert.IsTrue(v.IsOpen);
            Assert.IsTrue(v.Body.ToList().Count > 3);

            // проверяем документы - листы TSmatchINFO.xlsx : ModelINFO, Report, Rules  
            sub(v, Declaration.sINFO);
            sub(v, Declaration.sReport);
            sub(v, Declaration.sRules);
        }

        internal void sub(Document toc, string name)
        {
            const string fn = "TSmatchINFO.xlsx";
            var vv = toc.Body.ToList();
            Document doc = null;
            foreach (var d in vv)
            {
                doc = (Document)d;
                if (doc.name == name) break;
            }
            Assert.IsNotNull(doc);
            Assert.IsFalse(doc.IsOpen);
            Assert.AreEqual(doc.name, doc.SheetN);
            Assert.AreEqual(doc.path, boot.ModelDir);
            Assert.AreEqual(fn, doc.FileName);
            Assert.AreEqual(2, doc.i0);
            if (doc.i0 != 2) Assert.Fail("doc[" + name + "].i0 != 2");
            Assert.AreEqual(-1, doc.il);
        }
    }
} // end namespace