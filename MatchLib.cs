/*-----------------------------------------------------------------------
 * MatchLib -- библиотека общих подпрограмм проекта match 3.0
 * 
 *  21.02.16 П.Храпкин, А.Пасс
 *  
 * - 20.11.13 переписано с VBA на С#
 * - 1.12.13 добавлен метод ToIntList
 * - 1.1.14 добавлен ToStrList с перегрузками и умолчаниями
 * - 02.01.14 двоичный поиск EOL
 *            добавлен метод getDateTime(dynamic inp)
 * - 11.01.14 EOC - определение числа колонок листа -- ПХ
 * - 12.12.15 переписано для TSmatch, добавлен фрагмент кода FileOp
 * - 22.12.15 вычистил все старые варианты EOL. Теперь только по Body в модуле Matrix
 * - 1.1.2016 overloaded ToStrList(DataTable, int)
 * - 12.1.16 добавлен метод ComputeMD5 
 * - 17.1.16 FileOp код для работы с файловой системой выделен в отдельный файл
 * - 22.1.16 добавил класс TextBoxWriter - систему вывода Log в окно формы
 * - 16.2.16 включил метод IContains(List<string>, v
 * - 21.2.16 убрал static class Matchlib; метод ToLat()
 * -------------------------------------------
 *      ---- методы Match.Lib ----
 * fileOpen(dir, name[,OpenMode]) - открываем файл Excel по имени name в директории Dir, возвращает Workbook
 * isFileExist(name)        - возвращает true, если файл name существует
 * isSheetExist(Wb, name)  - проверяет, есть ли в Workbook Wb лист name 
 * ToIntList(s, separator)  - возвращает List<int>, разбирая строку s с разделителями separator
 * ToStrList(s,[separator]) - возвращает List<string> из Range или из строки s с разделителем
 * IContains(List<string> lst, v) возвращает true, если в списке lst есть строка, содержащаяся в v
 * timeStr()                - возвращает строку, соответствующую текущнму времени
 * ComputeMD5(List>object> obj) - возвращает строку контрольной суммы MD5 по аргументу List<object>
 * ToLat(str) - замена в текстовой строке знаков кириллицы соотв.по написанию знаками латинского алфавита 
 */

using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Excel = Microsoft.Office.Interop.Excel;

using Decl = TSmatch.Declaration.Declaration;   // модуль с константами и определениями

