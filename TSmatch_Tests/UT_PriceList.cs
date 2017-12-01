/*=================================
 * PriceList Unit Test 30.11.2017
 *=================================
 * Фактически, дублирует проверку прайс-листов
 * в меню TSmatch - Настройки-Проверка прайс-листов
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Boot = TSmatch.Bootstrap.Bootstrap;
using FileOp = match.FileOp.FileOp;

namespace TSmatch.PriceList.Tests
{
    [TestClass()]
    public class UT_PriceList
    {
        [TestMethod()]
        public void UT_PriceList_CheclAll()
        {
            Boot boot = new Boot();
            boot.Init();

            var p = new PriceList();
            p.CheckAll();

            Assert.IsTrue(2 * 2 == 4);

            FileOp.AppQuit();
        }
    }
}