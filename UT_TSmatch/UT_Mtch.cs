/*=================================
* Match Unit Test 21.7.2017
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
            var sr = model.sr = new SR();
            model.mh = new MH();
            model.SetModDir(boot);
//23/7            model.dir = model.sr.dir;
//23/7            model.name = sr.name;
            var elements = model.elements = sr.Raw(model);
            model.elmGroups = model.mh.getGrps(model.elements);
            model.sr.getSavedRules();

            string md5 = model.getMD5(model.elements);

            var gr = model.elmGroups[0];
            foreach (var rule in model.Rules)
            {
                Mtch _match = new Mtch(gr, rule);
                string new_md5 = model.getMD5(model.elements);
                Assert.AreEqual(new_md5, md5);
            }

            FileOp.AppQuit();
        }
    }
}