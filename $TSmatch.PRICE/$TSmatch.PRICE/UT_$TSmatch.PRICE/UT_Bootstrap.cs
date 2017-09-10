using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.PRICE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSmatch.PRICE.Tests
{
    [TestClass()]
    public class UT_Bootstrap
    {
        [TestMethod()]
        public void UT_Bootstrap_Test1()
        {
            Assert.Fail();
        }
#if OLD // куски FP Unit Test из старого FingetPrint c разбором параметров
        //// 31/3/17 - new FP implemented with PRICE handled price-list recognition
        ////////////////////[TestMethod()]
        ////////////////////public void UT_FP_constr1_Test()
        ////////////////////{
        ////////////////////    // * тест: Уголок constructor1 FP(type.Rule, Проф: L * x *)
        ////////////////////    FP xr2 = new FP(FPtype.Rule, "Проф: L * x *");
        ////////////////////    Assert.AreEqual(xr2.section.type.ToString(), "Profile");
        ////////////////////    Assert.AreEqual(xr2.pars.Count, 1);
        ////////////////////    Assert.AreEqual(xr2.pars[0].par.ToString(), "l*x*");
        ////////////////////    Assert.AreEqual(xr2.txs.Count, 0);

        ////////////////////    // * Уголок с материалом constructor1 FP(type.Rule, Проф: L * x * cт *)");
        ////////////////////    xr2 = new FP(FPtype.Rule, "Проф: L * x * cт *");
        ////////////////////    Assert.AreEqual(xr2.section.type.ToString(), "Profile");
        ////////////////////    Assert.AreEqual(xr2.pars.Count, 1);
        ////////////////////    Assert.AreEqual(xr2.pars[0].par.ToString(), "l*x*cт*");
        ////////////////////    Assert.AreEqual(xr2.txs.Count, 0);

        ////////////////////    // * тест: Бетон -монолит constructor1 FP(type.Rule, М:В*)");
        ////////////////////    FP rule = new FP(FPtype.Rule, "M:B*;");
        ////////////////////    Assert.AreEqual(rule.pars.Count, 1);
        ////////////////////    Assert.AreEqual(rule.typeFP.ToString(), "Rule");
        ////////////////////    Assert.AreEqual(rule.section.type.ToString(), "Material");
        ////////////////////    Assert.AreEqual(rule.pars[0].par.ToString(), "b*");
        ////////////////////    Assert.AreEqual(rule.txs.Count, 0);

        ////////////////////    xr2 = new FP(FPtype.Rule, "Профиль:");
        ////////////////////    Assert.AreEqual(xr2.section.type.ToString(), "Profile");
        ////////////////////    Assert.AreEqual(xr2.pars[0].par.ToString(), string.Empty);

        ////////////////////    //-- CompSet tests
        ////////////////////    FP xr1 = new FP(FPtype.CompSet, "Описание: {3}");
        ////////////////////    Assert.AreEqual(xr1.pars.Count, 1);
        ////////////////////    Assert.AreEqual(xr1.typeFP.ToString(), "CompSet");
        ////////////////////    Assert.AreEqual(xr1.section.type.ToString(), "Description");
        ////////////////////    Assert.AreEqual(xr1.Col(), 3);

        ////////////////////    xr1 = new FP(FPtype.CompSet, "Цена: {4} если нет другого материала в описании");
        ////////////////////    Assert.AreEqual(xr1.pars.Count, 1);
        ////////////////////    Assert.AreEqual(xr1.typeFP.ToString(), "CompSet");
        ////////////////////    Assert.AreEqual(xr1.section.type.ToString(), "Price");
        ////////////////////    Assert.AreEqual(xr1.Col(), 4);
        ////////////////////    Assert.AreEqual(xr1.txs.Count, 0);  // решил tx справа от параметра игнорировать
        ////////////////////}

        ////////////////////[TestMethod()]
        ////////////////////public void UT_FP_Component_constr2()
        ////////////////////{
        ////////////////////    var Im = new IMIT();
        ////////////////////    var rule = Im.IM_Rule("M:*;Проф:Уголок равнопол.=Уголок *x*c*");
        ////////////////////    var sec = new Sec("M:");

        ////////////////////    var fpMat = new FP("Уголок 75х6 6м Ст3пс/сп5", rule, sec);

        ////////////////////    //////////////////var csFPmat = new FP(FPtype.CompSet, "M:{2}");
        ////////////////////    // 27/3 //////////var csFPprf = new FP(FPtype.CompSet, "Prf:Уголок {1}");
        ////////////////////    //////////////////var csFPs = new List<FP> { csFPmat, csFPprf };

        ////////////////////    // 23.3.2017 пока решил не позволять использовать несколько параметров в строке
        ////////////////////    //xr1 = new FP(FPtype.CompSet, "Цена: {4} НДС {12}{14}%");
        ////////////////////    //Assert.AreEqual(xr1.pars.Count, 3);
        ////////////////////    //Assert.AreEqual(xr1.txs[3], "%");
        ////////////////////    //Assert.AreEqual(xr1.Col(2), 14);
        ////////////////////    //Assert.AreEqual(xr1.txs.Count, 4);
        ////////////////////    //Assert.AreEqual(xr1.typeFP.ToString(), "CompSet");
        ////////////////////    //Assert.AreEqual(xr1.section.type.ToString(), "Price");
        ////////////////////    //Assert.AreEqual(xr1.Col(), 4);
        ////////////////////    //var sec = new Sec("Prf: L12x5");
        ////////////////////    //19/3            FP fp = new FP(sec);
        ////////////////////    Assert.Fail();
        ////////////////////}
#endif  // OLD
    }
}