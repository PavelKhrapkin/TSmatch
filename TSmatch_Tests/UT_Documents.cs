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
        Boot boot = new Boot();

        [TestMethod()]
        public void UT_DocMsg()
        {
            boot.Init();
// не написано, отложил на потом!!!!!!!!!!!!!!!!
            // test 1: getDoc("No File")
            // test 2: getDoc("No SheetN")
   //         Assert.Fail();

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void getDoc()
        {
            boot.Init();

            // test 1: getDoc() => TOC
            Document toc = Document.getDoc();   // static getDoc()
            Assert.AreEqual(toc.name, "TOC");

            //test 2: getDoc(ModelINFO)
            if (Document.IsDocExist("ModelInfo"))
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
            boot.Init();
            Document doc = Document.getDoc("ModelINFO", reset: true, create_if_notexist: true);

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.il > doc.i0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_isDocExists()
        {
            Assert.IsFalse(Document.IsDocExist("TOC"));    // до вызова Boot документ ТОС не существует
            boot.Init();
            Assert.IsTrue(Document.IsDocExist("TOC"));     // поле Boot - OK
            Assert.IsTrue(Document.IsDocExist());
            Assert.IsFalse(Document.IsDocExist("bla-bla")); // заведомо не существующий документ

            // test 1: chech if Tab "ModelINFO" exist in current TSmatchINFO.xlsx
            //         if NOT - check Doc UT_Docunent in UT_Debug.xlsx
            string sINFO = "ModelINFO";
            Document rep = Document.getDoc(sINFO, create_if_notexist: false, fatal: false);
            bool ok = false;
            if (rep != null) ok = Document.IsDocExist(sINFO);
            else
            {
                string sUTdoc = "UT_Document";
                Document ut_doc = Document.getDoc(sUTdoc);
                ok = Document.IsDocExist(sUTdoc);
            }

            Assert.IsTrue(ok);

            // тут не надо делать тест с созданим Документа и его стиранием - 
            //..'это можно делать в другихUT_Document
            //Document doc = Document.getDoc(sINFO, create_if_notexist: true, fatal: false, reset: true);
            //    Assert.IsTrue(Document.IsDocExist(sINFO));
            //    doc.Close();
            //    //31/7                FileOp.Delete(doc.FileDirectory, name + ".xlsx");
            //    Assert.IsFalse(Document.IsDocExist(sINFO))        

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Start()
        {
            boot.Init();     
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
            boot.Init();
            string sIR = "InitialRules";

            var doc = Document.getDoc(sIR);

            Assert.AreEqual(sIR, doc.name);
            Assert.AreEqual(4, doc.i0);
            Assert.AreEqual(15, doc.il);

            FileOp.AppQuit();
        }
    }
}