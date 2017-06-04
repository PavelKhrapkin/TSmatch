/*=================================
 * Components Unit Test 3.6.2017
 *=================================
 */
 using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSmatch.Component.Tests
{
    [TestClass()]
    public class UT_Component
    {
        [TestMethod()]
        public void UT_Component_isMatch()
        {
            ElmAttSet.Group gr = new ElmAttSet.Group();
            Rule.Rule rule = new Rule.Rule();
            var comp = new Component();

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
            Assert.IsTrue(b);
#if NOT_WORKS_YET   //4/3/17
            //test 3: gr="U10P_8240_97" rule="Профиль: Швеллер = U*П_;" => TRUE
            gr.prf = "U10P_8240_97";
            rule.text = "Профиль: Швеллер = U*П_;";
            rule.ruleDP = new DPar.DPar("Профиль: Швеллер = U*П_;");
            rule.synonyms = rule.RuleSynParse(rule.text);
            comp.compDP = new DPar.DPar("Prf: Швеллер 10П");
            b = comp.isMatch(gr, rule);
            Assert.IsTrue(b);
#endif //NOT_WORKS_YET 4/6/17
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

    }
}