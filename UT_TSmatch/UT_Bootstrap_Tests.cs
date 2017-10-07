/*================================
* Bootstrap Test 6.10.2017
*=================================
* Specific probel of this Unit Test for Bootstrap:
* It cannot run, only if all resesources loaded at startup time not available
*/
using match.FileOp;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSmatch.Bootstrap.Tests
{
    [TestClass()]
    public class UT_Bootstrap_Tests : Bootstrap
    {
        [TestMethod()]
        public void UT_Bootstrap()
        {
            var boot = new Bootstrap(init:true);

            Assert.IsTrue(boot.ModelDir.Length > 0);
            Assert.IsTrue(boot.TOCdir.Length > 0);
            Assert.IsNotNull(boot.docTSmatch);
            Assert.AreEqual(boot.docTSmatch.name, "TOC");
            Assert.IsTrue(boot.docTSmatch.il > 20);

            FileOp.AppQuit();
        }

// 6/10/17        public delegate string mes(Enum x, string str);

        [TestMethod()]
        public void UT_Boot_ResxErr()
        {
            var boot = new Bootstrap(init:false);
            var U = new UT_TSmatch._UT_Msg();
            U.SetCulture("en");

            // test 0: "en" NoFile, NoDoc, Obsolet, ErrResource
            try { resxError(ResErr.NoFile, "TSmatch.xlsx"); } catch { } U.GetTxt();
            Assert.AreEqual("[Bootstrap.resError]: Not found TSmatch file \"TSmatch.xlsx\"", U.msg);

            try { resxError(ResErr.NoDoc, "TOC"); } catch { } U.GetTxt();
            Assert.AreEqual("[Bootstrap.resError]: No TSmatch Resource Document \"TOC\"", U.msg);

            try { resxError(ResErr.Obsolete, "Forms"); } catch { } U.GetTxt();
            Assert.AreEqual("[Bootstrap.resError]: Resource \"Forms\" obsolete. Please, update it!", U.msg);

            try { resxError(ResErr.ErrResource, "TSmatch_xlsx"); } catch { } U.GetTxt();
            Assert.AreEqual("[Bootstrap.resError]: Internal Resource error. Resource \"TSmatch_xlsx\".", U.msg);

            // test 1: "ru" NoFile, NoDoc, Obsolet, ErrResource
            U.SetCulture("ru");
            try { resxError(ResErr.NoFile, "TSmatch.xlsx"); } catch { } U.GetTxt();
            Assert.AreEqual("[Bootstrap.resError]: Не найден файл \"TSmatch.xlsx\"", U.msg);

            try { resxError(ResErr.NoDoc, "TOC"); } catch { } U.GetTxt();
            Assert.AreEqual("[Bootstrap.resError]: Нет ресурсного документа \"TOC\"", U.msg);

            try { resxError(ResErr.Obsolete, "Forms"); } catch { } U.GetTxt();
            Assert.AreEqual("[Bootstrap.resError]: Ресурс \"Forms\" устарел. Пожалуйста обновите его!", U.msg);

            try { resxError(ResErr.ErrResource, "TSmatch_xlsx"); } catch { } U.GetTxt();
            Assert.AreEqual("[Bootstrap.resError]: Внутренняя ошибка ресурса \"TSmatch_xlsx\"", U.msg);
        }
    }
}