/*=================================
 * Components Unit Test 23.6.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

using Lib = match.Lib.MatchLib;
using MH = TSmatch.Handler.Handler;
using ElmGr = TSmatch.ElmAttSet.Group;

namespace TSmatch.Component.Tests
{
    [TestClass()]
    public class UT_Component
    {
        MH mod = new MH();
        ElmAttSet.Group gr = new ElmAttSet.Group();
        List<ElmGr> inp = new List<ElmGr>();
        Rule.Rule rule = new Rule.Rule();
        Component comp = new Component();

        [TestMethod()]
        public void UT_Component_checkComp()
        {
            //test 1: gr="PL10*100" rule="Prf: PL=—*x*" comp="PL10x100" => TRUE
            gr.prf = "PL10*100";
            rule.text = "Prf:PL=—*x*";
            rule.ruleDP = new DPar.DPar(rule.text);
            rule.synonyms = rule.RuleSynParse(rule.text);
            comp.compDP = new DPar.DPar("PL10x100");
            bool b = comp.isMatch(gr, rule);
            Assert.IsTrue(b);

            //test 2: gr="PL10*100" rule="Prf: PL=—*x@*" comp="PL10x300" => TRUE
            gr.prf = "pl10*100";
            rule.text = "Prf:PL=—*x@*";
            rule.ruleDP = new DPar.DPar(rule.text);
            rule.synonyms = rule.RuleSynParse(rule.text);
            comp.compDP = new DPar.DPar("Prf:PL10x300");
            Assert.AreEqual(comp.compDP.dpar.Count, 1);
            Assert.AreEqual(comp.compDP.dpar[Section.Section.SType.Profile], "pl10x300");
            b = comp.isMatch(gr, rule);

            //test 3: gr="L75x5" rule="Профиль: Уголок=L *x*;" => TRUE
            gr.prf = "l75x5";
            rule.text = Lib.ToLat("Профиль: Уголок=L *x*;");
            rule.ruleDP = new DPar.DPar(rule.text);
            rule.synonyms = rule.RuleSynParse(rule.text.ToLower());
            comp.compDP = new DPar.DPar("Prf:Уголок 75 x 5");
            string vs = rule.synonyms[Section.Section.SType.Profile][0];
            string vd = comp.viewComp_(Section.Section.SType.Profile);
            Assert.IsTrue(vd.Contains(vs));
            Assert.AreEqual(comp.compDP.dpar.Count, 1);
            var v = comp.compDP.dpar[Section.Section.SType.Profile];
            Assert.AreEqual(v, "угoлoк75x5");
            b = comp.isMatch(gr, rule);
            Assert.IsTrue(b);

            //test 4: gr="I20" rule="Профиль: Балка =I* дл;" comp="Балка 20 дл. 9м Ст3пс5" => TRUE
            gr.Prf = "I20"; gr.prf = "i20";
            rule.text = "Профиль: Балка =I*";
            string comp_txt = "Балка 20";   // <==!!
            rule.ruleDP = new DPar.DPar(rule.text);
            rule.synonyms = rule.RuleSynParse(rule.text);
            var syns = rule.synonyms[Section.Section.SType.Profile].ToList();
            Assert.AreEqual(syns[0], "бaлкa");
            Assert.AreEqual(syns[1], "i");
            comp.compDP = new DPar.DPar("Prf:" + comp_txt);
            Assert.AreEqual(comp.compDP.dpar.Count, 1);
            Assert.AreEqual(comp.compDP.dpStr[Section.Section.SType.Profile], comp_txt);
            b = comp.isMatch(gr, rule);
            Assert.IsTrue(b);

            //test 4-1: gr="I30Ш2" rule="Профиль: Двутавр=I*Ш*" => TRUE
            initGr("I30Ш2");
            initRule("М: C245=C255 ; Профиль: Двутавр=I*Ш*");
            initComp("двутавр 30Ш2");
            b = comp.isMatch(gr, rule);
            Assert.IsTrue(b);

            //test 5: gr="Гн.100x4" rule="Профиль: Швеллер = U*П_;" => TRUE
            initGr("Гн.100x4");
            initRule("Профиль: Гн.*х*");
            initComp("Гн. 100х4");
            b = comp.isMatch(gr, rule);
            Assert.IsTrue(b);

            //test 6: gr="Гн.100x4" rule="Профиль: Швеллер = U*П_;" Comp=M:C345 => FALSE
            initGr("Гн.100x46");
            initRule("Профиль: Уголок=L *x*x*");
            initComp("Гн. 100х4", "C345");
            b = comp.isMatch(gr, rule);
            Assert.IsTrue(!b);
        }

        [TestMethod()]
        public void UT_isOK()
        {
            var comp = new Component();

            string pattern = "*x@*";
            string c = "6x1500x2500";
            string g = "6x94";

            bool ok = comp.isOK(pattern, c, g);
            Assert.IsTrue(ok);
        }

        [TestMethod()]
        public void UT_Component_rulePar()
        {
            // test 1: rp("*x*", "2x1250x2500") => "2", "1250", "2500"
            var comp = new Component();
            string pattern = "*x*x*";
            string str = "2x1250x2500";
            var v = comp.rulePar(pattern, str);
            Assert.AreEqual(v.Count, 3);
            Assert.AreEqual(v[0], "2");
            Assert.AreEqual(v[1], "1250");
            Assert.AreEqual(v[2], "2500");

            // test 2: rp("*x@*x@", "2x1250x2500") => "2", "@1250", "@2500"
            pattern = "*x@*x@";
            v = comp.rulePar(pattern, str);
            Assert.AreEqual(v.Count, 3);
            Assert.AreEqual(v[0], "2");
            Assert.AreEqual(v[1], "@1250");
            Assert.AreEqual(v[2], "@2500");

            // test 3: rp("*x@*x*", "2x1250x2500") => "2", "@1250", "2500"
            pattern = "*x@*x*";
            v = comp.rulePar(pattern, str);
            Assert.AreEqual(v.Count, 3);
            Assert.AreEqual(v[0], "2");
            Assert.AreEqual(v[1], "@1250");
            Assert.AreEqual(v[2], "2500");

            // test 4: rp("*x@*", "6*80") => "6", "@80"
            str = "6*80";
            pattern = "*x@*";
            v = comp.rulePar(pattern, str);
            Assert.AreEqual(v.Count, 2);
            Assert.AreEqual(v[0], "6");
            Assert.AreEqual(v[1], "@80");

            // test 5: rp("*", "6") => "6"
            str = "6";
            pattern = "*";
            v = comp.rulePar(pattern, str);
            Assert.AreEqual(v.Count, 1);
            Assert.AreEqual(v[0], "6");

            // test 6: rp("*x@*", "10x300") => "10", "@300"
            str = "10x300";
            pattern = "*x@*";
            v = comp.rulePar(pattern, str);
            Assert.AreEqual(v.Count, 2);
            Assert.AreEqual(v[0], "10");
            Assert.AreEqual(v[1], "@300");
        }

        private void initGr(string v, string mat = "C245")
        {
            inp.Clear();
            gr.Prf = v;
            gr.prf = Lib.ToLat(v.ToLower().Replace(" ", ""));
            gr.Mat = mat;
            gr.mat = Lib.ToLat(mat.ToLower().Replace(" ", ""));
            inp.Add(gr);
//23/7            mod.elmGroups = inp;
        }

        private void initRule(string v)
        {
            rule.text = v;
            rule.ruleDP = new DPar.DPar(rule.text);
            rule.synonyms = rule.RuleSynParse(rule.text);
        }

        private void initComp(string v, string mat = "C245")
        {
            comp.compDP.dpar.Clear();
            comp.compDP.dpStr.Clear();
            comp.compDP.Ad(Section.Section.SType.Profile, v);
            comp.compDP.Ad(Section.Section.SType.Material, mat);
        }
    }
}