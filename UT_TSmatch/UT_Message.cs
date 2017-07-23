/*=================================
 * Message Unit Test 29.5.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //var msg = new Message();
            //msg.Init();

            int cnt = Msg.msgs.Count;
            Assert.IsTrue(cnt > 10);
            Msg.txt("число {0} and {1}", 3.14, 2.7);
            string tx = Msg.msg, ert = Msg.errType;
            Assert.AreEqual("число 3,14 and 2,7", Msg.msg);
            Assert.AreEqual("(*)TSmatch INFO", Msg.errType);

            FileOp.AppQuit();
        }
    }
}