/*=================================
 * Group Unit Test 31.8.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Boot = TSmatch.Bootstrap.Bootstrap;
using FileOp = match.FileOp.FileOp;

namespace TSmatch.Group.Tests
{
    [TestClass()]
    public class UT_Group
    {
        Model.Model model = new Model.Model();

        [TestMethod()]
        public void UT_CheckGroups()
        {
            var boot = new Boot();
            model = model.sr.SetModel(boot);
            var gr = new Group();
            Message.Message.Dialog = false;

            gr.CheckGroups(ref model);

            var grps = model.elmGroups;
            int cntUsual = grps.Count(x => x.type == Group.GrType.UsualPrice);
            int cntSpec = grps.Count(x => x.type == Group.GrType.SpecPrice);
            int cntNo = grps.Count(x => x.type == Group.GrType.NoPrice);
            int cntWarn = grps.Count(x => x.type == Group.GrType.Warning);
            Assert.AreEqual(grps.Count(), cntNo + cntSpec + cntUsual + cntWarn);
            bool w = grps.Any(x => x.type == Group.GrType.Warning);
            Assert.IsTrue(w);
 
            FileOp.AppQuit();
        }
    }
}