/*=================================
* Match Unit Test 4.8.2017
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
            var model = new Mod();
//4/8.2017            model.SetModel(boot);
            model.SetModDir(boot);
            var sr = new SR();
            model.elements = sr.Raw(model);
            List<Elm> elmCopy = new List<Elm>();
            foreach (Elm elm in model.elements) elmCopy.Add(elm);
            for (int i = 0; i < elmCopy.Count; i++) Assert.AreEqual(elmCopy[i], model.elements[i]);
            int cnt = model.elements.Count;
            string MD5 = model.getMD5(model.elements);
            Assert.IsTrue(cnt > 0);
            string cMD5 = model.getMD5(elmCopy);
            Assert.AreEqual(cMD5, MD5);          
            var mh = new MH();
            model.elmGroups = mh.getGrps(model.elements);
            Assert.IsTrue(model.elmGroups.Count > 0);

            model = sr.getSavedRules(model, init:true);
            Assert.IsTrue(model.Rules.Count > 0);
            foreach (var gr in model.elmGroups)
            {
//4/8                var gr = model.elmGroups[0];
                Assert.IsTrue(model.Rules.Count > 0);

                var mtch = new Mtch(model);

                foreach (var rule in model.Rules)
                {
                    Assert.IsNotNull(rule.CompSet.Supplier);
                    Assert.IsTrue(rule.CompSet.Components.Count > 0);

                    Assert.IsTrue(mtch.OK_MD5());

                    Mtch _match = new Mtch(gr, rule);

                    Assert.IsTrue(mtch.OK_MD5());

                    string new_md5 = model.getMD5(model.elements);
                    Assert.AreEqual(new_md5, MD5);
                }
            }

            FileOp.AppQuit();
        }
    }
}