/*================================
* Bootstrap Test 9.10.2017
*=================================
*/
using match.FileOp;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSmatch.Bootstrap.Tests
{
    [TestClass()]
    public class UT_Bootstrap_Tests
    {
        _Bootstrap boot = new _Bootstrap();
        UT_TSmatch._UT_MsgService U = new UT_TSmatch._UT_MsgService();

        [TestMethod()]
        public void UT_Bootstrap()
        {
            boot.Init();

            Assert.IsTrue(boot.ModelDir.Length > 0);
            Assert.IsTrue(boot.TOCdir.Length > 0);
            Assert.IsNotNull(boot.docTSmatch);
            Assert.AreEqual(boot.docTSmatch.name, "TOC");
            Assert.IsTrue(boot.docTSmatch.il > 20);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Boot_ResxErr()
        {
            U.SetCulture("en");

            // test 0: "en" - English
            string s = boot._rErr(1, "TSmatch.xlsx");   // 1 - ResErr.NoFile - нет ресурсного файла
            Assert.AreEqual("[Bootstrap.resError]: Not found TSmatch file \"TSmatch.xlsx\"", s);

            s = boot._rErr(2, "TOC");                   // 2 - ResErr.NoDoc - нет ресурсного документа
            Assert.AreEqual("[Bootstrap.resError]: No TSmatch Resource Document \"TOC\"", s);

            s = boot._rErr(3, "Forms");                 // 3 - ResErr.Obsolete - ресурс устарел
            Assert.AreEqual("[Bootstrap.resError]: Resource \"Forms\" obsolete. Please, update it!", s);

            s = boot._rErr(0, "Something");             // 0 - ResErr.ErrResource - нет такого ресурса 
            Assert.AreEqual("[Bootstrap.resError]: Internal Resource error. Resource \"Something\".", s);

            // test 1: "ru" - Russian
            U.SetCulture("ru");

            s = boot._rErr(1, "TSmatch.xlsx");          // 1 - ResErr.NoFile - нет ресурсного файла
            Assert.AreEqual("[Bootstrap.resError]: Не найден файл \"TSmatch.xlsx\"", s);

            s = boot._rErr(2, "TOC");                   // 2 - ResErr.NoDoc - нет ресурсного документа
            Assert.AreEqual("[Bootstrap.resError]: Нет ресурсного документа \"TOC\"", s);

            s = boot._rErr(3, "Forms");                 // 3 - ResErr.Obsolete - ресурс устарел
            Assert.AreEqual("[Bootstrap.resError]: Ресурс \"Forms\" устарел. Пожалуйста обновите его!", s);

            s = boot._rErr(0, "Something");             // 0 - ResErr.ErrResource - нет такого ресурса
            Assert.AreEqual("[Bootstrap.resError]: Внутренняя ошибка ресурса \"Something\"", s);
        }


        /// <summary>
        /// Перехват сообщений о фатальных ошибках в Msg.F
        /// </summary>
        public class _Bootstrap : Bootstrap
        {
            public string _rErr(int type, string str)
            {
                string result = "";
                ResErr errType = (ResErr)type;
                if (!Enum.IsDefined(typeof(ResErr), errType))
                    Assert.Fail("enum Bootstrap.ResErr=" + type + " not defined");
                try { resxError(errType, str); }
                catch (Exception e)
                {
                    const string prefix = "Msg.F: ";
                    if (e.Message.IndexOf(prefix) != 0) Assert.Fail();
                    result = e.Message.Substring(prefix.Length);
                }
                return result;
            }
        }
    }
}