/*=================================
* Message Unit Test 12.11.2017
*=================================
* History:
*  8.10.2017 - no static in Message
*  9.10.2017 - Msg.F
* 25.10.2017 - Msg.AskOK, AskFok, AskYN
* 10.11.2017 - no static and refactoring in Message
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TSmatch.Message.Tests
{
    [TestClass()]
    public class UT_Message
    {
        [TestMethod()]
        public void UT_init()
        {
            // тест 0: проверяем singleton initialisation
            var U = new UT_TSmatch._UT_MsgService();
            U.SetLanguage("ru");
            int cnt = U.cnt_msgs();
            Assert.IsTrue(cnt > 10);

            // test 1: Msg.S ru not known
            string s = U.S("число {0} и {1}", 3.14, 2.7);
            Assert.AreEqual("число_3,14_и_2,7", uS(s));

            // test 2: change culture to en, known
            U.SetLanguage("en");
            s = U.S("S");
            Assert.AreEqual("Loading", s);

            // test 3: return culture to ru
            U.SetLanguage("ru");
            s = U.S("S");
            Assert.AreEqual("Гружу", s);
        }

        [TestMethod()]
        public void UT_SetLanguage()
        {
            var Msg = new Message();

            // test 1: "en"
            Msg.SetLanguage("en");
            string s = Msg.S("S");
            Assert.AreEqual("Loading", s);

            // test 2: return culture to ru
            Msg.SetLanguage("ru");
            s = Msg.S("S");
            Assert.AreEqual("Гружу", s);
        }


        //[TestMethod()]
        //public void UT_AskS()
        //{
        //    // 2017.09.13 - AskS еще не реализован - заглушка в Message и тут в UT
        //    string reply = Msg.AskS("редактируем текст:", "text examle");
        //    Assert.IsNull(reply);
        //}

        [TestMethod()]
        public void UT_S()
        {
            var Msg = new Message();

            // test 0: Message не инициализирован, сообщение "незнакомое"
            string s = Msg.S("Not Initialized Message");
            Assert.AreEqual(uS(s), "Not_Initialized_Message");

            // test 1: нормальный вывод сообщения по русски
            Msg.SetLanguage("ru");
            s = Msg.S("MainWindow__RePrice");
            Assert.AreEqual(s, "Пересчет стоимости материалов");

            // test 2: вывод неизвестного сообщения
            s = Msg.S("тра-ля-ля", "и маленькая тележка");
            Assert.AreEqual("тра-ля-ля", uS(s));

            // test 3: вывод сообщения, которое есть, но с отсутствующим параметром
            s = Msg.S("Bootstrap__resError_NoFile");
            Assert.AreEqual(s, "(!)TSmatch STRING [Bootstrap.resError]: Не найден файл \"{0}\"");

            // test 4: распознавание сообщения с пробелами, замена их на '_'
            s = Msg.S("WPF_MainWindow_grPrf");
            Assert.AreEqual("Профиль", s);
        }

        internal string uS(string str)
        {
            string prefix = "(*)TSmatch STRING ";
            int i0 = prefix.Length;
            return str.Substring(i0);
        }

        [TestMethod()]
        public void UT_W_I_F()
        {
            // test 0: in_UT, not known message
            var U = new UT_TSmatch._UT_MsgService();
            U.SetLanguage("en");
            U._W("text for test");
            Assert.AreEqual("text_for_test", U.GetMsg());

            // test 1: W(str, parameters)
            U._W("Bootstrap__resError_NoFile", 3.1415);
            Assert.AreEqual("[Bootstrap.resError]: Not found TSmatch file \"3,1415\"", U.GetMsg());

            // test 2: I(str, parameters)
            U.SetLanguage("ru");
            U._I("Bootstrap__resError_NoDoc", "какой-то.doc");
            Assert.AreEqual("[Bootstrap.resError]: Нет ресурсного документа \"какой-то.doc\"", U.GetMsg());

            // test 3: F(str, parameters)
            U._F("Bootstrap__resError_Obsolete", "город");
            Assert.AreEqual("[Bootstrap.resError]: Ресурс \"город\" устарел. Пожалуйста обновите его!", U.GetMsg());
        }

        [TestMethod()]
        public void UT_AskYN()
        {
            // test 0: Dialog = false - работаем без остановки
            var U = new UT_TSmatch._UT_MsgService();
            try { U.AskYN("text for test"); }
            catch (ArgumentException e) when (e.Message == "Msg.I: text_for_test") { };
        }

        [TestMethod()]
        public void UT_AskOK()
        {
            // test 0: Dialog = false - работаем без остановки
            var U = new UT_TSmatch._UT_MsgService();
            U.SetLanguage("");
            try { U.AskOK("text for test"); }
            catch (ArgumentException e) when (e.Message == "Msg.I: text_for_test") { }

            // test 1: Dialog = false - работаем без остановки; AskFOK()
            string exCode = string.Empty;
            try { U.AskFOK("text for test 2"); }
            catch (ArgumentException e) { exCode = e.Message; }
            Assert.AreEqual("Msg.I: text_for_test_2", exCode);
        }

#if NOT_MADE_YET
        [TestMethod()]
        public void UT_F()
        {
            // test 0: перехват Exception Msg.F
            var U = new UT_TSmatch._UT_MsgService();
            U.FF()
            U.Msg_W("text for test");
            Assert.AreEqual("text_for_test", U.GetMsg());

            // test 1: message should be modal -- должен спрашивать [OK?]
            // --!!-- этот тест закомментирован. Открывать комментарии только для проверки Msg.W вручную
            //Msg.Dialog = true;
            //Msg.W("Should be modal");
        }
#endif
    }
}