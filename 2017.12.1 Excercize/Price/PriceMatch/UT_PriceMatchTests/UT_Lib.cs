/* --------------------------------------------------------
 * UT Lib 20.02.2018 Pavel Khrapkin
 * --------------------------------------------------------
 * UT_ToLat     тест перевода строки в латынь
 * UT_GetMD5    проверка подсчета MD5
 * UT_WrXML     тест записи в xml файл
 * UT_RdXML     тест чтения MD5
 * UT_IdGen     проверка генератора Id lkz для разных классов
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace PriceMatch.Tests
{
    [TestClass()]
    public class UT_Lib
    {
        private Lib Lib = new Lib();
        FileOp fo = new FileOp();
        private string path;
        private string file = "UT_Lib";

        [TestInitialize]
        public void Initialyze()
        {
            string desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path = Path.Combine(desktop_path, @"$DebugDir\UT");
        }

        [TestMethod()]
        public void UT_ToLat()
        {
            string str = "мой текст";
            string lat = Lib.ToLat(str);
            Assert.AreEqual("мoй тeкcт", lat);

            //speed test
            DateTime t0 = DateTime.Now;
            for (int i = 0; i < 1000000; i++)
            {
                lat = Lib.ToLat(str + " " + i);
            }
            DateTime t1 = DateTime.Now;

            TimeSpan mlnCycles = t1 - t0;
        }

        [TestMethod()]
        public void UT_GetMD5()
        {
            // test 0: MD5(null) => ArgumentNullException
            try
            {
                Lib.GetMD5(null);
                Assert.Fail("No ArgumentNullException generated");
            }
            catch { }

            // test 1: MD5("ABCD") => "9D7F395A90D9571F884F5192BB1AAC42"
            string x = Lib.GetMD5("ABCD");
            Assert.AreEqual("9D7F395A90D9571F884F5192BB1AAC42", x);

            // test 2: twice calculated MD5 get same result
            string str = "ABCD";
            string y = Lib.GetMD5(str);
            Assert.AreEqual(x, y);

            // test 3: speed test 10 000 => 100 ms
            DateTime t0 = DateTime.Now;
            const int N = 10000;
            string str0 = "ABCD";
            for (int i = 0; i < N; i++)
            {
                str = str0 + i % 25 + 'A';
                x = Lib.GetMD5(str);
            }
            DateTime t1 = DateTime.Now;
            TimeSpan dt = t1 - t0;
        }

        [TestMethod()]
        public void UT_WrXML()
        {
            // test 0: write XML file with desktop_path string
            string desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = Path.Combine(desktop_path, @"$DebugDir\UT");
            file += "_XML";
            Lib.WrXML(path, file, desktop_path);

            // test 1: wtite List<string>
            List<string> lst = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                lst.Add("A" + i);
            }
            string filePath = Path.Combine(path, file);
            Lib.WrXML(filePath, lst);
            var v = Lib.RdXML<List<string>>(filePath);
            Assert.AreEqual(v.Count, lst.Count);
            for (int i = 0; i < v.Count; i++)
            { Assert.AreEqual(v[i], lst[i]); }

            // test 2: Append xml Write chech
            Lib.WrXML(filePath, lst);
            Lib.WrXML(filePath, lst);
            v = Lib.RdXML<List<string>>(filePath);
            Assert.AreEqual(v.Count, lst.Count);
            for (int i = 0; i < v.Count; i++)
            { Assert.AreEqual(v[i], lst[i]); }

            // test 3: Read/Write Supplier.xml
            filePath = Path.Combine(desktop_path, "$DebugDir", "Prices", "Suppliers.xml");
            if(fo.IsFileExist(filePath))
            {
                var ss = Lib.RdXML<Suppliers>(filePath);
                int cnt = ss.AllSuppliers.Count;
                Lib.WrXML(filePath, ss);
                Lib.WrXML(filePath, ss);
                var xx = Lib.RdXML<Suppliers>(filePath);
                Assert.AreEqual(ss.AllSuppliers.Count, xx.AllSuppliers.Count);
            }
        }

        [TestMethod()]
        public void UT_RdXML()
        {
            var fo = new FileOp();
            if (!fo.IsFileExist(path, file)) UT_WrXML();
            var lst = new List<string>();

            //test 0: read xml with List<string>
            var x = Lib.RdXML<List<string>>(Path.Combine(path, file));
            Assert.AreEqual(10, x.Count);
            Assert.AreEqual("A5", x[5]);
        }

        [TestMethod()]
        public void UT_IdGen()
        {
            // test 0: IdGen(null) 
            string id = Lib.IdGen(null);
            Assert.AreEqual(5, id.Length);

            // test 1: IdGen for InputPO and Rule
            string id1 = Lib.IdGen(Declaration.IdTypeItemPO);
            string id2 = Lib.IdGen(Declaration.IdTypeItemPO);
            Assert.IsTrue(id1 != id2);
            Assert.AreEqual(id1.Length, id2.Length);
            Assert.AreEqual(id1.Length, Declaration.IdTypeItemPO.Length + 5);
            string id3 = Lib.IdGen(Declaration.IdTypeRule);
            Assert.IsFalse(id3 == id1);
            Assert.IsFalse(id3.Length == id2.Length);
            Assert.AreEqual(id3.Length, Declaration.IdTypeRule.Length + 5);
        }
    } // end UT_LibTests class
} // end namespace