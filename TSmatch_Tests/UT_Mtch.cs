/*=================================
* Match Unit Test 28.11.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using FileOp = match.FileOp.FileOp;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Mod = TSmatch.Model.Model;
using SR = TSmatch.SaveReport.SavedReport;
using MH = TSmatch.Handler.Handler;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using System.Linq;

namespace TSmatch.Matcher.Tests
{
    [TestClass()]
    public class UT_Matcher
    {
        Boot boot = new Boot();
        Mod mod = new Mod();

        [TestMethod()]
        public void UT_Mtch()
        {
            boot.Init();
            var model = mod.sr.SetModel(boot, initSupl: false);
            Assert.IsTrue(model.elmGroups.Count > 0);
            Assert.IsTrue(model.Rules.Count > 0);
            var Rules = model.Rules.ToList();
            var grps = model.elmGroups.ToList();

            // test 1 Уголок L50x5 -> цена 7 209 руб
            Rule.Rule rule = Rules.Find(x => x.sCS.Contains("Уголок"));
            Group.Group gr = grps.Find(x => x.Prf.Contains("L"));
            if (rule != null && gr != null)
            {
                rule.Init();
                var m = new Mtch(gr, rule);
                Assert.IsTrue(gr.totalPrice > 7000);
                double rubPerKg = gr.totalPrice / gr.totalWeight;
                Assert.IsTrue(rubPerKg > 20);
            }

            // test 2 Полоса -30 из Листа ЛСС
            rule = Rules.Find(x => x.sCS.Contains("Лист"));
            gr = grps.Find(x => x.Prf.Contains("—"));
            if (rule != null && gr != null)
            {
                rule.Init();
                var m = new Mtch(gr, rule);
                Assert.IsTrue(gr.totalPrice > 7000);
                double rubPerKg = gr.totalPrice / gr.totalWeight;
                Assert.IsTrue(rubPerKg > 20);
            }

            // test 3 Бетон
            rule = Rules.Find(x => x.sCS.Contains("бетон"));
            gr = grps.Find(x => x.mat.Contains("b"));
            if (rule != null && gr != null)
            {
                rule.Init();
                var m = new Mtch(gr, rule);
                Assert.IsTrue(gr.totalPrice > 7000);
                double rubPerM3 = gr.totalPrice / gr.totalVolume; //.totalWeight;
                Assert.IsTrue(rubPerM3 > 2000);
            }
            //foreach (var gr in model.elmGroups)
            //{
            //    Assert.IsTrue(model.Rules.Count > 0);
            //    foreach (var rule in model.Rules)
            //    {
            //        Assert.IsNotNull(rule.CompSet.Supplier);
            //        Assert.IsTrue(rule.CompSet.Components.Count > 0);
            //        Mtch _match = new Mtch(gr, rule);
            //    }
            //}
            FileOp.AppQuit();
        }
    }
}