using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TSmatch.Rule;
using SType = TSmatch.Section.Section.SType;
//31/1 using FPtype = TSmatch.FingerPrint.FingerPrint.type;

using IMIT = TSmatch.Unit_Tests.Imitation._UT_Imitation;


namespace TSmatch.Rule.Tests
{
    [TestClass()]
    public class UT_Rule
    {
        [TestMethod()]
        public void UT_Rule_SynParse()
        {
            var Im = new IMIT();
            var cs = Im.IM_CompSet();

            Rule rule = new Rule("Проф:Уголок равнопол.=L*", cs);
            Assert.AreEqual(rule.synonyms.Count, 1);
            Assert.AreEqual(rule.synonyms.ContainsKey(SType.Profile), true);
            var synLst = rule.synonyms[SType.Profile].ToList();
            Assert.AreEqual(synLst.Count, 2);
            Assert.AreEqual(synLst[1], "l");

            string ld = "Назначение - вспомогательные конструкции;"
                + "M: C235 = C245; Проф: Уголок*";
            rule = new Rule(ld, cs);
            Assert.AreEqual(rule.synonyms.Count, 1);
            Assert.AreEqual(rule.synonyms[SType.Material].Count, 2);
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
