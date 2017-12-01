/*=================================
* Model Unit Test 30.11.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Boot = TSmatch.Bootstrap.Bootstrap;
using FileOp = match.FileOp.FileOp;
using Mod = TSmatch.Model.Model;

namespace TSmatch.Model.Tests
{
    [TestClass()]
    public class UT_Model
    {
        Boot boot = new Boot();
        Mod mod = new Mod();

        [TestMethod()]
        public void UT_setCity()
        {
            string str = "Cанкт-Петербург, Кудрово";;
            mod.setCity(str);
            Assert.AreEqual("Cанкт-Петербург", mod.adrCity);
            Assert.AreEqual("Кудрово", mod.adrStreet);
        }

        [TestMethod()]
        public void UT_getMD5()
        {
            boot.Init();
            Assert.AreEqual(0, mod.elements.Count);

            // test empty list of elements MD5
            string md5 = mod.getMD5(mod.elements);
            Assert.AreEqual("4F76940A4522CE97A52FFEE1FBE74DA2", md5);

            // test getMD5 with Raw()
            mod = mod.sr.SetModel(boot);
            mod.elements = mod.sr.Raw(mod);
            Assert.IsTrue(mod.elements.Count > 0);
            string MD5 = mod.getMD5(mod.elements);
            Assert.AreEqual(32, MD5.Length);
            Assert.IsTrue(MD5 != md5);

            // test -- проверка повторного вычисления MD5
            string MD5_1 = mod.getMD5(mod.elements);
            Assert.AreEqual(MD5_1, MD5);

            FileOp.AppQuit();
        }


        [TestMethod()]
        public void UT_get_pricingMD5()
        {
            Assert.AreEqual(0, mod.elements.Count);
            Assert.AreEqual(0, mod.elmGroups.Count);

            // test empty list of groups pricingMD5
            string pricingMD5 = mod.get_pricingMD5(mod.elmGroups);
            const string EMPTY_GROUP_LIST_PRICINGMD5 = "5E7AD112B9369E41723DDFD797758E62";
            Assert.AreEqual(EMPTY_GROUP_LIST_PRICINGMD5, pricingMD5);

            // test real model and TSmatchINFO.xlsx
            boot.Init();
            mod = mod.sr.SetModel(boot, initSupl: true);
            mod.elements = mod.sr.Raw(mod);
            var grp = mod.mh.getGrps(mod.elements);

            pricingMD5 = mod.get_pricingMD5(grp);

            Assert.IsNotNull(pricingMD5);
            Assert.AreEqual(32, pricingMD5.Length);
            Assert.IsTrue(EMPTY_GROUP_LIST_PRICINGMD5 != pricingMD5);

            FileOp.AppQuit();
        }
    }
}