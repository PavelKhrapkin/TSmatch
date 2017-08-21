/*=================================
 * Rule Unit Test 21.8.2017
 *=================================
 *-- ToDo 2017.6.7 пересмотреть старые тесты (OLD 6/6/17) и либо выбросить их совсем, либо обновить
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using Mod = TSmatch.Model.Model;
using FileOp = match.FileOp.FileOp;
using SType = TSmatch.Section.Section.SType;

//7/6 using IMIT = TSmatch.Unit_Tests.Imitation._UT_Imitation;


namespace TSmatch.Rule.Tests
{
    [TestClass()]
    public class UT_Rule
    {
        [TestMethod()]
        public void UT_Rule_Constructor()
        {
            var boot = new Bootstrap.Bootstrap();
            var model = new Mod();
            model.sr.SetModel(boot);

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
        public void UT_Rule_SynParse()
        {
            var boot = new Bootstrap.Bootstrap();
            var model = new Mod();
            model.sr.SetModel(boot);

            Rule rule = new Rule(6);
            var Syns = rule.synonyms;
            Assert.AreEqual(2, Syns[SType.Material].Count);
            if (model.name == "")
            {
                Assert.AreEqual("c235", Syns[SType.Material][0]);
                Assert.AreEqual("c245", Syns[SType.Material][1]);
            }
            Assert.AreEqual(3, Syns[SType.Profile].Count);
            Assert.AreEqual("пoлocaгopячeкaтaнaя", Syns[SType.Profile][0]);
            Assert.AreEqual("pl", Syns[SType.Profile][1]);

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
