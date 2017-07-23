using TSmatch.Model;
/*=================================
* Model Unit Test 14.07.2017
*=================================
*/
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FileOp = match.FileOp.FileOp;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using Mod = TSmatch.Model.Model;
using Decl = TSmatch.Declaration.Declaration;
using SR = TSmatch.SaveReport.SavedReport;
using MH = TSmatch.Model.Handler.ModHandler;

namespace TSmatch.Model.Tests
{
    [TestClass()]
    public class UT_Model
    {
        Boot boot;
        Mod model;

        [TestMethod]
        public void UT_SetModel()
        {
            boot = new Bootstrap.Bootstrap();
            model = new Mod();
            model.SetModel(boot);

            Assert.IsTrue(model.name.Length > 0);
            Assert.IsTrue(model.dir.Length > 0);
            Assert.IsTrue(model.dir.Length > 0);
            Assert.IsTrue(model.dir.Contains(@"\"));
            Assert.IsTrue(model.dir.Contains(":"));
            Assert.IsTrue(model.date > Decl.OLD);
            Assert.IsTrue(model.date < DateTime.Now);
            Assert.IsTrue(model.elements.Count > 0);
            Assert.AreEqual(model.elements.Count, model.elementsCount);
            Assert.IsTrue(model.elmGroups.Count > 0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_SetModDir()
        {
            boot = new Bootstrap.Bootstrap();
            model = new Mod();
            model.SetModDir(boot);

            Assert.IsNotNull(model.dir);
            Assert.IsTrue(FileOp.isDirExist(model.dir));
            Assert.IsTrue(model.name.Length > 0);
            Assert.IsTrue(model.elementsCount > 0);
            Assert.IsTrue(model.phase.Length > 0);
            //            Assert.IsTrue(model.date > Decl.OLD & model.date < DateTime.Now);
            //            Assert.IsTrue(model.pricingDate > Decl.OLD & model.pricingDate < DateTime.Now);
            Assert.IsTrue(model.elementsCount > 0);
            //            Assert.AreEqual(32, model.MD5.Length);
            //            Assert.AreEqual(32, model.pricingMD5.Length);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_setCity()
        {
            string str = "Cанкт-Петербург, Кудрово";

            Mod mod = new Mod();
            mod.setCity(str);

            Assert.AreEqual("Cанкт-Петербург", mod.adrCity);
            Assert.AreEqual("Кудрово", mod.adrStreet);
        }

        [TestMethod()]
        public void UT_get_pricingMD5()
        {
            var boot = new Boot();
            var model = new Mod();
            model.sr = new SaveReport.SavedReport();
            model.SetModDir(boot);
            model.elements = model.sr.Raw(model);
            var mh = new MH();
            var grp = mh.getGrps(model.elements);

            string pricingMD5 = model.get_pricingMD5(grp);

            Assert.IsNotNull(pricingMD5);
            Assert.AreEqual(32, pricingMD5.Length);

            FileOp.AppQuit();
        }
    }
}