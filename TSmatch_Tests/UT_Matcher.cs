using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Matcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mtch = TSmatch.Matcher.Mtch;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using Rule = TSmatch.Rule.Rule;
using Comp = TSmatch.Component.Component;

using SType = TSmatch.Section.Section.SType;

using IMIT = TSmatch.Unit_Tests.Imitation._UT_Imitation;

namespace TSmatch.Matcher.Tests
{
    [TestClass()]
    public class MtchTests
    {
        [TestMethod()]
        public void MtchTest_C245()
        {
            //arrange Group, Components, CompSet, Rule
            var Im = new IMIT();
            var rule = Im.IM_Rule();
            var syns = rule.synonyms[SType.Profile].ToList();
            Assert.AreEqual(syns[0], "угoлoк");
            Assert.AreEqual(syns[1], "l");
            Comp comp = rule.CompSet.Components[1];
            Assert.AreEqual(comp.Str(SType.Profile), "Уголок 20x5");
            Assert.AreEqual(comp.compDP.dpar[SType.Profile], "угoлoк20x5");
            var gr = Im.IM_Group("C245");
            Assert.AreEqual(gr.mat, "c245");
            Assert.AreEqual(gr.prf, "l20x5");
            //act
            Mtch m = new Mtch(gr, rule);
            //assert
            Assert.AreEqual(m.ok, Mtch.OK.Match);
            Assert.AreEqual(m.group.guids.Count, 2);
            Assert.AreEqual(m.component.Str(SType.Profile), "Уголок 20x5");
            Assert.AreEqual(m.group.totalPrice, 2020);
        }
        [TestMethod()]
        public void MtchTest_B20()
        {
            var Im = new IMIT();
            var rule = Im.IM_Rule("Mat: B*");
            var gr = Im.IM_Group("B20");    //Prf "1900x1600" set in Im
            Assert.AreEqual(gr.mat, "b20");
            Assert.AreEqual(gr.prf, "1900x1600");

            Mtch m = new Mtch(gr, rule);

            Assert.AreEqual(m.ok, Mtch.OK.NoMatch);
        }
    }
}