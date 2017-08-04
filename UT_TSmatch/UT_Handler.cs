/*=================================
 * Handler Unit Test 3.8.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

using FileOp = match.FileOp.FileOp;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Mod = TSmatch.Model.Model;
using TS = TSmatch.Tekla.Tekla;
using MH = TSmatch.Handler.Handler;
using SR = TSmatch.SaveReport.SavedReport;
using Elm = TSmatch.ElmAttSet.ElmAttSet;

namespace TSmatch.Handler.Tests
{
    [TestClass()]
    public class UT_Handler
    {
#if OLD //3.7/17
        [TestMethod()]
        public void UT_ModHandler_PrfUpdate()
        {
            var mod = new ModHandler();
            ElmAttSet.Group gr = new ElmAttSet.Group();
            List<ElmGr> inp = new List<ElmGr>();

            Test_I(mod, gr);
            Test_U(mod, gr);

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

        private void Test_I(ModHandler mod, ElmGr gr)
        {
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

        private void Test_U(ModHandler mod, ElmGr gr)
        {
            List<ElmGr> inp = new List<ElmGr>();

        }

        // 2017.06.29 тест двутавров
        [TestMethod()]
        public void UT_ModHandler_PrfUpdate_I()
        {
            var mod = new ModHandler();
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


        // 2017.06.30 тест замкнутых профилей ГОСТ 30245-2003
        [TestMethod()]
        public void UT_ModHandler_PrfUpdate_PP_PK()
        {
            var mod = new ModHandler();
            ElmAttSet.Group gr = new ElmAttSet.Group();
            List<ElmGr> inp = new List<ElmGr>();

            // test 0: "PP140X100X5_30245_2003" => "Гн.[]140x100x5"
            gr.Prf = "PP140X100X5_30245_2003";
            gr.prf = "pp140X100X5_30245_2003";
            inp.Add(gr);
            var res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "гн.[]140x100x5");
            Assert.AreEqual(res[0].Prf, "Гн.[]140x100x5");

            // test 1: "PK100X4_30245_2003" => "Гн.100x4"
            inp.Clear();
            gr.Prf = "PK100X4_30245_2003";
            gr.prf = "pk100X4_30245_2003";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "гн.100x4");
            Assert.AreEqual(res[0].Prf, "Гн.100x4");
        }
        // 2017.06.29 тест швеллеров
        [TestMethod()]
        public void UT_ModHandler_PrfUpdate_U()
        {
            var mod = new ModHandler();
            ElmAttSet.Group gr = new ElmAttSet.Group();
            List<ElmGr> inp = new List<ElmGr>();

            // test 0: "U10_8240_97" => "[10П"
            gr.Prf = "U10_8240_97";
            gr.prf = "u10_8240_97";
            inp.Add(gr);
            var res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "[10");
            Assert.AreEqual(res[0].Prf, "[10");

            // test 1: "U20P_8240_97" => "[20П"
            inp.Clear();
            gr.Prf = "U20P_8240_97";
            gr.prf = "u20p_8240_97";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "[20п");
            Assert.AreEqual(res[0].Prf, "[20П");

            // test 2: "U30AP_8240_97" => "[30aП"
            inp.Clear();
            gr.Prf = "U30AP_8240_97";
            gr.prf = "u30ap_8240_97";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "[30aп");
            Assert.AreEqual(res[0].Prf, "[30аП");

            // test 3: "U20Y_8240_97" => "[20У"
            inp.Clear();
            gr.Prf = "U20Y_8240_97";
            gr.prf = "u20y_8240_97";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "[20y");
            Assert.AreEqual(res[0].Prf, "[20У");

            // test 4: "U30AY_8240_97" => "[30aУ"
            inp.Clear();
            gr.Prf = "U30AY_8240_97";
            gr.prf = "u30ay_8240_97";
            inp.Add(gr);
            res = mod.PrfUpdate(inp);
            Assert.AreEqual(res[0].prf, "[30ay");
            Assert.AreEqual(res[0].Prf, "[30аУ");
        }

        [TestMethod()]
        public void UT_ModHandler_geGroup_Native()
        {
            var boot = new Boot();
            var model = new Mod();
            model.SetModel(boot);
        }
#endif //OLD 3.7.17
        [TestMethod()]
        public void UT_Hndl()
        {
            var boot = new Boot();
            var model = new Mod();
            model.SetModDir(boot);
            var sr = new SR();
            model.elements = sr.Raw(model);
            List<Elm> elmCopy = new List<Elm>();
            foreach (Elm elm in model.elements) elmCopy.Add(elm);
            for (int i = 0; i < elmCopy.Count; i++) Assert.AreEqual(elmCopy[i], model.elements[i]);
            int cnt = model.elements.Count;
            string MD5 = model.getMD5(model.elements);
            Assert.IsTrue(cnt > 0);
            string cMD5 = model.getMD5(elmCopy);
            Assert.AreEqual(cMD5, MD5);
            if (model.Rules == null || model.Rules.Count == 0)
            {
                sr.getSavedRules(model, init:true);
            }
            var mh = new MH();

            mh.Hndl(ref model);

            // проверка, что elements не испортились
            foreach (var gr in model.elmGroups) cnt -= gr.guids.Count();
            Assert.AreEqual(0, cnt);
            Assert.AreEqual(model.elements.Count, elmCopy.Count);
            for (int i = 0; i < elmCopy.Count; i++) Assert.AreEqual(elmCopy[i], model.elements[i]);
            string newMD5 = model.getMD5(model.elements);
            string copyMD5 = model.getMD5(elmCopy);
            Assert.AreEqual(model.getMD5(model.elements), MD5);


            //Hndl performance test -- 180 sec for 100 cycles ОНХП модель 1124 элемента
            //                      -- 20,4 sec 1 cycle модель "Навес над трибунами" 7128 э-тов
            int nLoops = 1;                    
            DateTime t0 = DateTime.Now;
            for (int i = 0; i < nLoops; i++)
            {
                mh.Hndl(ref model);
            }
            TimeSpan ts = DateTime.Now - t0;
            var secHndl = ts.TotalSeconds / nLoops;
            Assert.IsTrue(secHndl > 0.0);

            // 

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Pricing()
        {
            var boot = new Boot();
            var model = new Mod();
            model.SetModel(boot);

            var mh = new MH();
            mh.Pricing(ref model);
            Assert.IsTrue(model.matches.Count > 0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_getGrps()
        {
            var boot = new Boot();
            var model = new Mod();
            var mh = new MH();
            var sr = new SR();
            if (boot.isTeklaActive) model.dir = TS.GetTeklaDir(TS.ModelDir.model);
            else model.dir = boot.ModelDir;

            model.elements = sr.Raw(model);
            string md5 = model.getMD5(model.elements);
            Assert.AreEqual(32, md5.Length);

            var grp = mh.getGrps(model.elements);
            Assert.IsTrue(grp.Count > 0);
            string pricing_md5 = model.get_pricingMD5(grp);
            Assert.AreEqual(32, pricing_md5.Length);

            FileOp.AppQuit();
        }
#if old //24/5 move to UT_ModelHandle
        [TestMethod]
        public void UT_Model_getGroup()
        {
            var Im = new IMIT();
            Mod model = Im.IM_Model();
            Dictionary<string, Elm> elements = Im.IM_Elements();
            string id1 = "MyId1", id2 = "MyId2";
            string mat = elements[id1].mat;
            string prf = elements[id1].prf;
            List<string> guids = new List<string> { id1, id2 };

            ElmAttSet.Group gr = new ElmAttSet.Group(elements, mat, prf, guids);

            Assert.AreEqual(gr.totalPrice, elements[id1].price + elements[id2].price);

            model.setElements(elements.Values.ToList());
            model.getGroups();
            Assert.AreEqual(model.elmGroups.Count, 3);
            Assert.AreEqual(model.elmGroups[0].mat, mat);
            Assert.AreEqual(model.elmGroups[0].prf, prf);
            Assert.AreEqual(model.elmGroups[0].totalPrice, 0);
            Assert.AreEqual(model.elmGroups[0].guids.Count, 2);
            Assert.AreEqual(model.elmGroups[1].guids.Count, 1);
            var gds0 = model.elmGroups[0].guids;
            Assert.AreEqual(gds0[0], id1);
            Assert.AreEqual(gds0[1], id2);
            Assert.AreEqual(model.elmGroups[1].totalPrice, 0);

            var grB = Im.IM_Group("B20");
            Assert.AreEqual(grB.mat, "b20");
            Assert.AreEqual(grB.prf, "1900x1600");
            Assert.AreEqual(grB.guids.Count, 2);
        }
#endif // 24/5 moveto UT_ModelHandle
    }
}