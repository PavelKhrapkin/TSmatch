/* -----------------------------------------------------
 * UT_Bootstrap 22.02.2018 Pavel Khrapkin
 * 
 * --- History: ---
 *  9.02.18 _UT_Boot класс для тестов protected 
 * 15.02.18 UT
 * 16.02.18 перенес в PriceMatch
 * 
 * --- Тесты: ---
 * UT_Boot  - проверка инициализации Boot() -чтения Suppliers и SuppliersInit
 * UT_getHpath - тест protected getHpath() - читает Hpath из Propetries
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace PriceMatch.Tests
{
    [TestClass()]
    public class UT_Boot
    {
        [TestMethod()]
        public void UT_Boot_Init()
        {
            Boot boot = new Boot();
            Assert.IsTrue(boot.VERSION.Contains("Price"));
            Assert.IsTrue(boot.fo.IsDirExist(boot.TOCdir));
            Assert.IsTrue(boot.fo.IsDirExist(boot.DebugDir));
            Assert.IsTrue(boot.fo.IsDirExist(boot.ExcelDir));
            Assert.IsTrue(boot.Suppliers.AllSuppliers.Count > 0);
            Assert.IsTrue(boot.ssInit.Count > 0);
            Assert.AreEqual(boot.Suppliers.AllSuppliers.Count, boot.ssInit.Count,
                "Не все Поставщики отображаются в SupplierInit.xml");
        }

        [TestMethod()]
        public void UT_getHpath()
        {
            _Boot boot = new _Boot();
            Dictionary<string, string> v = boot._getHpath();

            Assert.IsTrue(boot.VERSION.Contains("PriceMatch v"));
            Assert.IsTrue(v["#DEBUG"].Contains("Desktop\\$DebugDir"));
            Assert.IsTrue(v["#TOC"].Contains("\\common\\exceldesign"));
            Assert.AreEqual(v["#TOC"], boot.TOCdir);
        }
    } // end UT_Boot class

    internal class _Boot : Boot
    {
        internal Dictionary<string, string> _getHpath() => getHpath();
    } // end _Boot class
} // end namespace