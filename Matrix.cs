/*---------------------------------------------------------------------------------
* Matrix -- базовый класс для хранения и работы со структурами данных в памяти C#
*
*  2.8.2017 П.Л.Храпкин 
*
*--- Unit Tests ---
* 2017.08.2 UT_iEOL OK
*---журнал---
* 2.1.2016 переписан из предыдущей версии, добовлен Indexer
* 28.2.16 добавлен Double; в Int удаляются разделители тысяч
* 15.7.17 minor cleanup
*  2.8.17 iEOL modified to decrease iEOL if empty rows are at the bottom
*----------------------------------------------------------------------------------
*   Конструкторы:
* Matr()            - пустой, для инициирования внутреннего массива по умолчанию
* Matr(object[,])   - копирует матрицу object[,] о внутренний массив
* Matr(DataTable)   - копирует значения из DataTable во внутренний массив
*   МЕТОДЫ:
* public {get;set} с индексированием и проверкой допустимости индексов
* Strng(i,j)          - возвращает значение строки в [i,j]
* Int(i,j[,msg])       - возвращает значение Int в ячейке [i,j]. При ошибке выводит сообщение msg
* iEOL(), iEOC()       - возвращает количество строк и колонок во внутреннем массиве соответственно
* DataTable DaTab()    - копирует текущую матрицу в DataTable
* object[,] TabDa(DataTable) - возвращает матрицу значений. копируя ее из аргумента DataTable
*/

using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

using Log = match.Lib.Log;
using System.Xml.Serialization;

namespace match.Matrix
{
    public class Matr : Object
    {
        private object[,] _matr = new object[100, 100];

        public object this[int i, int j]
        {
            get
            {
                try { var v = _matr[i, j]; }
                catch { Log.FATAL("ошибка при обращении get к Matr[" + i + "," + j + "]"); }
                return _matr[i, j];
            }
            set
            {
                try { var v = _matr[i, j]; }
                catch { Log.FATAL("ошибка при обращении set к Matr[" + i + "," + j + "]"); }
                _matr[i, j] = value;
            }
        }
        public Matr() { }
        public Matr(object[,] obj)
        {
            _matr = obj;
        }
        /// <summary>
        /// Matr(string[]) - инициализация объектного массива с первой строкой из шапки. Специально для Excel с 1!!
        /// </summary>
        /// <history>10.1.2016 PKh</history>
        /// <param name="str">массив текстовых строк - заголовкий колонок</param>
        public Matr(string[] str)
        {
            int[] size = { 1, str.Length};
            int[] lbnd = { 1, 1 };
            Array m = Array.CreateInstance(typeof(Object), size, lbnd);
            _matr = (object[,]) m;
            int i = 1;
            foreach (var s in str) _matr[1, i++] = s as object;
        }
        ////////public Matr(DataTable dt)
        ////////{
        ////////    try
        ////////    {
        ////////        foreach (DataRow row in dt.Rows)
        ////////        {
        ////////            int rw = 0;
        ////////            object obj;
        ////////            for (int col = 0; col <= dt.Columns.Count; col++)
        ////////            {
        ////////                obj = row[col];
        ////////                _matr[rw++, col] = obj;
        ////////            }
        ////////        }
        ////////    }
        ////////    catch (Exception ex)
        ////////    {
        ////////        string mes = ex.Message;
        ////////    }
        ////////}
        public string Strng(int i, int j)
        {
            var v = _matr[i, j];
            return (v == null) ? "" : v.ToString();
        }
        public int Int(int i, int j, string msg = "wrong int")
        {
            object v = _matr[i, j];
            if (v == null) return 0;
            if (v.GetType() == typeof(int)) { return (int)v; }
            try
            {
                int value;
                string val = v.ToString().Replace(" ", "").Replace(" ", "");
                if (int.TryParse(val, out value)) return value;
                //!!               Log.FATAL(msg);
            }
            catch { }//!!! Log.FATAL(msg); }
            return 0;
        }
        public double Double(int i, int j, string msg = "wrong double")
        {
            object v = _matr[i, j];
            try
            {
                double value;
                string val = v.ToString().Replace(" ", "").Replace(" ", "");
                if (double.TryParse(val, out value)) return value;
            }
            catch { }//!!! Log.FATAL(msg); }
            return 99999999;
        }
        public int iEOL()
        {
            int row_l = _matr.GetUpperBound(0);
            int row_0 = _matr.GetLowerBound(0);
            while (row_l >= row_0)
            {
                if (!isRowEmpty(row_l)) break;
                row_l--;
            }
            return row_l;
        }
        private bool isRowEmpty(int iRow)
        {
            int col_0 = _matr.GetLowerBound(1);
            int col_l = _matr.GetUpperBound(1);
            for (int x = col_0; x <= col_l; x++) if (_matr[iRow, x] != null) return false;
            return true;
        }
        public int iEOC() { return _matr.GetLength(1); }
        public int LBoundR() { return _matr.GetLowerBound(0); }
        public int LBoundC() { return _matr.GetLowerBound(1); }
        public DataTable DaTab()
        {
            DataTable _dt = new DataTable();
            int maxCol = iEOC(), maxRow = iEOL();
            for (int col = 1; col <= maxCol; col++) _dt.Columns.Add();
            for (int rw = 1; rw <= maxRow; rw++)
            {
                _dt.Rows.Add();
                for (int col = 1; col <= maxCol; col++) _dt.Rows[rw - 1][col - 1] = _matr[rw, col];
            }
            return _dt;
        }
        public object[,] TabDa(DataTable dt)
        {
            int icol = 0, jrow = 0;
            foreach (DataColumn col in dt.Columns)
            {
                foreach (DataRow rw in dt.Rows) { _matr[jrow++, icol] = rw[icol]; }
                icol++;
            }
            return _matr;
        }
        //public void AddRow(string[] str)
        //{
        //    int rws = iEOL() + 1, cls = iEOC();
        //    int[] size = { rws, str.Length };
        //    int[] lbnd = { 1, 1 };
        //    dynamic m = Array.CreateInstance(typeof(object), size, lbnd);
        //    for (int i = 1; i < rws; i++)
        //        for (int j = 1; j <= cls; j++)
        //            m[i, j] = _matr[i, j];
        //    int col = 0;
        //    foreach (var s in str) m[rws, ++col] = s;
        //    _matr = (object[,]) m;
        //}
        public void AddRow(dynamic obj)
        {
            int rws = iEOL() + 1, old_cls = iEOC(), cls = old_cls;   //!! Math.Max(old_cls, obj.Length);
            int[] size = { rws, cls };
            int[] lbnd = { 1, 1 };
            dynamic m = Array.CreateInstance(typeof(object), size, lbnd);
            for (int i = 1; i < rws; i++)
                for (int j = 1; j <= old_cls; j++)
                    m[i, j] = _matr[i, j];
                        int col = 1;
                        foreach (var s in obj) m[rws, col++] = s;
            _matr = (object[,])m;
        }
        public void Init(dynamic obj)
        {
            int rws = 1, cls = obj.Length;
            int[] size = { rws, cls }, lbnd = { 1, 1 };
            dynamic m = Array.CreateInstance(typeof(object), size, lbnd);
            int col = 0;
            foreach (var s in obj) m[1, ++col] = s;
            _matr = (object[,])m;
        }
    } //class Matr

