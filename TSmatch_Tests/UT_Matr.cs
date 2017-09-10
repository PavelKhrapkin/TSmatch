/*=================================
* Matrix Unit Test 2.8.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace match.Matrix.Tests
{
    [TestClass()]
    public class UT_Matr
    {
        [TestMethod()]
        public void UT_iEOL()
        {
            object[,] matr = { { 4, 3, 2 }, { 6, 8, 7 }, { null, -5, null }, { null, null, null }, { null, null, null } };
            Matr m = new Matr(matr);

            Assert.AreEqual(2, m.iEOL());
        }
    }
}