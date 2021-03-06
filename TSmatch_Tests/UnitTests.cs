﻿/*----------------------------------------------------------------
 * Unit Test Module 
 *
 * 17.03.2017 Pavel Khrapkin
 *
 *--- History ---
 * 14.03.2017 Unit Test implemented
 * 16.03.2017 Section test-tried to make Oleg' advise with Biitstrap.initSection(),
 *            but return to static constructor in Section;
 * 17.03.2017 FingetPrtnt Test
 * ---- Tested Modules: ------------
 * Parameter    2017.03.14 OK
 * Section      2017.03.16 OK
 * FingerPrint  2017.03.17 частично - не все тесты перенесены из модуля TEST,
 *                         не распознается tx справа от параметра {n} - может и не надо??
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lib = match.Lib.MatchLib;
using UT_Par = TSmatch.Parameter.utp;
using UT_Sec = TSmatch.Section.Section;
using SType = TSmatch.Section.Section.SType;
using FP = TSmatch.FingerPrint.FingerPrint;
using FPtype = TSmatch.FingerPrint.FingerPrint.type;

namespace TSmatch.Unit_Tests
{
    [TestClass()]
    public class Parameter_Tests
    {
        [TestMethod()]
        public void UT_Parameter_Test()
        {
            var utp = new UT_Par("{4}");
            Assert.AreEqual(utp.type, "String");
            Assert.AreEqual(utp.tx, "");
            Assert.AreEqual(utp.par.ToString(), "4");

            utp = new UT_Par("Ab{123}cD");
            Assert.AreEqual(utp.type, "String");
            Assert.AreEqual((string)utp.par, "123");
            Assert.AreEqual(utp.tx, "ab");

            utp = new UT_Par("текст");
            Assert.AreEqual(utp.type, "String");
            Assert.AreEqual(utp.tx, Lib.ToLat("текст"));
            Assert.AreEqual((string)utp.par, utp.tx);

            utp = new UT_Par("x{3");
            Assert.AreEqual((string)utp.par, "x{3");
            Assert.AreEqual(utp.tx, (string)utp.par);

            utp = new UT_Par("def}fg");
            Assert.AreEqual((string)utp.par, "def}fg");
            Assert.AreEqual(utp.tx, (string)utp.par);

            utp = new UT_Par("Da{34{85}uy");
            Assert.AreEqual(utp.tx, "da");
            Assert.AreEqual((string)utp.par, "3485");  // поскольку внутреняя { стирается

            ///---- ParType test
            utp = new UT_Par("цена: {d~3}");
            Assert.AreEqual(utp.type, "Double");
            Assert.AreEqual((string)utp.par, "3");

            Assert.AreEqual(parType("{2}"), "String");
            Assert.AreEqual(parType("{s~2}"), "String");
            Assert.AreEqual(parType("{i~4}"), "Integer");
            Assert.AreEqual(parType("{d~3}"), "Double");
            Assert.AreEqual(parType("{digital~3}"), "Double");
            Assert.AreEqual(parType("текст{i~1}b{d~2,2}ff"), "Integer");
            Assert.AreEqual(parType("другой текст"), "String");
            Assert.AreEqual(parType(""), "String");

            //            Assert.Fail();
        }
        string parType(string str)
        {
            var utp = new UT_Par(str);
            return utp.type.ToString();
        }
    } // end class Parameter_Tests

    [TestClass()]
    public class Section_Test
    {
        [TestMethod()]
        public void UT_Section_Test()
        {
            new Bootstrap.Bootstrap.initSection();

            var s = new UT_Sec("hh");
            Assert.AreEqual(s.type, SType.NOT_DEFINED);

            s = new UT_Sec(string.Empty);
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");

            s = new UT_Sec("M: qq");
            Assert.AreEqual(s.type, SType.Material);
            Assert.AreEqual(s.body, "qq");

            s = new UT_Sec("Prf: x; price: y;");
            Assert.AreEqual(s.type, SType.Profile);
            Assert.AreEqual(s.body, "x");

            s = new UT_Sec("Описание: xx");
            Assert.AreEqual(s.type, SType.Description);
            Assert.AreEqual(s.body, "xx");

            s = new UT_Sec("длина: 5");
            Assert.AreEqual(s.type, SType.LengthPerUnit);
            Assert.AreEqual(s.body, "5");

            s = new UT_Sec("Объем: 7");
            Assert.AreEqual(s.type, SType.VolPerUnit);
            Assert.AreEqual(s.body, "7");

            s = new UT_Sec("вес: 77");
            Assert.AreEqual(s.type, SType.WeightPerUnit);
            Assert.AreEqual(s.body, "77");

            s = new UT_Sec("единица: ");
            Assert.AreEqual(s.type, SType.Unit);
            Assert.AreEqual(s.body, "");

            //----- error input text handling -----
            s = new UT_Sec("Цена 2540");   // no ':'
            Assert.AreEqual(s.type, SType.Price);
            Assert.AreEqual(s.body, "");

            s = new UT_Sec("Цена 2540;");
            Assert.AreEqual(s.type, SType.Price);
            Assert.AreEqual(s.body, "");

            s = new UT_Sec("; профиль: L");
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");

            //--- construtor Section("..;..;", SType) test
            s = new UT_Sec(";проф: Sec; Mat: n", SType.Profile);
            Assert.AreEqual(s.type, SType.Profile);
            Assert.AreEqual(s.body, "sec");

            s = new UT_Sec(";проф: Sec; Mat: n", SType.Unit);
            Assert.AreEqual(s.type, SType.NOT_DEFINED);
            Assert.AreEqual(s.body, "");
        }
    } // end class Section_Test

    [TestClass()]
    public class FingerPrint_Tests
    {
        [TestMethod()]
        public void UT_FP_constr1_Test()
        {
            // * тест: Уголок constructor1 FP(type.Rule, Проф: L * x *)
            FP xr2 = new FP(FPtype.Rule, "Проф: L * x *");
            Assert.AreEqual(xr2.section.type.ToString(), "Profile");
            Assert.AreEqual(xr2.pars.Count, 1);
            Assert.AreEqual(xr2.pars[0].ToString(), "l*x*");
            Assert.AreEqual(xr2.txs.Count, 0);

            // * Уголок с материалом constructor1 FP(type.Rule, Проф: L * x * cт *)");
            xr2 = new FP(FPtype.Rule, "Проф: L * x * cт *");
            Assert.AreEqual(xr2.section.type.ToString(), "Profile");
            Assert.AreEqual(xr2.pars.Count, 1);
            Assert.AreEqual(xr2.pars[0].ToString(), "l*x*cт*");
            Assert.AreEqual(xr2.txs.Count, 0);

            // * тест: Бетон -монолит constructor1 FP(type.Rule, М:В*)");
            FP rule = new FP(FPtype.Rule, "M:B*;");
            Assert.AreEqual(rule.pars.Count, 1);
            Assert.AreEqual(rule.typeFP.ToString(), "Rule");
            Assert.AreEqual(rule.section.type.ToString(), "Material");
            Assert.AreEqual(rule.pars[0], "b*");
            Assert.AreEqual(rule.txs.Count, 0);

            xr2 = new FP(FPtype.Rule, "Профиль:");
            Assert.AreEqual(xr2.section.type.ToString(), "Profile");

            FP xr1 = new FP(FPtype.CompSet, "Описание: {3}");
            Assert.AreEqual(xr1.pars.Count, 1);
            Assert.AreEqual(xr1.typeFP.ToString(), "CompSet");
            Assert.AreEqual(xr1.section.type.ToString(), "Description");
            Assert.AreEqual(xr1.Col(), 3);

            //-- CompSet tests
            xr1 = new FP(FPtype.CompSet, "Цена: {4} если нет другого материала в описании");
            Assert.AreEqual(xr1.pars.Count, 1);
            Assert.AreEqual(xr1.typeFP.ToString(), "CompSet");
            Assert.AreEqual(xr1.section.type.ToString(), "Price");
            Assert.AreEqual(xr1.Col(), 4);
            Assert.AreEqual(xr1.txs.Count, 2);
            Assert.AreEqual(xr1.txs[0], "");
//13/3            Assert.AreEqual(xr1.txs[1].Length > 10, true); - не распознается остаток строки справа!!

            xr1 = new FP(FPtype.CompSet, "Цена: {4} НДС {12}{14}%");
            Assert.AreEqual(xr1.pars.Count, 3);
            //13/3            Assert.AreEqual(xr1.txs[3], "%");      -- то же
            Assert.AreEqual(xr1.Col(2), 14);
            Assert.AreEqual(xr1.txs.Count, 4);
            Assert.AreEqual(xr1.typeFP.ToString(), "CompSet");
            Assert.AreEqual(xr1.section.type.ToString(), "Price");
            Assert.AreEqual(xr1.Col(), 4);
        }
    }
} // end namespace TSmatch.Unit_Tests