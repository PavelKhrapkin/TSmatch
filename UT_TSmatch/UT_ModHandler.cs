/*=================================
 * Model.Handler Unit Test 1.07.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Model.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileOp = match.FileOp.FileOp;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Mod = TSmatch.Model.Model;
using ElmGr = TSmatch.ElmAttSet.Group;
using Msg = TSmatch.Message.Message;
using TS = TSmatch.Tekla.Tekla;
using TSmatch.ElmAttSet;

namespace TSmatch.Model.Handler.Tests
{
    [TestClass()]
    public class UT_ModHandler
    {

    //////    [TestMethod()]
    //////    public void UT_ModHandler_PrfUpdate()
    //////    {
    //////        var mod = new ModHandler();
    //////        ElmAttSet.Group gr = new ElmAttSet.Group();
    //////        List<ElmGr> inp = new List<ElmGr>();

    //////        Test_I(mod, gr);
    //////        Test_U(mod, gr);

    //////        // test 0: "PL6" -> "—6"
    //////        gr.Prf = "PL6";
    //////        gr.prf = "pl6";
    //////        inp.Add(gr);
    //////        var res = mod.PrfUpdate(inp);
    //////        Assert.AreEqual(res[0].prf, "—6");

    //////        // test 1: "—100*6" => "—6x100"
    //////        inp.Clear();
    //////        gr.Prf = gr.prf = "—100*6";
    //////        inp.Add(gr);
    //////        res = mod.PrfUpdate(inp);
    //////        Assert.AreEqual(res[0].prf, "—6x100");

    //////        // test 2: "—100*6" => "—6x100"
    //////        gr.Prf = gr.prf = "—100*6";
    //////        inp.Add(gr);
    //////        res = mod.PrfUpdate(inp);
    //////        Assert.AreEqual(res[1].prf, "—6x100");

    //////        // test 3: "L75X5_8509_93" => L75x5"
    //////        gr.Prf = gr.prf = "L75X5_8509_93";
    //////        inp.Add(gr);
    //////        res = mod.PrfUpdate(inp);
    //////        Assert.AreEqual(res[2].Prf, "L75x5");
    //////        Assert.AreEqual(res[2].prf, "l75x5");
    //////    }

        //////// 2017.06.30 тест замкнутых профилей ГОСТ 30245-2003
        //////[TestMethod()]
        //////public void UT_ModHandler_PrfUpdate_PP_PK()
        //////{
        //////    var mod = new ModHandler();
        //////    ElmAttSet.Group gr = new ElmAttSet.Group();
        //////    List<ElmGr> inp = new List<ElmGr>();

        //////    // test 0: "PP140X100X5_30245_2003" => "Гн.[]140x100x5"
        //////    gr.Prf = "PP140X100X5_30245_2003";
        //////    gr.prf = "pp140X100X5_30245_2003";
        //////    inp.Add(gr);
        //////    var res = mod.PrfUpdate(inp);
        //////    Assert.AreEqual(res[0].prf, "гн.[]140x100x5");
        //////    Assert.AreEqual(res[0].Prf, "Гн.[]140x100x5");

        //////    // test 1: "PK100X4_30245_2003" => "Гн.100x4"
        //////    inp.Clear();
        //////    gr.Prf = "PK100X4_30245_2003";
        //////    gr.prf = "pk100X4_30245_2003";
        //////    inp.Add(gr);
        //////    res = mod.PrfUpdate(inp);
        //////    Assert.AreEqual(res[0].prf, "гн.100x4");
        //////    Assert.AreEqual(res[0].Prf, "Гн.100x4");
        //////}
        //////// 2017.06.29 тест швеллеров
        //////[TestMethod()]
        //////public void UT_ModHandler_PrfUpdate_U()
        //////{
        //////    var mod = new ModHandler();
        //////    ElmAttSet.Group gr = new ElmAttSet.Group();
        //////    List<ElmGr> inp = new List<ElmGr>();

        //////    // test 0: "U10_8240_97" => "[10П"
        //////    gr.Prf = "U10_8240_97";
        //////    gr.prf = "u10_8240_97";
        //////    inp.Add(gr);
        //////    var res = mod.PrfUpdate(inp);
        //////    Assert.AreEqual(res[0].prf, "[10");
        //////    Assert.AreEqual(res[0].Prf, "[10");

        //////    // test 1: "U20P_8240_97" => "[20П"
        //////    inp.Clear();
        //////    gr.Prf = "U20P_8240_97";
        //////    gr.prf = "u20p_8240_97";
        //////    inp.Add(gr);
        //////    res = mod.PrfUpdate(inp);
        //////    Assert.AreEqual(res[0].prf, "[20п");
        //////    Assert.AreEqual(res[0].Prf, "[20П");

        //////    // test 2: "U30AP_8240_97" => "[30aП"
        //////    inp.Clear();
        //////    gr.Prf = "U30AP_8240_97";
        //////    gr.prf = "u30ap_8240_97";
        //////    inp.Add(gr);
        //////    res = mod.PrfUpdate(inp);
        //////    Assert.AreEqual(res[0].prf, "[30aп");
        //////    Assert.AreEqual(res[0].Prf, "[30аП");

        //////    // test 3: "U20Y_8240_97" => "[20У"
        //////    inp.Clear();
        //////    gr.Prf = "U20Y_8240_97";
        //////    gr.prf = "u20y_8240_97";
        //////    inp.Add(gr);
        //////    res = mod.PrfUpdate(inp);
        //////    Assert.AreEqual(res[0].prf, "[20y");
        //////    Assert.AreEqual(res[0].Prf, "[20У");

        //////    // test 4: "U30AY_8240_97" => "[30aУ"
        //////    inp.Clear();
        //////    gr.Prf = "U30AY_8240_97";
        //////    gr.prf = "u30ay_8240_97";
        //////    inp.Add(gr);
        //////    res = mod.PrfUpdate(inp);
        //////    Assert.AreEqual(res[0].prf, "[30ay");
        //////    Assert.AreEqual(res[0].Prf, "[30аУ");
        //////}

        //////[TestMethod()]
        //////public void UT_ModHandler_geGroup_Native()
        //////{
        //////    var boot = new Boot();
        //////    var model = new Mod();
        //////    model.SetModel(boot);
        //////}

        [TestMethod()]
        public void UT_Hndl()
        {
            var boot = new Boot();
            var model = new Mod();
            model.SetModel(boot);

            var mh = new ModHandler();
            mh.Hndl(model);
            int cnt = 0;
            foreach (var gr in model.elmGroups) cnt += gr.guids.Count();
            Assert.AreEqual(model.elements.Count(), cnt);

            //Hndl performance test -- 180 sec for 100 cycles
            DateTime t0 = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                mh.Hndl(model);
            }
            TimeSpan ts = DateTime.Now - t0;
            Assert.IsTrue(ts.TotalSeconds > 0.0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Pricing()
        {
            var boot = new Boot();
            var model = new Mod();
            model.SetModel(boot);

            var mh = new ModHandler();
            mh.Pricing(ref model);
            Assert.IsTrue(model.matches.Count > 0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_getGrps()
        {
            var boot = new Boot();
            var model = new Mod();
            model.sr = new SaveReport.SavedReport();
            if (boot.isTeklaActive)
            {
                model.dir = TS.GetTeklaDir(TS.ModelDir.model);
                var ts = new TS();
                model.elementsCount = ts.elementsCount();
            }
            else
            {
                throw new NotImplementedException();
            }
            model.elements = model.sr.Raw(model);
            var mh = new ModHandler();

            var grp = mh.getGrps(model.elements);
            Assert.IsTrue(grp.Count > 0);

            FileOp.AppQuit();
        }
    }
}