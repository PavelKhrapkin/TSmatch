/*=================================
 * ProfileUpdate Unit Test 2.07.2017
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

        [TestInitialize()]
        public void Initialize()
        {
//            MessageBox.Show("TestMethodInit");
        }

        // 2017.07.1 тест двутавров
        [TestMethod()]
        public void UT_PrfUpdate_I()
        {
            // test 0: "I10_8239_89" => "I10"
            initGr("I10_8239-89");
            var xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i10", mod.elmGroups[0].prf);
            Assert.AreEqual("I10", mod.elmGroups[0].Prf);

            #region --- серия Б ---
            // test Б1: "I20B1_20_93" => I20Б1"
            initGr("I20B1_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i20б1", mod.elmGroups[0].prf);
            Assert.AreEqual("I20Б1", mod.elmGroups[0].Prf);

            // test Б2: "I20B2_20_93" => I20Б2"
            initGr("I20B2_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i20б2", mod.elmGroups[0].prf);
            Assert.AreEqual("I20Б2", mod.elmGroups[0].Prf);

            // test Б3: "I50B3_20_93" => I20Б23"
            initGr("I50B3_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i50б3", mod.elmGroups[0].prf);
            Assert.AreEqual("I50Б3", mod.elmGroups[0].Prf);
            #endregion --- серия Б ---

            #region --- серия K ---
            // test K1: "I20K1_20_93" => I20K1"
            initGr("I20K1_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i20к1", mod.elmGroups[0].prf);
            Assert.AreEqual("I20К1", mod.elmGroups[0].Prf);

            // test К2: "I20К5_20_93" => I20К5"
            initGr("I20K5_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i20к5", mod.elmGroups[0].prf);
            Assert.AreEqual("I20К5", mod.elmGroups[0].Prf);

            // test К3: "I20К3A_20_93" => I20К3А" I20K3A_20_93
            initGr("I20K3A_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i20к3a", mod.elmGroups[0].prf);
            Assert.AreEqual("I20К3А", mod.elmGroups[0].Prf);
            #endregion --- серия K ---

            #region --- серия Ш = H ---
            // test Ш1: "I30H1_20_93" => I30Ш1"
            initGr("I30H1_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i30ш1", mod.elmGroups[0].prf);
            Assert.AreEqual("I30Ш1", mod.elmGroups[0].Prf);

            // test Ш2: "I100Р5_20_93" => I100Ш5"
            initGr("I100H5_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i100ш5", mod.elmGroups[0].prf);
            Assert.AreEqual("I100Ш5", mod.elmGroups[0].Prf);
            #endregion --- серия Ш = H ---

            #region --- серия Р40-93 Д и У ---
            // test Д1: "I25D3A_20_93" => I25Д3A"
            initGr("I25D3A_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i25д3a", mod.elmGroups[0].prf);
            Assert.AreEqual("I25Д3А", mod.elmGroups[0].Prf);

            // test Ш2: "I36Y2A_20_93" => "I36У2A"
            initGr("I36Y2A_20_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i36y2a", mod.elmGroups[0].prf);
            Assert.AreEqual("I36У2А", mod.elmGroups[0].Prf);
            #endregion --- серия Р40-93 Д и У ---
        }

        // 2017.07.2 тест швеллеров
        [TestMethod()]
        public void UT_PrfUpdate_U()
        {
            #region --- серия У ГОСТ 8240-97 --- 
            // test 1У: "U18AY_8240_97" => "]18aУ"
            initGr("U18AY_8240_97");
            var xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[18ay", mod.elmGroups[0].prf);
            Assert.AreEqual("[18аУ", mod.elmGroups[0].Prf);

            // test 2У: "U6.5Y_8240_97" => "]6.5У"
            initGr("U6.5Y_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[6.5y", mod.elmGroups[0].prf);
            Assert.AreEqual("[6.5У", mod.elmGroups[0].Prf);
            #endregion --- серия У --- 
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

        private void initGr(string v)
        {
            inp.Clear();
            gr.Prf = v;
            gr.prf = Lib.ToLat(v.ToLower().Replace(" ", ""));
            inp.Add(gr);
            mod.elmGroups = inp;
        }
    } // end class UT_ProfileUpdate
} // end namespace TSmatch.ProfileUpdate.Tests