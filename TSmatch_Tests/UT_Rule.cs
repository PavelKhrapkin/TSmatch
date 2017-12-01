/*=================================
 * Rule Unit Test 30.11.2017
 *=================================
 *-- ToDo 2017.6.7 пересмотреть старые тесты (OLD 6/6/17) и либо выбросить их совсем, либо обновить
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

using Boot = TSmatch.Bootstrap.Bootstrap;
using Mod = TSmatch.Model.Model;
using Doc = TSmatch.Document.Document;
using FileOp = match.FileOp.FileOp;
using SType = TSmatch.Section.Section.SType;

//7/6 using IMIT = TSmatch.Unit_Tests.Imitation._UT_Imitation;


namespace TSmatch.Rule.Tests
{
    [TestClass()]
    public class UT_Rule
    {
        Boot boot = new Boot();
        Mod mod = new Mod();

        [TestMethod()]
        public void UT_Rule_Constructor()
        {
            boot.Init();
            mod.sr.SetModel(boot);

            Rule rule = new Rule(5);
            Assert.IsNotNull(rule);
            Assert.AreEqual(rule.sSupl, "СтальХолдинг");
            Assert.AreEqual(rule.sCS, "Уголок равнопол.");

            // test Exception
            // при ошибке в файле Rules, конструктор возвращает в е номер строки файла, где ошибка
            try { new Rule(2); }
            catch (InvalidCastException e) { };

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Rule_InitSupplier()
        {
            boot.Init();
            mod.sr.SetModel(boot);
            Doc dRul = Doc.getDoc("Rules");
            Assert.IsNotNull(dRul);

            // test 1: Init one Rule
            Rule r = new Rule(4 + 3);
            r.Init();
            var csDP = r.CompSet.csDP;
            Assert.IsNotNull(csDP);
            bool b = csDP.dpStr.ContainsKey(SType.UNIT_Weight);

            // test 2: Check if all Rules have CompSet with Section Unit_
            mod = mod.sr.SetModel(boot, initSupl: true);
            Assert.IsTrue(mod.Rules.Count > 0);
            foreach(var rule in mod.Rules)
            {
                csDP = rule.CompSet.csDP;
                bool bw = csDP.dpStr.ContainsKey(SType.UNIT_Weight);
                bool bv = csDP.dpStr.ContainsKey(SType.UNIT_Vol);
                Assert.IsTrue(bw || bv);
            }

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Rule_SynParse()
        {
            boot.Init();
            mod = mod.sr.SetModel(boot);
            var Rules = mod.Rules.ToList();

            // test 0: синонимы по материалам M:С235=C245
            Rule rule = Rules.Find(x => x.text.Contains("c255=c245=c235"));
            if (rule != null)
            {
                var SynsM = rule.synonyms[SType.Material];
                Assert.IsTrue(SynsM.Count >= 3);
                Assert.AreEqual("c255", SynsM[0]);
                Assert.AreEqual("c245", SynsM[1]);
                Assert.AreEqual("c235", SynsM[2]);
            }

            FileOp.AppQuit();
        }

#if FOR_PRICE
        [TestMethod()]
        public void UT_Rule_Parser()
        {
            var Im = new IMIT();
            var cs = Im.IM_CompSet();
            string ld = "M:*;Prof:опис: Уголок=L*x*;Price:*";
            Rule rule = Im.IM_Rule(ld);

            ///31/3///////var rFPs = rule.Parser(FPtype.Rule, ld);
            //////////////Assert.AreEqual(rFPs.Count, 3);
            var ruleSyns = rule.synonyms[SType.Profile];
            Assert.AreEqual(ruleSyns.Count, 2);
            Assert.AreEqual(ruleSyns[0], "угoлoк");
            Assert.AreEqual(ruleSyns[1], "l");
            //////////////////var matFP = rFPs[SType.Material];
            //////////////////Assert.AreEqual(matFP.parN(), "*");
            /// 31/3 /////////var prfFP = rFPs[SType.Profile];
            //////////////////Assert.AreEqual(prfFP.parN(), "yгoлoк=l*x*");
            //////////////////Assert.AreEqual(rFPs[SType.Price].parN(), "*");
        }
#endif // FOR_PRICE
    }
}
