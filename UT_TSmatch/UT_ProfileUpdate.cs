/*=================================
 * ProfileUpdate Unit Test 16.07.2017
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
        public void UT_ProfleUpdate_I()
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

            #region --- серия Ш = H = ДВУТАВР ---
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

            // test Ш3: "ДВУТAВР30Ш2" => I30Ш2"
            initGr("ДВУТAВР30Ш2");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("i30ш2", mod.elmGroups[0].prf);
            Assert.AreEqual("I30Ш2", mod.elmGroups[0].Prf);
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
        public void UT_ProfileUpdate_U()
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

            // test 3У: "U18AY_8240_97" => "]18аУ"
            initGr("U18AY_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[18ay", mod.elmGroups[0].prf);
            Assert.AreEqual("[18аУ", mod.elmGroups[0].Prf);
            #endregion --- серия У --- 

            #region --- серия П ГОСТ 8240-97 --- 
            // test 1П: "U30P_8240_97" => "]30П"
            initGr("U30P_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[30п", mod.elmGroups[0].prf);
            Assert.AreEqual("[30П", mod.elmGroups[0].Prf);

            // test 2П: "U16AP_8240_97" => "]16аП"
            initGr("U16AP_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[16aп", mod.elmGroups[0].prf);
            Assert.AreEqual("[16аП", mod.elmGroups[0].Prf);
            #endregion --- серия П --- 

            #region --- серии Э, Л, С ГОСТ 8240-97 ---
            // test 1Э: "U5E_8240_97" => "]5Э"
            initGr("U5E_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[5э", mod.elmGroups[0].prf);
            Assert.AreEqual("[5Э", mod.elmGroups[0].Prf);

            // test 2Л: "U27L_8240_97" => "]27Л"
            initGr("U27L_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[27л", mod.elmGroups[0].prf);
            Assert.AreEqual("[27Л", mod.elmGroups[0].Prf);

            // test 3C: "U26CA_8240_97" => "]26Ca"
            initGr("U26CA_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[26ca", mod.elmGroups[0].prf);
            Assert.AreEqual("[26Cа", mod.elmGroups[0].Prf);

            // test 4C: "U30CB_8240_97" => "]30Cб"
            initGr("U30CB_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[30cб", mod.elmGroups[0].prf);
            Assert.AreEqual("[30Cб", mod.elmGroups[0].Prf);

            // test 5C: "U26C_8240_97" => "]26C"
            initGr("U26C_8240_97");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("[26c", mod.elmGroups[0].prf);
            Assert.AreEqual("[26C", mod.elmGroups[0].Prf);
            #endregion --- серии Э, Л, C ГОСТ 8240-97 ---
        }

        // 2017.07.2 тест уголков
        [TestMethod()]
        public void UT_ProfileUpdate_L()
        {
            // Уголки равнополочные ГОСТ 8509-93 и неравнополочные ГОСТ 8510-86 --- 
            // test 1L: "L40X5_8509_93" => "L40x5"
            initGr("L40X5_8509_93");
            var xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("l40x5", mod.elmGroups[0].prf);
            Assert.AreEqual("L40x5", mod.elmGroups[0].Prf);

            // test 2L: "L250X35_8509_93" => "L250x35"
            initGr("L250X35_8509_93");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("l250x35", mod.elmGroups[0].prf);
            Assert.AreEqual("L250x35", mod.elmGroups[0].Prf);

            // test 3L: "L75X50X8_8510_86" => "L75x50x8"
            initGr("L75X50X8_8510_86");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("l75x50x8", mod.elmGroups[0].prf);
            Assert.AreEqual("L75x50x8", mod.elmGroups[0].Prf);
        }

        [TestMethod()]
        public void UT_ProfileUpdate_PL()
        {
            // test 0: "PL6" -> "—6"
            initGr("PL6");
            var xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("—6", mod.elmGroups[0].Prf);

            // test 1: "PL100*6" => "—6x100"
            initGr("PL100x6");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("—6x100", mod.elmGroups[0].Prf);

            // test 2: "PL6*100" => "—6x100"
            initGr("PL6*100");
            xx = new ProfileUpdate(ref inp); ;
            Assert.AreEqual("—6x100", mod.elmGroups[0].Prf);
        }

        [TestMethod()]
        public void UT_ProfileUpdate_PK_PP()
        {
            // test 0PP: "PP140X100X6_67_2287_80" -> "Гн.[] 140x100x6"
            initGr("PP140X100X6_67_2287_80");
            var xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("Гн.[]140x100x6", mod.elmGroups[0].Prf);

            // test 1PK: "PK160X5_36_2287_80" -> "Гн.[] 160x5"
            initGr("PK160X5_36_2287_80");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("Гн.160x5", mod.elmGroups[0].Prf);

            // test 2PK: "Профиль(кв.)120X120X7.0" -> "Гн.120x7"
            initGr("Профиль(кв.)120X120X7.0");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("Гн.120x7", mod.elmGroups[0].Prf);

            // test 3PK from Issue 2017.07.06 : "Гн.120x120x7" -> "Гн. 120x7"
            initGr("Гн.120x120x7");
            xx = new ProfileUpdate(ref inp);
            Assert.AreEqual("Гн.120x7", mod.elmGroups[0].Prf);
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