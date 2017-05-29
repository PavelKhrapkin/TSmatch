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
        public void UT_isMatch()
        {
            ElmAttSet.Group gr = new ElmAttSet.Group();
            gr.prf = "U10P_8240_97";
            Rule.Rule rule = new Rule.Rule();
            rule.text = "Профиль: Швеллер = U*П_;";
            rule.ruleDP = new DPar.DPar("Профиль: Швеллер = U*П_;");
            rule.synonyms = rule.RuleSynParse(rule.text);
            var comp = new Component();
            comp.compDP = new DPar.DPar("Prf: Швеллер 10П");
            bool b = comp.isMatch(gr, rule);

            Assert.IsTrue(b);
        }
    }
}