    /// <summary>
    /// class Matrix -- взято из http://www.cyberforum.ru/csharp-beginners/thread220862.html
    /// </summary>
    /// <history>5.1.2016 адаптировал П.Храпкин</history>
    public class Matrix<T> where T : new()
    {
        private readonly List<List<T>> _matrix;

        /// <summary>
        /// Cоздание матрицы.
        /// </summary>
        /// <param name="rowsCount">Количество строк.</param>
        /// <param name="columnCount">Количество столбцов.</param>
        public Matrix(int rowsCount = 2, int columnCount = 2)
        {
            ColumnCount = columnCount;
            RowsCount = rowsCount;
            _matrix = new List<List<T>>(rowsCount);
            for (int i = 0; i < rowsCount; i++)
            {
                var list = new List<T>(columnCount);
                for (int j = 0; j < columnCount; j++)
                    list.Add(default(T));
                _matrix.Add(list);
            }
        }

        /// <summary>
        /// Cоздание матрицы.
        /// </summary>
        /// <param name="data">Исходный двумерный массив.</param>
        public Matrix(T[,] data)
        {
            RowsCount = data.GetLength(0);
            ColumnCount = data.GetLength(1);
            _matrix = new List<List<T>>(RowsCount);
            for (int i = 0; i < RowsCount; i++)
            {
                var list = new List<T>(ColumnCount);
                for (int j = 0; j < ColumnCount; j++)
                    list.Add(data[i, j]);
                _matrix.Add(list);
            }
        }

        /// <summary>
        /// Элемент матрицы.
        /// </summary>
        /// <param name="i">Индекс строки.</param>
        /// <param name="j">Индекс столбца.</param>
        /// <returns></returns>
        public T this[int i, int j]
        {
            get { return _matrix[i][j]; }
            set { _matrix[i][j] = value; }
        }

        /// <summary>
        /// Количество строк.
        /// </summary>
        public int RowsCount { get; private set; }

        /// <summary>
        /// Количество столбцов.
        /// </summary>
        public int ColumnCount { get; private set; }

