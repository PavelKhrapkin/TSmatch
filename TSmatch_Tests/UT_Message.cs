/*=================================
* Message Unit Test 9.10.2017
*=================================
* History:
*  8.10.2017 - no static in Message
*  9.10.2017 - Msg.F
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            int cnt = U.cnt_msgs();
            Assert.IsTrue(cnt > 10);

            // test 1: Msg.txt
            U._txt("число {0} и {1}", 3.14, 2.7);
            Assert.AreEqual("число_3,14_и_2,7", U.GetMsg());
            Assert.AreEqual("(*)TSmatch INFO", U.GetErrType());

            // test 2: change culture to en
            U.SetCulture("en");

            string s = U.Msg_S("S");
            Assert.AreEqual("Loading", s);

            // test 3: return culture to ru
            U.SetCulture("ru");
            s = U.Msg_S("S");
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
            string s = Msg.SS("Not Initialized Message");
            Assert.AreEqual(s, "(*)TSmatch SPLASH Not_Initialized_Message");

            // test 1: нормальный вывод сообщения по русски
            s = Msg.SS("SectionTab_is_empty");
            Assert.AreEqual(s, "[Section]: не инициализирован словарь секций SectionTab");

            // test 2: вывод неизвестного сообщения
            s = Msg.SS("тра-ля-ля", "и маленькая тележка");
            Assert.AreEqual("(*)TSmatch SPLASH тра-ля-ля", s);

            // test 3: вывод сообщения, которое есть, но с отсутствующим параметром
            s = Msg.SS("Bootstrap__resError_NoFile");
            Assert.AreEqual(s, "(!)TSmatch SPLASH [Bootstrap.resError]: Не найден файл \"{0}\"");

            // test 4: распознавание сообщения с пробелами, замена их на '_'
            s = Msg.SS("SectionTab is empty");
            Assert.AreEqual("[Section]: не инициализирован словарь секций SectionTab", s);
        }

        [TestMethod()]
        public void UT_W()
        {
            // test 0: Dialog = false - работаем без остановки
            var U = new UT_TSmatch._UT_MsgService();
            U.Msg_W("text for test");
            Assert.AreEqual("text_for_test", U.GetMsg());

            // test 1: message should be modal -- должен спрашивать [OK?]
            // --!!-- этот тест закомментирован. Открывать комментарии только для проверки Msg.W вручную
            //Msg.Dialog = true;
            //Msg.W("Should be modal");
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