namespace match.Lib
{
    /// <summary>
    /// класс MatchLib  -- библиотека общих подпрограмм
    /// </summary>
    public class MatchLib
    {
        /// <summary>
        /// fileOpen(dir, name[,OpenMode=Open])   - открываем файл Excel по имени name, возвращает Workbook
        /// </summary>
        /// <param dir="dir">каталог, где открываем файл</param>
        /// <param name="name">имя файла</param>
        /// <param optional OpenMode="OpenMode">режим Open. Если не существует - создать </param>
        /// <returns>Excel.Workbook</returns>
        /// <history>11.12.2013
        /// 7.1.14  - единая точка выхода из метода с finally
        /// 12.12.15 - перенес в matchLib, переписал для TSmatch
        /// 2.1.2016 - добавил параметр OpenMode
        /// 4.1.2016 - отделил dir от name
        /// 10.1.16 - bug fix, Log.set/exit balance
        /// </history>
        public static string dirDBs = null;             // имя каталога для Документов и базы данных
        private static Excel.Application _app = null;   // Application Excel
        /// <summary>
        /// ToStrList(Excel.Range)  - возвращает лист строк, содержащийся в ячейках
        /// </summary>
        /// <param name="rng"></param>
        /// <returns>List<streeng></streeng></returns>
        /// <jornal> 1.1.2014 P.Khrapkin
        /// </jornal>
        public static List<string> ToStrList(Excel.Range rng)
        {
            List<string> strs = new List<string>();
            foreach (Excel.Range cell in rng) strs.Add(cell.Text);
            return strs;
        }
        /// <summary>
        /// ToStrList(DataRow, int[] indx)  - возвращает лист строк, содержащийся в ячейках ряда,
        ///                                   указанных в массиве indx - в списке номеров колонок
        /// </summary>
        /// <param name="rw"></param>
        /// <param name="indx"></param>
        /// <returns></returns>
        public static List<string> ToStrList(DataRow rw, int[] indx)
        {
            List<string> strs = new List<string>();
            foreach (int i in indx) strs.Add(rw[i] as string);
            return strs;
        }
        public static List<string> ToStrList(object[] rw, int[] indx)
        {
            List<string> strs = new List<string>();
            foreach (int i in indx) strs.Add(rw[i] as string);
            return strs;
        }
        /// <summary>
        /// ToStrList(DataRow, int)   - возвращает лист строк, содержащийся в ячейке i
        /// </summary>
        /// <param name="rw">входная строка, возможно, содержащая несколько подстрок</param>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <history>10.1.2016 PKh разбор строки с делимитрами по подстрокам</history>
        public static List<string> ToStrList(DataRow rw, int i)
        {
            List<string> strs = new List<string>();
            string[] s = rw[i].ToString().Split(Decl.STR_DELIMITER);
            foreach (string str in s)
            {
                string st = str.Trim();
                if (!string.IsNullOrEmpty(st)) strs.Add(st);
            }
            return strs;
        }
        public static List<string> ToStrList(object[] rw, int i)
        {
            List<string> strs = new List<string>();
            strs.Add(rw[i] as string);
            return strs;
        }
        /// <summary>
        /// overloaded ToStrList(string, [separator = ','])
        /// </summary>
        /// <param name="s"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<string> ToStrList(string s, char separator = ',')
        {
            List<string> strs = new List<string>();
            if (string.IsNullOrEmpty(s)) return null;
            string[] ar = s.Split(separator);
            foreach (var item in ar) strs.Add(item);
            return strs;
        }
        /// <summary>
        /// IContains(List<string> lst, v) возвращает true, если в списке lst есть строка, содержащаяся в v
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IContains(List<string> lst, string v)
        {
            bool flag = false;
            foreach (string s in lst)
                if (v.Contains(s)) { flag = true; break; }
            return flag;
        }
        /// <summary>
        /// если в rng null, пусто или ошибка - возвращаем 0, а иначе целое число
        /// </summary>
        /// <param name="rng"></param>
        /// <returns></returns>
        public static int RngToInt(Excel.Range rng)
        {
            int v = 0;
            try
            {
                string str = rng.Text;
                int.TryParse(str, out v);
            }
            catch { v = 0; }
            return v;
        }
        /// <summary>
        /// ToIntList(s, separator) - разбирает строку s с разделительным символом separatop;
        ///                           возвращает List int найденных целых чисел
        /// </summary>
        /// <param name="s"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        /// <history> 12.12.13 A.Pass
        /// </history>
        public static List<int> ToIntList(string s, char separator)
        {
            string[] ar = s.Split(separator);
            List<int> ints = new List<int>();
            foreach (var item in ar)
            {
                int v;
                if (int.TryParse(item, out v)) ints.Add(v);
            }
            return ints;
        }
        public static int ToInt(string s, string msg = "не разобрана строка")
        {
            int v;
            if (int.TryParse(s, out v)) return v;
            Log.Warning(msg + " \"" + s + "\"");
            return -1;
        }
        /// <summary>
        /// isCellEmpty(sh,row,col)     - возвращает true, если ячейка листа sh[rw,col] пуста или строка с пробелами
        /// </summary>
        /// <param name="sh"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        /// <history> 13.12.13 A.Pass
        /// </history>
        public static bool isCellEmpty(Excel.Worksheet sh, int row, int col)
        {
            var value = sh.UsedRange.Cells[row, col].Value2;
            return (value == null || value.ToString().Trim() == "");
        }
        /// <summary>
        /// getDateTime(dynamic inp)     - возвращает DateTime для любого значения (из ячейки Excel)
        /// </summary>
        /// <param name="inp"></param>
        /// <returns>DateTime</returns>
        /// <history> 02.01.14 A.Pass
        /// 15.1.16 PKh перписано с TryParse, проверено с timeTest. Ох и намаялся..((
        /// </history>
        public static DateTime getDateTime(dynamic inp)
        {
            if (inp == null) return new DateTime(0);
            if (inp.GetType() == typeof(DateTime)) return inp;
            if (inp.GetType() == typeof(string))
            {
                DateTime ret, day, hms;
                int dd, mm, yy, h, m, s;
                if (DateTime.TryParse(inp, out ret)) return ret;
                string[] pDate = inp.Split(' ');
                if (DateTime.TryParse(pDate[0], out day))
                { dd = day.Day; mm = day.Month; yy = day.Year; }
                else return new DateTime(0);
                if (DateTime.TryParse(pDate[1], out hms))
                { h = hms.Hour; m = hms.Minute; s = hms.Second; }
                else { h = m = s = 0; }
                return new DateTime(yy, mm, dd, h, m, s);
            }
            if (inp.GetType() == typeof(Double)) return DateTime.FromOADate(inp);
            return new DateTime(0);
        } // end getDateTime
        public static string timeStr() { return timeStr(DateTime.Now); }
        public static string timeStr(DateTime t) { return t.ToString("d.MM.yy H:mm:ss"); }
        public static string timeStr(DateTime t, string format) { return t.ToString(format); }
#if testComputeMD5
        static void Main(string[] args)
        {
            List<object> lst = new List<object>();
            lst.Add(25);
            lst.Add(null);
            lst.Add(15);
            lst.Add(-1);
            lst.Add("txt");
            lst.Add(null);
            string key = ComputeMD5(lst);
        }
#endif
        /// <summary>
        /// ComputeMD5 - возвращает строку контрольной суммы MD5 по входному списку
        /// </summary>
        /// <param name="obj">входной параметр - список объектов</param>
        /// <returns></returns>
        /// <history>12.1.2016 PKh</history>
        public static string ComputeMD5(List<object> obj)
        {
            string str = "";
            foreach (var v in obj) str += v == null ? "" : v.ToString();
            return ComputeMD5(str);
        }
        public static string ComputeMD5(string s)
        {
            string str = "";
            MD5 md5 = new MD5CryptoServiceProvider();
            for (int i = 0; i < s.Length; i++) str += s[i];
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(data).Replace("-", "");
        }
        /// <summary>
        /// ToLat() - замена в текстовой строке знаков кириллицы аналогичными
        ///           по написалию знаками латинского алфавита       
        /// </summary>
        /// <returns>преобразованная текстовая строка</returns>
        /// <history> 21.2.2016 </history>
        ////// не получилось сделать вызов str.ToLat()
        //////public string ToLat(this string str, bool up = false)
        //////{
        //////    if (up) str = str.ToUpper();
        //////    string s = ToLat(str);
        //////    return s; }
        public static string ToLat(string str)
        {
            const string cyr = "АВЕКМНОРСТХаеорсух";
            const string lat = "ABEKMHOPCTXaeopcyx";
            string lt = "";
            foreach (var s in str)
            {
                char? st = null;
                for (int i = 0; i < cyr.Length; i++)
                    if (s == cyr[i]) { st = lat[i]; break; }
                if (st == null) lt += s; else lt += st;
            }
            return lt;
        }
    }   // конец класса MatchLib
    /// <summary>
    /// Lib - библиотека нестатических методов
    /// </summary>
    /// <history> 21.2.2016 </history>
    /// <summary>
    /// Log & Dump System
    /// </summary>
    /// <history> 30.12.2013 P.Khrapkin
    /// 1.1.2016 в FATAL выводим стек имен
    /// </history>
    public class Log
    {
        private static string _context;
        static Stack<string> _nameStack = new Stack<string>();

