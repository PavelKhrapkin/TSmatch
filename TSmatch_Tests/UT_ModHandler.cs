/*=================================
 * Model.Handler Unit Test 4.6.2017
 *=================================
 */
 using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Model.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSmatch.Model.Handler.Tests
{
    [TestClass()]
    public class UT_ModHandler
    {
        [TestMethod()]
        public void UT_ModHandler_PrfUpdate()
        {
            var mod = new ModHandler();
            ElmAttSet.Group gr = new ElmAttSet.Group();

            // test 1: "—100*6" => "—6x100"
            gr.Prf = gr.prf = "—100*6";
            mod.elmGroups.Add(gr);
            mod.PrfUpdate();
            var v = mod.elmGroups[0].prf;
            Assert.AreEqual(v, "—6x100");

            // test 2: "—100*6" => "—6x100"
            gr.Prf = gr.prf = "—100*6";
            mod.elmGroups.Add(gr);
            mod.PrfUpdate();
            v = mod.elmGroups[1].prf;
            Assert.AreEqual(v, "—6x100");

            // test 3: "L75X5_8509_93" => L75x5"
            gr.Prf = gr.prf = "L75X5_8509_93";
            mod.elmGroups.Add(gr);
            mod.PrfUpdate();
            v = mod.elmGroups[2].prf;
            Assert.AreEqual(v, "L75x5");
        }
    }
}