/*=================================
* Match Unit Test 20.8.2017
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

namespace TSmatch.Matcher.Tests
{
    [TestClass()]
    public class UT_Matcher
    {
        [TestMethod()]
        public void UT_Mtch()
        {
            var boot = new Boot();
            var sr = new _SR();
            var model = sr.SetModel(boot, initSupl: true);
            Assert.IsTrue(model.elmGroups.Count > 0);
            Assert.IsTrue(model.Rules.Count > 0);

            foreach (var gr in model.elmGroups)
            {
                Assert.IsTrue(model.Rules.Count > 0);
#if CHECK_MD5
                var mtch = new Mtch(model);
#endif
                foreach (var rule in model.Rules)
                {
                    Assert.IsNotNull(rule.CompSet.Supplier);
                    Assert.IsTrue(rule.CompSet.Components.Count > 0);
#if CHECK_MD5
                    Assert.IsTrue(mtch.OK_MD5());
#endif
                    Mtch _match = new Mtch(gr, rule);
#if CHECK_MD5
                    Assert.IsTrue(mtch.OK_MD5());
                    string new_md5 = model.getMD5(model.elements);
                    Assert.AreEqual(new_md5, model.MD5);
#endif
                }
            }
            FileOp.AppQuit();
        }
    }
    class _SR : SR
    {
        internal Mod _GetSavedRules(Mod model)
        {
            return GetSavedRules(model, init: true);
        }
    } // end interface class _SR for access to SavedReport method
}