        public Log(string msg)
        {
            _context = "";
            foreach (string name in _nameStack) _context = name + ">" + _context;
            _tx(DateTime.Now.ToLongTimeString() + " " + _context + " " + msg);
        }
        public static void set(string sub) { _nameStack.Push(sub); }
        public static void exit() { _nameStack.Pop(); }
        public static void FATAL(string msg)
        {
            new Log("\n\n[FATAL] " + msg);
            _tx("\n\tв стеке имен:");
            foreach (var s in _nameStack) _tx("\t-> " + s);
            System.Diagnostics.Debugger.Break();
        }
        public static void Warning(string msg) { new Log("\n[warning] " + msg); }
        public static void START(string msg)
        {
            Console.WriteLine(DateTime.Now.ToShortDateString() + " ---------< " + msg + " >---------");
        }
        private static void _tx(string tx) { Console.WriteLine(tx); }
    }
    /// <summary>
    /// TextBoxWriter - система отладки с Log в WindowsForm 
    /// из http://devnuances.com/c_sharp/kak-perenapravit-vyivod-konsoli-v-textbox-v-c-sharp/
    /// </summary>
    /// <history> 
    /// 22.1.2016 - безуспешно потратил день на попытки сделать консольный вывод потокобезопасным.
    ///             В итоге вернулся к прежнему коду, когда поток Start, инициализирующий загрузку 
    ///             начальных данные из TSmatch.xlsx приходится делать в потоке WindowsForm, то есть
    ///             практически, без вывода Log. 
    /// </history>
    public class TextBoxStreamWriter : TextWriter
    {
        TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            _output.AppendText(value.ToString());
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    } // end class
}  //end namespace