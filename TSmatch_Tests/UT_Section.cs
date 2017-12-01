/*=================================
* Section Unit Test 28.11.2017
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

            // test err 24/11/17: err: 
            s = new Sect("Ед: руб/т");
            Assert.AreEqual(SType.UNIT_Weight, s.type);
            Assert.AreEqual("", s.body);

            s = new Sect("Ед: за м ");
            Assert.AreEqual(SType.UNIT_Length, s.type);
            Assert.AreEqual("", s.body);

            s = new Sect("Ед: руб/м3 ");
            Assert.AreEqual(SType.UNIT_Vol, s.type);
            Assert.AreEqual("", s.body);

            s = new Sect("единица: ");
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");

            //----- error input text handling -----
            s = new Sect("Цена 2540");   // no ':'
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "цeнa2540");
            // то есть s.body заполняется, хотя заголовок секции не распознан

            s = new Sect("Цена 2540;");
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "цeнa2540");

            s = new Sect("; профиль: L");
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");

            //--- construtor Section("..;..;", SType) test 
            //           -> SType.Unit не находим, возвращаем SType.NOT_DEFINED
            s = new Sect(";проф: Sec; Mat: n", SType.Unit);
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");

            // из строки с секциями Материал и Профиль -> выбор второй секции 
            s = new Sect("m: c255=c245; пpoфиль: yгoлoк=l *x*;", SType.Profile);
            Assert.AreEqual("yгoлoк=l*x*", s.body);
            Assert.AreEqual(SType.Profile, s.type);

            // performatce test 10 000 циклов за < 1 сек, т.е. на Sect 100 микросекунд
            int cnt = 100000;
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

        [TestMethod()]
        // UT_SecType сделат исключительно для укорачивания пути отладки
        //..по сути, все проверяет UT_Section
        public void UT_SecType()
        {
            var s = new _Section();

            // test 1: "Ед:" -> SType.Unit
            var st = s._SecType("Ед: руб/т");
            Assert.AreEqual(SType.Unit, st);

            // test 2: "Объем:" -> SType.Volume
            st = s._SecType("Объем:");
            Assert.AreEqual(SType.VolPerUnit, st);
        }
    }

    public class _Section : Section 
    {
        public SType _SecType(string str)
        {
            return SecType(str);
        }
    }
}