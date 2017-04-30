using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using IMIT = TSmatch.Unit_Tests.Imitation._UT_Imitation;

using Elm = TSmatch.ElmAttSet.ElmAttSet;
using Mod = TSmatch.Model.Model;

namespace TSmatch.Unit_Tests.UT_Model
{
    [TestClass]
    public class UT_Model
    {
        [TestMethod]
        public void UT_Model_getModJournal()
        {
            Mod mod = new Mod();
            int i = mod.getModJournal("ЗС2", @"C:\TeklaStructuresModels\2016\ЗС2");
        }

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
    }
}
