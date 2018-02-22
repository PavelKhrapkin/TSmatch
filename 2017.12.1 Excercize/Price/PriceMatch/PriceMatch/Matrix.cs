/* -----------------------------------------------------
 * Matrix = Mtr - basic class to keep matrix of Objects
 * 
 * 5.02.2018 Pavel Khrapkin
 * 
 * --- History: ---
 *  5.02.2018 - transfer from Exercise/Matrix
 * ------- Constructors and Methods ----------------
 * Mtr(dynamic matrix)  - initialyze Mtr with matrix
 * iEOL()   - find quantity of Rows in matrix
 * iEOC()   - find quantity of columns in matrix
 * x()      - return object in matrix
 * x(i)     - return obj[i]
 * x(i,j)   - return object in row=x, col=y
 */
using System;
using System.Collections.Generic;

namespace PriceMatch
{
    [Serializable]
    public class Mtr
    {
        private dynamic matrix;

        public Mtr() { }
        public Mtr(dynamic matrix)
        {
            this.matrix = matrix;
        }

        public int iEOL()
        {
            int row_l = matrix.GetUpperBound(0);
            int row_0 = matrix.GetLowerBound(0);
            while (row_l >= row_0)
            {
                if (!isRowEmpty(row_l)) break;
                row_l--;
            }
            return row_l;
        }
        private bool isRowEmpty(int iRow)
        {
            int col_0 = matrix.GetLowerBound(1);
            int col_l = matrix.GetUpperBound(1);
            for (int x = col_0; x <= col_l; x++) if (matrix[iRow, x] != null) return false;
            return true;
        }
        public int iEOC() { return matrix.GetLength(1); }

        public int lBoundR() { return matrix.GetLowerBound(0); }
        public int lBoundC() { return matrix.GetLowerBound(1); }
        public int uBoundR() { return matrix.GetUpperBound(0); }
        public int uBoundC() { return matrix.GetUpperBound(1); }

        public object x() { return matrix; }
        public object x(int i) { return matrix.GetValue(i); }
        public object x(int i, int j)
        {
            return matrix.GetValue(i, j);
        }

        public List<object> ToList()
        {
            List<object> result = new List<object>();
            foreach (object obj in matrix) result.Add(obj);
            return result;
        }
    } // end Mtr class
} // end namespace