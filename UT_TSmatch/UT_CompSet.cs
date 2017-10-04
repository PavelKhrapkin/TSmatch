/*=================================
 * CompSet Unit Test 3.10.2017
 *=================================
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Supl = TSmatch.Suppliers.Supplier;
using Comp = TSmatch.Component.Component;
using SType = TSmatch.Section.Section.SType;
using DP = TSmatch.DPar.DPar;


namespace TSmatch.CompSet.Tests
{
    [TestClass()]
    public class UT_CompSet
    {
        [TestMethod()]
        public void UT_CompSet_init()
        {
            string LoadDescriptor = "M:1; опис:3; профиль:2; цена: 4; Ед: руб/т";
            List<Comp> comps = new List<Comp>()
            {
                new Comp(new DP("Prf:I10")),
                new Comp(new DP("Prf:I20"))
            };
            Supl supl = new Supl("СтальХолдинг", init:false);
            CompSet cs = new CompSet("Балка", supl, LoadDescriptor, comps);
            Assert.AreEqual("Балка", cs.name);
            Assert.AreEqual("1", cs.csDP.dpar[SType.Material]);
            Assert.AreEqual("2", cs.csDP.dpar[SType.Profile]);
            Assert.AreEqual("3", cs.csDP.dpar[SType.Description]);
            Assert.AreEqual("4", cs.csDP.dpar[SType.Price]);
            Assert.AreEqual("СтальХолдинг", cs.Supplier.Name);
            Assert.AreEqual(2, cs.Components.Count);
            Assert.AreEqual("I10", cs.Components[0].Str(SType.Profile));
            Assert.AreEqual("I20", cs.Components[1].Str(SType.Profile));
        }
    }
}