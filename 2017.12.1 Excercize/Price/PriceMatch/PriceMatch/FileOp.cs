/*---------------------------------------------------
 * FileOp - File operations 16.02.2018 Pavel Khrapkin
 * 
 * --- History: ---
 * 18.01.2018 - handle null for Path.Combine
 *  5.02.2018 - transfer to PriceResx
 * 16.02.2018 - transfer to PriceMatch
 * 
 * --- Methods: ---
 * FileOpen(path, name) - open file name or pickup it when running
 * GetSheetValue(SheetN) - read Excel sheet
 * IsDirExist(path) - return true, when file directory found at path
 * IsFileExist(path, file)  - return true when fil exist
 * IsFileExist(pathFile)
 * IsSheetExist(SheetN) - return true, when Sheet exist in recent ope Excel
 */
using System;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace PriceMatch
{
    public class FileOp
    {
        //       public bool IsDirExist(string path) { return Directory.Exists(path); }
        //    public class FileOp
        //    {
        Excel.Application _app = null, tApp = null;
        Excel.Workbooks wbks = null;
        Excel.Workbook _wb = null;
        Excel.Worksheet _sheet = null;

        public void FileOpen(string path, string name)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("File Name");
            try
            {
                _app = (Excel.Application)Marshal.GetActiveObject("Excel.Application");
            }
            catch
            {
                _app = new Excel.Application();
                if (_app == null)
                    throw new Exception("Cannot run Excel Application");
            }
            try { _wb = _app.Workbooks.get_Item(name); }
            catch { _wb = _app.Workbooks.Open(Path.Combine(path, name)); }
        }

        public Mtr GetSheetValue(string sheetN)
        {
            try
            {
                _sheet = _wb.Worksheets[sheetN];
                return new Mtr(_sheet.UsedRange.get_Value());
            }
            catch { return null; }
        }

        public bool IsDirExist(string path) { return Directory.Exists(path); }
        public bool IsFileExist(string path, string name)
        {
            if (path == null || name == null) return false;
            return IsFileExist(Path.Combine(path, name));
        }
        public bool IsFileExist(string pathFile)
        {
            try { return File.Exists(pathFile); }
            catch { return false; }
        }
        public bool IsSheetExist(string SheetN)
        {
            try { return _wb.Names.Item(SheetN) != null; }
            catch { return false; }
        }

        public void AppQuit()
        {
            if (_app != null)
            {
                _app.Quit();
            }
        }
    } // end class
} // end namespace