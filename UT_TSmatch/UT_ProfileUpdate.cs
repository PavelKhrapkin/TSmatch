/*=================================
 * ProfileUpdate Unit Test 1.07.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.ProfileUpdate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Handler = TSmatch.Model.Handler.ModHandler;
using ElmGr = TSmatch.ElmAttSet.Group;
using Lib = match.Lib.MatchLib;
using TSmatch.ElmAttSet;

namespace TSmatch.ProfileUpdate.Tests
{
    [TestClass()]
    public class UT_ProfileUpdate
    {
        Handler mod = new Handler();
        ElmAttSet.Group gr = new ElmAttSet.Group();
        List<ElmGr> inp = new List<ElmGr>();


        private void UT_ProfileUpdate_I()
        {
            // test 0: "I10_8239_89" => "I10"
            initGr("I10_8239-89");
            var res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "i10");
            Assert.AreEqual(res[0].Prf, "I10");

            // test 1: "I20B1_20_93" => I20"
            inp.Clear();
            gr.Prf = "I20B1_20_93";
            gr.prf = "i20b1_20_93";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "i20б1");
            Assert.AreEqual(res[0].Prf, "I20Б1");

            // test 2: "I20B2_20_93" => I20"
            inp.Clear();
            gr.Prf = "I20B2_20_93";
            gr.prf = "i20b2_20_93";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "i20б2");
            Assert.AreEqual(res[0].Prf, "I20Б2");

            // test 3: "I50B3_20_93" => I20"
            inp.Clear();
            gr.Prf = "I50B3_20_93";
            gr.prf = "i50b3_20_93";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "i50б3");
            Assert.AreEqual(res[0].Prf, "I50Б3");
        }

        [TestMethod()]
        public void UT_ProfileUpdate_check()
        {
            // test 0: "PL6" -> "—6"
            gr.Prf = "PL6";
            gr.prf = "pl6";
            inp.Add(gr);
            var res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "—6");

            // test 1: "—100*6" => "—6x100"
            inp.Clear();
            gr.Prf = gr.prf = "—100*6";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "—6x100");

            // test 2: "—100*6" => "—6x100"
            gr.Prf = gr.prf = "—100*6";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[1].prf, "—6x100");

            // test 3: "L75X5_8509_93" => L75x5"
            gr.Prf = gr.prf = "L75X5_8509_93";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[2].Prf, "L75x5");
            Assert.AreEqual(res[2].prf, "l75x5");
        }

        private void Test_U(Handler mod, ElmGr gr)
        {
            List<ElmGr> inp = new List<ElmGr>();

        }

        // 2017.06.29 тест двутавров
        [TestMethod()]
        public void UT_PrfUpdate_I()
        {
            var mod = new Handler();
            ElmAttSet.Group gr = new ElmAttSet.Group();
            List<ElmGr> inp = new List<ElmGr>();

            // test 0: "I10_8239_89" => "I10"
            gr.Prf = "I10_8239-89";
            gr.prf = "i10_8239-89";
            inp.Add(gr);
            var res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "i10");
            Assert.AreEqual(res[0].Prf, "I10");

            // test 1: "I20B1_20_93" => I20"
            inp.Clear();
            gr.Prf = "I20B1_20_93";
            gr.prf = "i20b1_20_93";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "i20б1");
            Assert.AreEqual(res[0].Prf, "I20Б1");

            // test 2: "I20B2_20_93" => I20"
            inp.Clear();
            gr.Prf = "I20B2_20_93";
            gr.prf = "i20b2_20_93";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "i20б2");
            Assert.AreEqual(res[0].Prf, "I20Б2");

            // test 3: "I50B3_20_93" => I20"
            inp.Clear();
            gr.Prf = "I50B3_20_93";
            gr.prf = "i50b3_20_93";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "i50б3");
            Assert.AreEqual(res[0].Prf, "I50Б3");
        }

        private void initGr(string v)
        {
            inp.Clear();
            gr.Prf = v;
            gr.prf = Lib.ToLat(v.ToLower().Replace(" ", ""));
            inp.Add(gr);
        }
    } // end class UT_ProfileUpdate
} // end namespace TSmatch.ProfileUpdate.Tests