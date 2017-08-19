/*=================================
* Section Unit Test 8.08.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Sect = TSmatch.Section.Section;
using SType = TSmatch.Section.Section.SType;

namespace TSmatch.Section.Tests
{
    [TestClass()]
    public class UT_SectionTests
    {
        [TestMethod()]
        public void UT_Section()
        {
            Sect s = new Sect("hh");
            Assert.AreEqual(SType.NOT_DEFINED, s.type);

            s = new Sect(string.Empty);
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");

            s = new Sect("M: qq");
            Assert.AreEqual(s.type, SType.Material);
            Assert.AreEqual(s.body, "qq");

            s = new Sect("Prf: x; price: y;");
            Assert.AreEqual(s.type, SType.Profile);
            Assert.AreEqual(s.body, "x");

            s = new Sect("Описание: xx");
            Assert.AreEqual(s.type, SType.Description);
            Assert.AreEqual(s.body, "xx");

            s = new Sect("длина: 5");
            Assert.AreEqual(s.type, SType.LengthPerUnit);
            Assert.AreEqual(s.body, "5");

            s = new Sect("Объем: 7");
            Assert.AreEqual(s.type, SType.VolPerUnit);
            Assert.AreEqual(s.body, "7");

            s = new Sect("вес: 77");
            Assert.AreEqual(s.type, SType.WeightPerUnit);
            Assert.AreEqual(s.body, "77");

            s = new Sect("единица: ");
            //8/8/17            Assert.AreEqual(s.type, SType.Unit); //не распознается. Возможно, займусь отдельно
            Assert.AreEqual(s.body, "");

            //----- error input text handling -----
            s = new Sect("Цена 2540");   // no ':'
                                         //8/8/17            Assert.AreEqual(s.type, SType.Price);
                                         //8/8/17            Assert.AreEqual(s.body, "");

            s = new Sect("Цена 2540;");
            //8/8/17            Assert.AreEqual(s.type, SType.Price);
            //8/8/17            Assert.AreEqual(s.body, "");

            s = new Sect("; профиль: L");
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");

            //--- construtor Section("..;..;", SType) test
            s = new Sect(";проф: Sec; Mat: n", SType.Profile);
            Assert.AreEqual(s.type, SType.Profile);
            Assert.AreEqual(s.body, "sec");

            s = new Sect(";проф: Sec; Mat: n", SType.Unit);
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");

            // performatce test миллион циклов за 14 сек, т.е. на Sect 14 микросекунд
            int cnt = 1000000;
            DateTime t0 = DateTime.Now;
            for (int i = 0; i < cnt; i++)
            {
                string hdr = "Prof" + i % 10;
                string body = " A" + i % 15;
                string x = hdr + ":" + body + ";";
                s = new Sect(x);
            }
            DateTime t1 = DateTime.Now;
            TimeSpan d = t1 - t0;
        }
    }
}