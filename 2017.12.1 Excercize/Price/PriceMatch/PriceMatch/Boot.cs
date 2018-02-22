/* ---------------------------------------------------------------------
 * Bootstrap - initialisation major global classes on application start
 * 
 * 22.02.2018 Pavel Khrapkin
 * 
 * --- History: ---
 *  5.02.18 transfer and adapted from Excercize/ReadPriceList
 *  7.02.18 use Properties.Setting.setting for getHpath()
 *  9.02.18 Boot.Init() in the main no-paramenter computer
 * 10.02.18 check TOCdir bug fix
 * 11.02.18 ExcelDir global string add
 * 14.02.18 public List<SuppliersInit> add to be loaded from SuppliersInit.xml
 * 15.02.18 move function getPriceLists into SuppliersInit to read AllSupliers
 * 22.02.18 stack overflow fixed: Boot init Suppliers, which init Boot
 * 
 * --- UT_Bootstrap: UT_Boot, UT_getHpath
 * --- Methods: ---
 * Boot()   - Initial setting of global fields
 - getHpath()   - setup Hpath Dictionary taken from Properties,
                  check if TOCdir and ExcelDir are correct
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace PriceMatch
{
    public class Boot
    {
        public string VERSION = Properties.Settings.Default.VERSION;
        public string TOCdir, ModelDir, DebugDir, TMPdir, macroDir, IFCschema, ExcelDir;
        public Suppliers Suppliers;
        public List<SupplierInit> ssInit;
        public Section Sect = new Section();
        public Document Doc = new Document();
        public Message Msg = new Message();
        public FileOp fo = new FileOp();
        public Lib Lib = new Lib();
        //8/12/17        public Model mod;
        public bool IsTeklaActive;

        public Boot()
        {
            var HPath = getHpath();
            Doc.Init(this, HPath);
            //8/12            mod = new Model();
            IsTeklaActive = false;  //времянка!!
            Suppliers = new Suppliers();
            Suppliers.Init(this);
        }

        protected Dictionary<string, string> getHpath()
        {
            var Hpath = new Dictionary<string, string>();

            string desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DebugDir = Path.Combine(desktop_path, Properties.Settings.Default.DebugDir);
            TOCdir = Properties.Settings.Default.TOCdir;
            if (!fo.IsDirExist(TOCdir))
            {
                TOCdir = Environment.GetEnvironmentVariable("TSmatch_Dir", EnvironmentVariableTarget.User);
                if (!fo.IsDirExist(TOCdir))
                    throw new FileNotFoundException(TOCdir + "- Edit Properties.Settings or Windows Environment Variable");
            }
            ExcelDir = Properties.Settings.Default.ExcelPriceDir;
            ExcelDir = Path.Combine(TOCdir, ExcelDir);
            if (!fo.IsDirExist(ExcelDir)) throw new FileNotFoundException(ExcelDir);
            ModelDir = DebugDir;

            // -- temporary -- for excercize only --
            //TOCdir = ModelDir = TMPdir = DebugDir = macroDir = IFCschema 
            //    = Path.Combine(desktop_path, "$DebugDir");

            Hpath.Add("#TOC", TOCdir);
            Hpath.Add("#Model", ModelDir);
            Hpath.Add("#Components", ExcelDir);
            Hpath.Add("#TMP", TMPdir);
            Hpath.Add("#DEBUG", DebugDir);
            Hpath.Add("#Macros", macroDir);
            Hpath.Add("#Envir", IFCschema);
            return Hpath;
        }
    }
}