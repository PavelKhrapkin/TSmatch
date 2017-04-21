using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CS = TSmatch.CompSet.CompSet;
using SType = TSmatch.Section.Section.SType;
using DP = TSmatch.DPar.DPar;
using FP = TSmatch.FingerPrint.FingerPrint;
using IMIT = TSmatch.Unit_Tests.Imitation._UT_Imitation;


namespace TSmatch.Component.Tests
{
    [TestClass()]
    public class CompSet_Test
    {
        [TestMethod()]
        public void UT_CompSet()
        {
            var Im = new IMIT();
            string ld = "M:1;Des:2;проф:3";
            var csDP = Im.IM_CompSet(ld).csDP;
            Assert.AreEqual(csDP.dpar.Count, 3);
            Assert.AreEqual(csDP.Col(SType.Material), 1);
            Assert.AreEqual(csDP.Col(SType.Description), 2);
            Assert.AreEqual(csDP.Col(SType.Profile), 3);

            csDP = Im.IM_CompSet("M:C245").csDP;
            Assert.AreEqual(csDP.dpStr[SType.Material], "C245");
            Assert.AreEqual(csDP.Col(SType.Material), -1);

            ld = "M:1; опис: 3; профиль: 2; цена: 6; Ед: руб / т";
            csDP = Im.IM_CompSet(ld).csDP;
            Assert.AreEqual(csDP.dpar.Count, 5);
            Assert.AreEqual(csDP.Col(SType.Material), 1);
            Assert.AreEqual(csDP.Col(SType.Description), 3);
            Assert.AreEqual(csDP.Col(SType.Profile), 2);
            Assert.AreEqual(csDP.Col(SType.Price), 6);

        }

 //////////////////       [TestMethod()]
 //////////////////       public void UT_CompSet_PL30()
 //// 1/4 /////////       {
 //////////////////           var Im = new IMIT();
 //////////////////           var rule = Im.IM_Rule();

 //////////////////           string LoadDescr
 //////////////////               = "опис: {1}; длина заг.:{3}; цена: {4};"
 //////////////////                   + "проф: опис: Полоса горячекатаная * x *";
 //////////////////           CS cs = new CS("Полоса для тестов", null, rule, LoadDescr);
 //////////////////           Assert.AreEqual(cs.csFPs.Count, 4);
 //////////////////           Assert.AreEqual(cs.csFPs[SType.Description].Col(), 1);
 //////////////////           Assert.AreEqual(cs.csFPs[SType.Price].Col(), 4);
 //////////////////           Assert.AreEqual(cs.csFPs[SType.LengthPerUnit].Col(), 3);
 //////////////////           FP csPrf = cs.csFPs[SType.Profile];
 ////////////////////31/3           Assert.AreEqual(csPrf.parN(), "пoлocaгopячeкaтaнaя*x*");
 //////////////////       }
    }

