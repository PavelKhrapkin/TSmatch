/*=================================
* Message Unit Test 13.9.2017
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
            Msg.txt("число {0} and {1}", 3.14, 2.7);
            U.GetTxt();
            Assert.AreEqual("число_3,14_and_2,7", U.msg);
            Assert.AreEqual("(*)TSmatch INFO", U.errType);

            // test 2: change culture to en
            U.SetCulture("en");
            string s = Msg.S("S");
            Assert.AreEqual("Loading", s);

            // test 3: return culture to ru
            U.SetCulture("ru");
            s = Msg.S("S");
            Assert.AreEqual("Гружу", s);
        }

        [TestMethod()]
        public void UT_AskS()
        {
            // 2017.09.13 - AskS еще не реализован - заглушка в Message и тут в UT
            string reply = Msg.AskS("редактируем текст:", "text examle");
            Assert.IsNull(reply);
        }

        [TestMethod()]
        public void UT_S()
        {
            // test 0: Message не инициализирован, сообщение "незнакомое"
            string s = Msg.S("Not Initialized Message");
            Assert.AreEqual(s, "(*)TSmatch SPLASH Not_Initialized_Message");

            // test 1: нормальный вывод сообщения по русски
            s = Msg.S("Bootstrap__No_Resource_File", "нечто");
            Assert.AreEqual(s, "[Bootstrap]: Нет ресурсного файла \"нечто\"");

            // test 2: вывод неизвестного сообщения
            s = Msg.S("тра-ля-ля", "и маленькая тележка");
            Assert.AreEqual("(*)TSmatch SPLASH тра-ля-ля", s);

            // test 3: вывод сообщения, которое есть, но с отсутствующим параметром
            s = Msg.S("Bootstrap__No_Resource_File");
            Assert.AreEqual(s, "(!)TSmatch SPLASH [Bootstrap]: Нет ресурсного файла \"{0}\"");

            // test 4: распознавание сообщения с пробелами, замена их на '_'
            s = Msg.S("SectionTab is empty");
            Assert.AreEqual("[Section]: не инициализирован словарь секций SectionTab", s);
        }

        [TestMethod()]
        public void UT_W()
        {
            // test 0: Dialog = false - работаем без остановки
            Msg.Dialog = false;
            Msg.W("text");
            Assert.IsTrue(true);

            // test 1: message should be modal -- должен спрашивать [OK?]
            Msg.Dialog = true;
            Msg.W("Should be modal");
        }
    }
}