/*----------------------------------------------------------------
 * Unit Test Module 
 *
 * 14.03.2017 Pavel Khrapkin
 *
 *--- History ---
 * 14.03.2017 Unit Test implemented
 * ---<ToDo>---------
 * 14.3.17 написать тесты Section
 * ---- Tested Modules: ------------
 * Parameter    2017.03.14 OK
 */
 using Microsoft.VisualStudio.TestTools.UnitTesting;
using UT_Par = TSmatch.Parameter.utp;
using UT_Sec = TSmatch.Section.uts;
using Lib = match.Lib.MatchLib;

namespace TSmatch.Tests
{
    [TestClass()]
    public class UT_ParameterTests
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

            //                Assert.Fail();
        }
        string parType(string str)
        {
            var utp = new UT_Par(str);
            return utp.type.ToString();
        }
    }

    [TestClass()]
    public class SectionTests
    {
        [TestMethod()]
        public void UT_Section_Test()
        {
            var uts = new UT_Sec("ff");
            int i = 223;
            Assert.AreEqual(i, 223);
//            Assert.Fail();
        }
    }
}