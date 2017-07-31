/*=================================
* Match Unit Test 28.7.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FileOp = match.FileOp.FileOp;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Mod = TSmatch.Model.Model;
using SR = TSmatch.SaveReport.SavedReport;
using MH = TSmatch.Handler.Handler;

namespace TSmatch.Matcher.Tests
{
    [TestClass()]
    public class UT_Matcher
    {
        [TestMethod()]
        public void UT_Mtch()
        {
            var boot = new Boot();
            var model = new Mod();
            model.SetModel(boot);
            Assert.IsTrue(model.elements.Count > 0);
            Assert.IsTrue(model.elmGroups.Count > 0);
            model = model.sr.getSavedRules(model, init:true);
            Assert.IsTrue(model.Rules.Count > 0);
            var gr = model.elmGroups[0];
            Assert.IsTrue(model.Rules.Count > 0);

            foreach (var rule in model.Rules)
            {
                Assert.IsNotNull(rule.CompSet.Supplier);
                Assert.IsTrue(rule.CompSet.Components.Count > 0);

                Mtch _match = new Mtch(gr, rule);

                string new_md5 = model.getMD5(model.elements);
 //               Assert.AreEqual(new_md5, md5);
            }

            FileOp.AppQuit();
        }
    }
}