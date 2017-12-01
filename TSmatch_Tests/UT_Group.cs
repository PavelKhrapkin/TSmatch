/*=================================
 * Group Unit Test 30.11.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using ElmGr = TSmatch.Group.Group;
using FileOp = match.FileOp.FileOp;

namespace TSmatch.Group.Tests
{
    [TestClass()]
    public class UT_Group
    {
        Boot boot = new Boot();
        Model.Model model = new Model.Model();
        UT_TSmatch._UT_MsgService U = new UT_TSmatch._UT_MsgService();

        [TestMethod()]
        public void UT_elmGroups()
        {
            boot.Init(false);
            List<Elm> elms = new List<Elm>();
            List<ElmGr> grps = new List<ElmGr>();
            if (boot.isTeklaActive) model.dir = boot.ModelDir;
            else model.dir = boot.DebugDir;
            Assert.IsFalse(model.dir == null);
            elms = model.sr.Raw(model);
            Assert.IsTrue(elms.Count > 0);

            grps = model.mh.getGrps(elms); 

            Assert.IsTrue(grps.Count > 1);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_CheckGroups()
        {
            boot.Init();
            model = model.sr.SetModel(boot);
            var gr = new _Group();

            // test 1: проверка наличия разных материалов в одной группе (Msg.W en и ru)
            string s = gr._CheckGroups(ref model, "en", "W"); 

            var grps = model.elmGroups;
            int cntUsual = grps.Count(x => x.type == Group.GrType.UsualPrice);
            int cntSpec = grps.Count(x => x.type == Group.GrType.SpecPrice);
            int cntNo = grps.Count(x => x.type == Group.GrType.NoPrice);
            int cntWarn = grps.Count(x => x.type == Group.GrType.Warning);
            Assert.AreEqual(grps.Count(), cntNo + cntSpec + cntUsual + cntWarn);
            if (cntWarn > 0)
            {
                bool w = grps.Any(x => x.type == Group.GrType.Warning);
                Assert.IsTrue(w);
                Assert.AreEqual("various materials in Group [4]\r\nprofile=\"900X900\", materials \"B20\", and \"Concrete_Undefined\"", s);             
                s = gr._CheckGroups(ref model, "ru", "W");
                Assert.AreEqual("разные материалы в группе [4]\r\nс профилем \"900X900\", материалы \"B20\" и \"Concrete_Undefined\"", s);
            }

            // test 2: проверка (Msg.F en и ru)
            model.elements.Clear();
            s = gr._CheckGroups(ref model, "en", "F");
            Assert.AreEqual("bad element or group list in model \"ONPZ-RD-ONHP-3314-1075_1.001-CI_3D_Tekla\" ", s);
            s = gr._CheckGroups(ref model, "ru", "F");
            Assert.AreEqual("ошибка списка элементов или списка групп в модели \"ONPZ-RD-ONHP-3314-1075_1.001-CI_3D_Tekla\"", s);

            FileOp.AppQuit();
        }
    }

    internal class _Group : Group
    {
        UT_TSmatch._UT_MsgService U = new UT_TSmatch._UT_MsgService();

        public string _CheckGroups(ref Model.Model model, string sLang = "", string sev="F")
        {
            U.SetLanguage(sLang);
            string result = "";
            string prefix = "Msg." + sev + ": [Group.CheckGroup]: ";
            try { CheckGroups(ref model); }
            catch (Exception e)
            {
                if (e.Message.IndexOf(prefix) == 0)
                {
                    result = e.Message.Substring(prefix.Length);
                }
            }
            return result;
        }
    }
}