    [TestClass()]
    public class ComponentTests
    {
        [TestMethod()]
        public void UT_Component()
        {
            var Im = new IMIT();
            var c = Im.Im_Comp("B12,5", "Бетон B12,5", price: "2970");
            Assert.AreEqual(c.viewComp(SType.Description), "Бетон B12,5");
            Assert.AreEqual(c.viewComp_(SType.Material), "b12,5");
            Assert.AreEqual(c.viewComp(SType.Price), "2970");
            Assert.AreEqual(c.viewComp(SType.LengthPerUnit).Contains("##"), true);

            ////////////////////////-- проверяем, что разные написания компонентов дают одно и то же
            //////////////////////var c1 = Im.Im_Comp("C255", "Двутавр 20Ш1", price: "2970");
            //////////////////////var c2 = Im.Im_Comp("C255", "ДВУТАВР 20Ш1", price: "2970");
            //////////////////////var c3 = Im.Im_Comp("C255", "двутавр 20Ш1", price: "2970");

            //////////////////////var model = Im.IM_Model();
            //////////////////////model.getGroups();
            //////////////////////var gr = Im.IM_Group("c255", "двутавр 20ш1");

            //////////////////           string ld = "M:С235;Des:{1};проф:оп:Уголок *х*; Price:{d~3}";
            //////////////////           var csFPs = Im.IM_CompSet(ld).csFPs;

            //////////////////           c = new Component("Уголок 25x8", price: 2345, csFPs: csFPs);
            //////////////////           Assert.AreEqual(c.fps.Count, 4);
            //////////////////           Assert.AreEqual(c.viewComp(SType.Material), "c245");
            //////////////////           var prfFP = c.fps[SType.Profile];
            // 3/4 ///////////           Assert.AreEqual(prfFP.pars.Count, 2);
            //////////////////           Assert.AreEqual(prfFP.parN(), "25");
            //////////////////           Assert.AreEqual(prfFP.parN(1), "8");
            ////////////////////           Assert.AreEqual(c.viewComp(SType.Profile), "Уголок 25x8");

            //////////////////           c = new Component("Уголок 25x8", "C245", price: 1234.56
            //////////////////               , csFPs: csFPs);
            //////////////////           Assert.AreEqual(c.viewComp(SType.Material), "c245");
            //////////////////           Assert.AreEqual(c.viewComp(SType.Profile), "угoлoк25x8");
            //////////////////           Assert.AreEqual(c.viewComp(SType.Price), "1234,56");

            //////////////////           var matFP = csDP[SType.Material];
            ////////////////////Assert.AreEqual(matFP.parN(), "c235");
            ////////////////////Assert.AreEqual(matFP.txs.Count, 1);
            ////////////////////Assert.AreEqual(matFP.txs[0], "c235");
            ////////////////////var desFP = csFPs[SType.Description];
            ////////////////////Assert.AreEqual(desFP.Col(), 1);
            ////////////////////Assert.AreEqual(desFP.txs.Count, 1);
            ////////////////////Assert.AreEqual(desFP.txs[0], "");
            ////////////////////var prfFP = csFPs[SType.Profile];
            ////////////////////Assert.AreEqual(prfFP.parN(), "yгoлoк*x*");
            ////////////////////Assert.AreEqual(prfFP.txs.Count, 1);
            ////////////////////Assert.AreEqual(prfFP.txs[0], "yгoлoк*x*");

            ////////////////////var rule = Im.IM_Rule();
            ////////////////////string LoadDescr
            ////////////////////    = "опис:{1};профиль: опис: Угoлoк *x*c*; длина:{2};цена:{3}";
            ////////////////////CS cs = new CS("Полоса для тестов", rule, LoadDescr);
            ////////////////////Assert.AreEqual(cs.csFPs.Count, 4);
            ////////////////////var csFPdes = cs.csFPs[SType.Description];
            ////////////////////Assert.AreEqual(csFPdes.pars.Count, 1);
            ////////////////////Assert.AreEqual(csFPdes.parN(), "1");
            ////////////////////Assert.AreEqual(csFPdes.Col(), 1);
            ////////////////////Assert.AreEqual(cs.csFPs[SType.Price].Col(), 3);
            ////////////////////Assert.AreEqual(cs.csFPs[SType.LengthPerUnit].Col(), 2);
            // 31/3 ////////////var csFPprf = cs.csFPs[SType.Profile];
            ////////////////////Assert.AreEqual(csFPprf.pars.Count, 1);
            ////////////////////Assert.AreEqual(csFPprf.txs.Count, 1);
            ////////////////////Assert.AreEqual(csFPprf.txs[0], "yгoлoк*x*c*");
            ////////////////////Assert.AreEqual(csFPprf.parN(), "yгoлoк*x*c*");

            ////////////////////var c1 = new Component("Уголок 25x8", "C245", price: 1234.56);
            ////////////////////var c2 = new Component("Уголок 35x12", "C245", price: 2541.23);
            ////////////////////List<Component> comps = new List<Component> { c1, c2 };

            ////////////////////cs = new CS("Пример уголков", rule, LoadDescr, comps: comps);
            ////////////////////var vv = cs.Components[1].fps[SType.Price].pars[0].par;
            ////////////////////Assert.AreEqual(
            ////////////////////    cs.Components[0].fps[SType.Price].pars[0].par.ToString()
            ////////////////////    , "1234,56");

            ////////////////////cs = Im.IM_CompSet();
            ////////////////////var c0p = cs.Components[0].fps[SType.Price].pars[0].par;
            ////////////////////var c1p = cs.Components[1].fps[SType.Price].pars[0].par;
            ////////////////////var c2p = cs.Components[2].fps[SType.Price].pars[0].par;
            ////////////////////var c3p = cs.Components[3].fps[SType.Price].pars[0].par;
            ////////////////////Assert.AreEqual(c0p,  "832");
            ////////////////////Assert.AreEqual(c1p, "1010");
            ////////////////////Assert.AreEqual(c2p, "1234,56");
            ////////////////////Assert.AreEqual(c3p, "2541,23");
        }

        [TestMethod()]
        public void Component_isMatch_Test()
        {
            string ld = "M:;D:;";
            var Im = new IMIT();
            var gr = Im.IM_Group();
            var rule = Im.IM_Rule();
            //////////////////var c = new Component("l20x5", "C245");
            //////////////////Im.IM_setRuleSynonyms(ref rule);
            //////////////////Assert.AreEqual(rule.synonyms[SType.Profile].ToList().Count, 2);
            //////////////////Assert.AreEqual(gr.prf, "l20x5");

            //////////////////bool b = c.isMatch(gr, rule);
            /// 31/3 /////////Assert.AreEqual(b, true);

            //////////////////gr = Im.IM_Group(1);
            //////////////////Assert.AreEqual(gr.prf, "l25x8");
            //////////////////b = c.isMatch(gr, rule);
            //////////////////Assert.AreEqual(b, false);

            //////////////////Im.IM_setRuleSynonyms(ref rule, 0);
            //////////////////c = new Component("Швеллер", "c245");
            //////////////////b = c.isMatch(gr, rule);
            //////////////////Assert.AreEqual(b, false);

            //////////////////c = new Component("Уголок равнопол.25x8", "c245");
            //////////////////Assert.AreEqual(gr.prf, "l25x8");
            //////////////////b = c.isMatch(gr, rule);
            //////////////////Assert.AreEqual(b, true);

            //////////////////c = new Component("Бетон B20", "B20");
            //////////////////gr = Im.IM_Group("b");
            //////////////////Assert.AreEqual(gr.mat, "b20");
            //////////////////b = c.isMatch(gr);
            //////////////////Assert.AreEqual(b, true);
        }
    }
}