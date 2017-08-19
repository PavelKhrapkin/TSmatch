/*===================================
 *  Saved Report Unit Test 16.08.2017
 *===================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileOp = match.FileOp.FileOp;
using Boot = TSmatch.Bootstrap.Bootstrap;

namespace TSmatch.Document.Tests
{
    [TestClass()]
    public class UT_DocumentTests
    {
        [TestMethod()]
        public void getDoc()
        {
            Boot boot = new Boot();

            // test 1: getDoc() => TOC
            Document toc = Document.getDoc();   // static getDoc()
            Assert.AreEqual(toc.name, "TOC");

            //test 2: getDoc(ModelINFO)
            if (Document.IsDocExists("ModelInfo"))
            {
                Document doc = Document.getDoc("ModelINFO", reset: false);
                Assert.AreEqual(doc.il > 9, true);
            }

            //test 3: getDoc(InitRules)
            Document ir = Document.getDoc("InitialRules");
            Assert.AreEqual("InitialRules", ir.name);
            Assert.AreEqual(4, ir.i0);
            Assert.AreEqual(15, ir.il);


            //test 4: getDoc("ГК Монолит")
            string docName = "ГК Монолит";
            Document d = Document.getDoc(docName);
            Assert.AreEqual(docName, d.name);
            Assert.AreEqual(8, d.i0);
            Assert.AreEqual(15, d.il);

            //-- реализация Get без static - еще не работает 20/4/17
            ////Document doc = doc.Get();           // возвращает TOC
            ////Assert.AreEqual(doc.name, "TOC");

            ////string name = "docNotExists";
            ////doc = new Document(name);
            ////Assert.AreEqual(doc.name, name);

            ////doc.Get(name);  // читает из файла name
            ////Assert.Fail();

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Reset()
        {
            Boot boot = new Boot();
            Document doc = Document.getDoc("ModelINFO", reset: true, create_if_notexist: true);

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.il > doc.i0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_isDocExists()
        {
            Assert.IsFalse(Document.IsDocExists("TOC"));    // до вызова Boot документ ТОС не существует
            Boot boot = new Boot();
            Assert.IsTrue(Document.IsDocExists("TOC"));     // поле Boot - OK
                                                            //31/7            Assert.IsTrue(Document.IsDocExists());
            Assert.IsFalse(Document.IsDocExists("bla-bla")); // заведомо не существующий документ

            string name = "ModelINFO";
            bool ok = Document.IsDocExists(name);
            if (ok)
            {
                Document rep = Document.getDoc("Report", create_if_notexist: false, fatal: false);
                Assert.IsNotNull(rep);
            }
            else
            {
                name = "UT_DEBUG";
                Document doc = Document.getDoc(name, create_if_notexist: true, fatal: false, reset: true);
                Assert.IsTrue(Document.IsDocExists(name));
                doc.Close();
                //31/7                FileOp.Delete(doc.FileDirectory, name + ".xlsx");
                Assert.IsFalse(Document.IsDocExists(name));
            }

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Start()
        {
            Boot boot = new Boot();

            var Documents = Document.__Documents();
            Assert.IsTrue(Documents.Count > 50);

            // test InitialRules
            string sIR = "InitialRules";
            Document ir = Documents[sIR];
            //            ir.
            Assert.AreEqual(4, ir.i0);
            Assert.AreEqual(15, ir.il);

            Assert.IsTrue(true);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_EOL()
        {
            Boot boot = new Boot();
            string sIR = "InitialRules";

            var doc = Document.getDoc(sIR);

            Assert.AreEqual(sIR, doc.name);
            Assert.AreEqual(4, doc.i0);
            Assert.AreEqual(15, doc.il);

            FileOp.AppQuit();
        }
    }
}