/*=================================
 * CompSet Unit Test 3.10.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Supl = TSmatch.Suppliers.Supplier;
using Comp = TSmatch.Component.Component;
using SType = TSmatch.Section.Section.SType;
using DP = TSmatch.DPar.DPar;
using Mod = TSmatch.Model.Model;
using FileOp = match.FileOp.FileOp;

namespace TSmatch.CompSet.Tests
{
    [TestClass()]
    public class UT_CompSet
    {
        Boot boot = new Boot();

        [TestMethod()]
        public void UT_CompSet_init()
        {
            string LoadDescriptor = "M:1; опис:3; профиль:2; цена: 4; Ед: руб/т";
            List<Comp> comps = new List<Comp>()
            {
                new Comp(new DP("Prf:I10")),
                new Comp(new DP("Prf:I20"))
            };
            Supl supl = new Supl("СтальХолдинг", init: false);
            CompSet cs = new CompSet("Балка", supl, LoadDescriptor, comps);
            Assert.AreEqual("Балка", cs.name);
            Assert.AreEqual("1", cs.csDP.dpar[SType.Material]);
            Assert.AreEqual("2", cs.csDP.dpar[SType.Profile]);
            Assert.AreEqual("3", cs.csDP.dpar[SType.Description]);
            Assert.AreEqual("4", cs.csDP.dpar[SType.Price]);
            Assert.AreEqual("СтальХолдинг", cs.Supplier.Name);
            Assert.AreEqual(2, cs.Components.Count);
            Assert.AreEqual("I10", cs.Components[0].Str(SType.Profile));
            Assert.AreEqual("I20", cs.Components[1].Str(SType.Profile));
        }

        [TestMethod()]
        public void UT_CompSet_init_Naive()
        {
            boot.Init();
            Mod mod = new Mod();

            // test 0: бетон -> должен быть DP[UNIT_Vol]
            Supl spl = new Supl("ГК Монолит СПб");
            CompSet csb = new CompSet("Товарный бетон", spl);
            Assert.IsNotNull(csb);
            Assert.IsNotNull(csb.csDP);
            Assert.AreEqual(4, csb.csDP.dpStr.Count);
            Assert.IsTrue(csb.csDP.dpar.ContainsKey(SType.UNIT_Vol));

            // test 1: after bug "Ед: руб/т" не попадал в compDP -> compDP содержит SType.UNIT_Weight
            Supl supl = new Supl("ЛенСпецСталь");
            CompSet cs = new CompSet("Полоса", supl);
            Assert.IsTrue(cs.csDP.dpar.ContainsKey(SType.UNIT_Weight));


            // test 2: Check if all Rules have CompSet with Section Unit_
            mod = mod.sr.SetModel(boot, initSupl: true);
            Assert.IsTrue(mod.Rules.Count > 0);
            Rule.Rule r = mod.Rules.ToList()[3];

            foreach (var rule in mod.Rules)
            {
                var csDP = rule.CompSet.csDP;
                bool bw = csDP.dpStr.ContainsKey(SType.UNIT_Weight);
                bool bv = csDP.dpStr.ContainsKey(SType.UNIT_Vol);
                Assert.IsTrue(bw || bv);
            }

            FileOp.AppQuit();
        }
    }
}