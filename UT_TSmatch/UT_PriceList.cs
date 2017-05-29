/*=================================
 * PriceList Unit Test 29.5.2017
 *=================================
 * Фактически, дублирует проверку прайс-листов
 * в меню TSmatch - Настройки-Проверка прайс-листов
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSmatch.PriceList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Decl = TSmatch.Declaration.Declaration;
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

            var p = new PriceList();
            p.CheckAll();

            Assert.IsTrue(2 * 2 == 4);

            FileOp.AppQuit();
        }
    }
}