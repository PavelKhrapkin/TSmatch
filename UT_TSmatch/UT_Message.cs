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
    }

    class UT_Msg : Msg
    {
        public int cnt_msgs() { return Msg._messages.Count; }
    }
}