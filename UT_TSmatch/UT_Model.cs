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
#if old //24/5 move to UT_ModelHandle
        [TestMethod]
        public void UT_Model_getGroup()
        {
            var Im = new IMIT();
            Mod model = Im.IM_Model();
            Dictionary<string, Elm> elements = Im.IM_Elements();
            string id1 = "MyId1", id2 = "MyId2";
            string mat = elements[id1].mat;
            string prf = elements[id1].prf;
            List<string> guids = new List<string> { id1, id2 };

            ElmAttSet.Group gr = new ElmAttSet.Group(elements, mat, prf, guids);

            Assert.AreEqual(gr.totalPrice, elements[id1].price + elements[id2].price);

            model.setElements(elements.Values.ToList());
            model.getGroups();
            Assert.AreEqual(model.elmGroups.Count, 3);
            Assert.AreEqual(model.elmGroups[0].mat, mat);
            Assert.AreEqual(model.elmGroups[0].prf, prf);
            Assert.AreEqual(model.elmGroups[0].totalPrice, 0);
            Assert.AreEqual(model.elmGroups[0].guids.Count, 2);
            Assert.AreEqual(model.elmGroups[1].guids.Count, 1);
            var gds0 = model.elmGroups[0].guids;
            Assert.AreEqual(gds0[0], id1);
            Assert.AreEqual(gds0[1], id2);
            Assert.AreEqual(model.elmGroups[1].totalPrice, 0);

            var grB = Im.IM_Group("B20");
            Assert.AreEqual(grB.mat, "b20");
            Assert.AreEqual(grB.prf, "1900x1600");
            Assert.AreEqual(grB.guids.Count, 2);
        }
#endif // 24/5 moveto UT_ModelHandle
    }
}