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
using MH = TSmatch.Handler.Handler;

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
            Assert.IsTrue(model.elements.Count > 0);
            Assert.IsTrue(model.phase.Length > 0);
            //            Assert.IsTrue(model.date > Decl.OLD & model.date < DateTime.Now);
            //            Assert.IsTrue(model.pricingDate > Decl.OLD & model.pricingDate < DateTime.Now);
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
        public void UT_getMD5()
        {
            var model = new Mod();
            Assert.AreEqual(0, model.elements.Count);

            // test empty list of elements MD5
            string md5 = model.getMD5(model.elements);
            Assert.AreEqual("4F76940A4522CE97A52FFEE1FBE74DA2", md5);
        }


        [TestMethod()]
        public void UT_get_pricingMD5()
        {
            var model = new Mod();
            Assert.AreEqual(0, model.elements.Count);
            Assert.AreEqual(0, model.elmGroups.Count);

            // test empty list of groups pricingMD5
            string pricingMD5 = model.get_pricingMD5(model.elmGroups);
            Assert.AreEqual("5E7AD112B9369E41723DDFD797758E62", pricingMD5);

            var boot = new Boot();
            model.sr = new SaveReport.SavedReport();
            model.SetModDir(boot);
            model.elements = model.sr.Raw(model);
            var mh = new MH();
            var grp = mh.getGrps(model.elements);

            pricingMD5 = model.get_pricingMD5(grp);

            Assert.IsNotNull(pricingMD5);
            Assert.AreEqual(32, pricingMD5.Length);

            FileOp.AppQuit();
        }
    }
}