        /// <summary>
        /// Добавить строку.
        /// </summary>
        /// <param name="index">Индекс вставки строки.</param>
        public void AddRow(int index)
        {
            RowsCount++;
            var list = new List<T>(ColumnCount);
            for (int j = 0; j < ColumnCount; j++)
                list.Add(default(T));
            _matrix.Insert(index, list);
        }

        /// <summary>
        /// Добавить столбец.
        /// </summary>
        /// <param name="index">Индекс вставки столбца.</param>
        public void AddColumn(int index)
        {
            ColumnCount++;
            foreach (var list in _matrix)
                list.Insert(index, default(T));
        }

        ////public override string ToString()
        ////{
        ////    var stringBuilder = new StringBuilder();
        ////    for (int i = 0; i < RowsCount; i++)
        ////    {
        ////        for (int j = 0; j < ColumnCount; j++)
        ////        {
        ////            stringBuilder.Append(_matrix[i][j]);
        ////            stringBuilder.Append(" ");
        ////        }
        ////        stringBuilder.AppendLine();
        ////    }
        ////    return stringBuilder.ToString();
        ////}
    } //class Matrix
#if UnitTest_Matrix
            Matr strs=new Matr();
  
            strs[0, 0] = "Hello";
            strs[0, 1] = "world!";
            string s = strs.String(0, 0);
#endif //  UnitTest_Matrix 2.1.2016
#if Versia_2015_an_earlier


namespace match.Matrix
{
    //////public class Matrix
    //////{
    //////    object value;

    //////    public Matrix(object val)
    //////    {
    //////        value = val;
    //////    }

    //////    public object get() {return value;}
    //////    public string ToStr() { return (value == null) ? "" : value.ToString(); }
    //////    public int ToInt(string msg)
    //////    {
    //////        if (value == null) return 0;
    //////        try
    //////        {
    //////            if (value.GetType() == typeof(int)) { return (int)value; }
    //////            int v;
    //////            if (int.TryParse(value.ToString(), out v)) return v;
    //////            Log.FATAL(msg);
    //////        }
    //////        catch { Log.FATAL(msg); }
    //////        return 0;
    //////    }
    //////}

    public class Matr : Object
    {
        private object[,] _matr = new object[100, 100];

        public Matr(object[,] obj)
        {
            _matr = obj;
        }
        public Matr(DataTable dt)
        {
            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    int rw = 0;
                    object obj;
                    for (int col = 0; col <= dt.Columns.Count; col++)
                    {
                        obj = row[col];
                        _matr[rw++, col] = obj;
                    }
                }
            }
            catch (Exception ex)
            {
                string mes = ex.Message;
            }
        }
//#if пока_не_нужно
        public List<object> getRow(int iRow)
        {
            List<object> _row = new List<object>(); 
            for (int i = 0; i < iEOC(); i++) _row.Add(_matr[iRow, i]);
            return _row;
        }
        public List<object> getCol(int col)
        {
            List<object> _col = new List<object>(); 
            for (int i = 0; i < iEOC(); i++) _row.Add(_matr[iRow, i]);
            return _row;
        }
//#endif
        public object get(int i, int j)
        {
            object v = null;
            try { v = _matr[i, j]; }
            catch { Log.FATAL("ошибка при обращении к Matr[" + i + "," + j + "]"); }
            return v;
        }
        public object set(int i, int j)   //2.1.16
        {
            try { _matr[i, j] = this; }
            catch { Log.FATAL("ошибка при обращении к Matr[" + i + "," + j + "]"); }
            return _matr[i, j];
        }
        public string String(int i, int j)
        {
            var v = get(i, j);
            return (v == null) ? "" : v.ToString();
        }
        public int Int(int i, int j, string msg = "wrong int")
        {
            object v = get(i, j);
            if (v == null) return 0;
            if (v.GetType() == typeof(int)) { return (int)v; }
            try
            {
                int value;
                string val = v.ToString();
                if (int.TryParse(val, out value)) return value;
                Log.FATAL(msg);
            }
            catch { Log.FATAL(msg); }
            return 0;
        }
        public int iEOL() { return _matr.GetLength(0); }
        public int iEOC() { return _matr.GetLength(1); }
        public DataTable DaTab()
        {
            DataTable _dt = new DataTable();
            int maxCol = iEOC(), maxRow = iEOL();
            for (int col = 1; col <= maxCol; col++) _dt.Columns.Add();
            for (int rw = 1; rw <= maxRow; rw++)
            {
                _dt.Rows.Add();
                for (int col = 1; col <= maxCol; col++) _dt.Rows[rw - 1][col - 1] = get(rw, col);
            }
            return _dt;
        }
        //public DataTable Add(DataRow rw)
        //{
        //    DataTable _dt = new DataTable();
        //    _dt = this.DaTab();
        //    int rwEOC = rw
        //    return _dt;
        //}
    } // конец класса Matr
#endif //Versia_2015_an_earlier
} //namespace Matrix