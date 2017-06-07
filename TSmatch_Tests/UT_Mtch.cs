/*=================================
 * Match Unit Test 6.6.2017
 *=================================
 *-- ToDo 2017.6.6 пересмотреть старые тесты (OLD 6/6/17) и либо выбросить их совсем, либо обновить
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using FileOp = match.FileOp.FileOp;
using Docs = TSmatch.Document.Document;
using Lib = match.Lib.MatchLib;
using Mod = TSmatch.Model.Model;

namespace TSmatch.Matcher.Tests
{
    [TestClass()]
    public class UT_Mtch
    {
        [TestMethod()]
        public void UT_Match()
        {
            //аналогичный тест выполняет UT_Component_isMtch
            //..но тут обращение на уровень выше в цепочке от ModHandler - Pricing
            var rule = new Rule.Rule();
            var comp = new Component.Component();
            ElmAttSet.Group gr = new ElmAttSet.Group();
            gr.guids = new List<string>() { "My GUID" };
            ElmAttSet.ElmAttSet elm = new ElmAttSet.ElmAttSet();
            elm.guid = gr.guids[0];
            gr.Elements.Add(elm.guid, elm);

            //test 1: gr="L75x6" rule="Профиль: Уголок=L *x*;" comp="Уголок 75 x 5" => OK.Match
            gr.prf = "l75x6";
            rule.text = Lib.ToLat("Профиль: Уголок=L *x*;");
            rule.ruleDP = new DPar.DPar(rule.text);
            rule.synonyms = rule.RuleSynParse(rule.text.ToLower());
            comp.compDP = new DPar.DPar("Prf:Уголок 75 x 6; Price: 32670");
            var cs = new TSmatch.CompSet.CompSet();
            cs.Components = new List<Component.Component>() { comp };
            cs.csDP = new DPar.DPar("Ед: руб./тн");
            rule.CompSet = cs;
            string vs = rule.synonyms[Section.Section.SType.Profile][0];
            string vd = comp.viewComp_(Section.Section.SType.Profile);
            Assert.IsTrue(vd.Contains(vs));
            Assert.AreEqual(comp.compDP.dpar.Count, 2);
            var v = comp.compDP.dpar[Section.Section.SType.Profile];
            Assert.AreEqual(v, "угoлoк75x6");
            bool b = comp.isMatch(gr, rule);
            Assert.IsTrue(b);
            var match = new Mtch(gr, rule);
            Assert.AreEqual(match.ok, Mtch.OK.Match);
        }

        [TestMethod()]
        public void UT_Match_Native()
        {
            var boot = new Bootstrap.Bootstrap();
            var model = new Mod();
            model.SetModel(boot);

            var gr = model.elmGroups[17];
            Assert.AreEqual("l75x6", gr.prf);
            var rule = new Rule.Rule(5);
            rule.Init();
            var match = new Mtch(gr, rule);
            Assert.AreEqual(match.ok, Mtch.OK.Match);
        }
#if OLD // 6/6/2017
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
#endif //OLD 6/6/2017
    }
}