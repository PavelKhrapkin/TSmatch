/*=================================
* Message Unit Test 22.8.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Boot = TSmatch.Bootstrap.Bootstrap;
using FileOp = match.FileOp.FileOp;
using Msg = TSmatch.Message.Message;

namespace TSmatch.Message.Tests
{
    [TestClass()]
    public class UT_Message
    {
        [TestMethod()]
        public void UT_init()
        {
            var boot = new Boot();

            // тест 0: проверяем singleton initialisation
            var U = new UT_Msg();
            int cnt = U.cnt_msgs();
            Assert.IsTrue(cnt > 10);

            // test 1: Msg.txt
            Msg.txt("число {0} and {1}", 3.14, 2.7);
            string tx = Msg.msg, ert = Msg.errType;
            Assert.AreEqual("число 3,14 and 2,7", Msg.msg);
            Assert.AreEqual("(*)TSmatch INFO", Msg.errType);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_AskS()
        {
            var boot = new Boot();

            string reply = Msg.AskS("редактируем текст:", "text examle");
            //22/8           Assert.Fail();

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_S()
        {
            // test 0: Message не инициализирован, сообщение "незнакомое"
            string s = Msg.S("Not Initialized Message");
            Assert.AreEqual(s, "(*)TSmatch SPLASH Not Initialized Message");

            // test 1: нормальный вывод сообщения по русски
            var boot = new Boot();
            s = Msg.S("No TSmatch Resource file", "нечто");
            Assert.AreEqual(s, "Нет ресурсного файла \"нечто\"");

            // test 2: вывод сообщения, которое есть, но с отсутствующим параметром
            s = Msg.S("No TSmatch Resource file");
            Assert.AreEqual(s, "(!)TSmatch SPLASH No TSmatch Resource file");

            FileOp.AppQuit();
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

    class UT_Msg : Msg
    {
        public int cnt_msgs() { return Msg._messages.Count; }
    }
}