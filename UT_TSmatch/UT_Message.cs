/*=================================
* Message Unit Test 3.10.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Msg = TSmatch.Message.Message;

namespace TSmatch.Message.Tests
{
    [TestClass()]
    public class UT_Message
    {
        [TestMethod()]
        public void UT_init()
        {
            // тест 0: проверяем singleton initialisation
            var U = new UT_TSmatch._UT_Msg();
            int cnt = U.cnt_msgs();
            Assert.IsTrue(cnt > 10);

            // test 1: Msg.txt
            U._txt("число {0} и {1}", 3.14, 2.7);
            U.GetTxt();
            Assert.AreEqual("число_3,14_и_2,7", U.msg);
            Assert.AreEqual("(*)TSmatch INFO", U.errType);

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
            Msg.Dialog = true;  //наличие этой строки связано с использованием static в Msg - портится в тестах
            // test 0: Message не инициализирован, сообщение "незнакомое"
            string s = Msg.S("Not Initialized Message");
            Assert.AreEqual(s, "(*)TSmatch SPLASH Not_Initialized_Message");

            // test 1: нормальный вывод сообщения по русски
            s = Msg.S("SectionTab_is_empty");
            Assert.AreEqual(s, "[Section]: не инициализирован словарь секций SectionTab");

            // test 2: вывод неизвестного сообщения
            s = Msg.S("тра-ля-ля", "и маленькая тележка");
            Assert.AreEqual("(*)TSmatch SPLASH тра-ля-ля", s);

            // test 3: вывод сообщения, которое есть, но с отсутствующим параметром
            s = Msg.S("Bootstrap__No_Resource_File");
            Assert.AreEqual(s, "(*)TSmatch SPLASH Bootstrap__No_Resource_File");

            // test 4: распознавание сообщения с пробелами, замена их на '_'
            s = Msg.S("SectionTab is empty");
            Assert.AreEqual("[Section]: не инициализирован словарь секций SectionTab", s);
        }

        [TestMethod()]
        public void UT_W()
        {
            // test 0: Dialog = false - работаем без остановки
            var U = new UT_TSmatch._UT_Msg();
            //           Assert.ThrowsException()
            U.Msg_W("text for test");
            U.GetTxt();
            Assert.AreEqual("text_for_test", U.msg);

            // test 1: message should be modal -- должен спрашивать [OK?]
            // --!!-- этот тест закомментирован. Открывать комментарии только для проверки Msg.W вручную
            //Msg.Dialog = true;
            //Msg.W("Should be modal");
        }
    }
}