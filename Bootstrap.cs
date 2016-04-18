/*-----------------------------------------------------------------------------------
 * Bootstrap - provide initial start of TSmatch, when necessary - startup procedure
 * 
 *  18.4.2016  Pavel Khrapkin
 *
 *--- JOURNAL ---
 * 25.3.2016 started 
 * 30.3.2016 - Template, Resource classes, HealthCheck() method
 * 18.4.2016 - Resource recover, when it is absent or obsolete
 * ---------------------------------------------------------------------------
 *      METHODS:
 * HealthCheck()    - return TRUE, when all necessary for TSmatch files found, and for work
 * readZIP(name)    - read from ZIP TSmatch components
 */

using Trimble.Connect.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

using TS = TSmatch.Tekla.Tekla;
using Log = match.Lib.Log;
using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using Docs = TSmatch.Document.Document;
using FileOp = match.FileOp.FileOp;
using Decl = TSmatch.Declaration.Declaration;

namespace TSmatch.Startup
{
    public class Bootstrap
    {
        static bool isTeklaActive = TS.isTeklaActive();
        static string MyPath;
        static string TOCdir;
        static string ComponentsDir = TOCdir + @"\База комплектующих";
        static string TMPdir = "";
        static string macroDir;
        static string ModelDir = ""; 
        static string desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static string DebugDir = desktop_path;

        public static void Bootsrap()
        {
            Log.set("Bootstrap");
            Resource.Start();               // подготавливаем ресурсы - в Bootstrap
            Resource.checkResource(Decl.R_TEKLA);   // проверяем, активна ли Tekla
            Resource.checkResource(Decl.R_TSMATCH); // проверяем, доступен ли файл TSmach.xlsx
            Template.Start();               // подготавливаем #шаблоны - в Bootstrap
            Docs.Start(TOCdir);             // инициируем Документы из TSmatch.xlsx/TOC
            Msg.Start();                    // инициируем Сообщения из TSmatch.xlsx/Messages
            Resource.checkResource();       // проверяем все ресурсы, в частности, актуальность даты в [1,1]          
            Suppliers.Supplier.Start();     // инициируем список Поставщиков из TSmatch.xlsx/Suppliers
            Matcher.Matcher.Start();        // инициируем Правила из TSmatch.xlsx/Matcher
            Model.Model.Start();            // инициируем Журнал Моделей, известных TSmatch в TSmatch.xlsx/Models

            bool ok = true;
            if(MyPath != TOCdir)
            {                   // Copy MyPath\TSmatch.exe TOCdir   (!) тут можно попытаться скопировать из TSmatch.ZIP если он есть в MyPath
                ok = FileOp.CopyFile(MyPath, Decl.TSMATCH_EXE, TOCdir, overwrite: true);
            }
            if (!FileOp.isFileExist(TOCdir, Decl.F_MATCH))
            {                   // Copy MyPath\TSmatch.xlsx TOCdir  (!) тут можно попытаться скопировать из TSmatch.ZIP если он есть в MyPath
                ok = FileOp.CopyFile(MyPath, Decl.F_MATCH, TOCdir, overwrite: true);
            }
            // make macroDir from TOCdir with Decl.BUTTON_DIR
            macroDir = TOCdir.Replace("exceldesign", Decl.BUTTON_DIR);
            Renew(macroDir, Decl.BUTTON_CS);    // Copy MyPath\Decl.BUTTON_CS macroDir
            Renew(macroDir, Decl.BUTTON_BMP);   // Copy MyPath\Decl.BUTTON_BMP macroDir
            Log.exit();
        }
        static bool Renew(string dir, string name)
        {
            bool ok = true;
            if (!FileOp.isFileExist(dir, name) && FileOp.isFileExist(MyPath, name))
                ok = FileOp.CopyFile(MyPath, name, dir);
            if (!ok && FileOp.isFileExist(MyPath, Decl.TSMATCH_ZIP))
            {
                ////using (ZipArchive zip = OpenRead("TSmatch.zip"))
                ////{ }
                ////    zip = ZipArchive.   .Open()
            }
            return ok;
        }
        static bool CopyOrExtract(bool update, string dir, string name)
        {
            bool ok = false;
            if (!update && FileOp.isFileExist(dir, name)) return true;
            bool isZIP = FileOp.isFileExist(MyPath, Decl.TSMATCH_ZIP);
            if (update && !FileOp.isFileExist(MyPath, name) && !isZIP) Msg.F("ERR Bootstrap");
            if (!update) ok = Renew(macroDir, name);
            else
                if (FileOp.isFileExist(MyPath, name)) ok = FileOp.CopyFile(MyPath, name, dir, overwrite: true);
            else { }
            return ok;
        }
        /// <summary>
        /// Template - class for #template handling; it is used to substitute the Document Path with real value in Windows
        ///     #templates listed in Declaration to the moment:
        ///     - #TOC   - Path to TSmatch.xlxs
        ///     - #Model - Path to Tekla Model, used for TSmatchINFO.xlsx path
        ///     - #TMP    - temporary file directory; it could be used for Model transfer
        ///     - #Component - Path to Tekla\Environment\common\exceldesign\База комплектующих
        ///     - #Debug - Path to debug report file
        /// </summary>
        /// <example> Docs.Start(FileDirTemplates, FileDirValues);
        /// </</example>
        /// <journal>30.3.2016</journal>
        public class Template
        {
            public readonly string template;       //#template, for example "#TOC"
            public readonly string templ_val;      //#template value, f.e. "C:/Users/Pavel/Desktop"

            public static List<Template> Templates = new List<Template>();

            public Template(string _template, string _templ_val)
            { template = _template; templ_val = _templ_val; }
            /// <summary>
            /// Bootstrap.Templates.Start -- #templates Path fill
            /// </summary>
            /// <journal> mar-2016 created
            /// 18.4.2016 -- #template Path build without Tekla
            /// </journal>
            public static void Start()
            {
                Log.set("Bootstrap.Template.Start");

                if (isTeklaActive)
                {
                    TOCdir = TS.GetTeklaDir();
                    ModelDir = TS.GetTeklaDir((int)TS.ModelDir.model);
                }
                else
                {
                }
                string[] FileDirTemplates = Decl.TOC_DIR_TEMPLATES;
                string[] FileDirValues = { TOCdir, ModelDir, ComponentsDir, TMPdir, DebugDir };
                int i = 0;
                foreach (var v in FileDirTemplates)
                    Templates.Add(new Template(FileDirTemplates[i], FileDirValues[i++]));
                Log.exit();
            }
        } // end class Template
        public class Resource
        {
            static List<Resource> Resources = new List<Resource>();

            private string type;
            private string name;
            private DateTime date;

            public Resource(string _name, string _type, DateTime _date)
            {
                type = _type; name = _name; date = _date;
            }

            public static void Start()
            {
                Log.set("Bootstrap.Resource.Start");
                setR(Decl.R_TEKLA,     Decl.R_TEKLA_TYPE,     Decl.R_TEKLA_DATE);
                setR(Decl.R_TSMATCH,   Decl.R_TSMATCH_TYPE,   Decl.R_TSMATCH_DATE);
                setR(Decl.R_TOC,       Decl.R_TOC_TYPE,       Decl.R_TOC_DATE);
                setR(Decl.R_MSG,       Decl.R_MSG_TYPE,       Decl.R_MSG_DATE);
                setR(Decl.R_SUPPLIERS, Decl.R_SUPPLIERS_TYPE, Decl.R_SUPPLIERS_DATE);
                setR(Decl.R_MODELS,    Decl.R_MODELS_TYPE,    Decl.R_MODELS_DATE);
                setR(Decl.R_RULES,     Decl.R_RULES_TYPE,     Decl.R_RULES_DATE);
                setR(Decl.R_CONST,     Decl.R_CONST_TYPE,     Decl.R_CONST_DATE);

                Bootstrap.MyPath = System.IO.Directory.GetCurrentDirectory();
                Log.exit();
            }
            private static void setR(string name, string type, string _date)
            {
                DateTime date = Lib.getDateTime(_date);
                Resource res = new Resource(name, type, date);
                Resources.Add(res);
            }
            /// <summary>
            /// checkResource([name]) - check Resource name with the date in [1,1]. Default - check all
            /// </summary>
            /// <param name="name">if name == "" - check all resources</param>
            /// <returns>true if Resource in uptodate</returns>
            /// <journal>31.3.2016
            /// 14.4.2016 - non static overload checkResource()
            /// 17.4.2016 - check Resource data
            /// </journal>
            private bool checkResource()
            {
                bool result = false;
                if (type == Decl.RESOURCE_TYPES.Document.ToString())
                {
                    Docs doc = Docs.getDoc(name, fatal: false);
                    if (doc == null) Recover(Decl.RESOURCE_FAULT_REASON.NoFile);
                }
                switch (name)
                {
                    case Decl.R_TEKLA:
                        result = isTeklaActive;
                        if (!result) Recover(Decl.RESOURCE_FAULT_REASON.NoTekla);
                        break;
                    case Decl.R_TSMATCH:
                        result = FileOp.isFileExist(TOCdir, Decl.F_MATCH);
                        if (!result) Recover(Decl.RESOURCE_FAULT_REASON.NoFile);
                        if (!checkTOCdate(TOCdir, this)) Recover(Decl.RESOURCE_FAULT_REASON.Obsolete);

                        {
                            // тут буду работать с ZIP; они могут содержать нужный файл -- см также в checkTOCdate()
                        }                  
                        break;
                    default:    // all other resources check
                        throw new NotImplementedException();
                        //////////                    default:    //case Decl.R_TOC, Decl.R_SUPPLIERS, Decl.R_MSG, Decl.R_MODELS, Decl.R_RULES
                        //////////                        Resource r = Resources.Find(x => x.name == name);
                        //////////                        if (r == null) Msg.F(err + "Unknown Resource", name);
                        //////////                        Docs doc = Docs.getDoc(name, fatal: false);
                        ////////////!!                        if (doc == null) rr.Recover(name);
                        //////////                        string doc11 = doc.Body.Strng(1, 1);
                        //////////                        DateTime docT = Lib.getDateTime(doc11);
                        ////////////!!                        if (docT < r.date) rr.Recover(docT);
                        ////////////!!                        else result = true;
                        //////////                        break;
                        //////////                } // end switch
                        break;
                }
//!!                if (!result) result = Recover();
                return result;
            }
            public static bool checkResource(string name = "")
            {
                Log.set("Bootstrap.checkResource");
                Resource r = Resources.Find(x => x.name == name);
                if (r == null) Msg.F("Err Bootstrap.checkResource: Unknown Resource", name);
                Log.exit();
                return r.checkResource();
            }
            /// <summary>
            /// Recover(Decl.RESOURCE_FAULT_REASON reason) -- when found inactive or obsolete Resource - Recover it
            /// </summary>
            /// <param name="reason"></param>
            /// <returns>true if Resource recovered successfully</returns>
            /// <journal>17.4.2016</journal>
            private bool Recover(Decl.RESOURCE_FAULT_REASON reason)
            {
                bool result = false;
                switch (reason)
                {
                    case Decl.RESOURCE_FAULT_REASON.NoFile:
                        getFile();
                        break;
                    case Decl.RESOURCE_FAULT_REASON.Obsolete:
                        if(checkTOCdate(desktop_path, this)) break;
                        if(checkTOCdate(desktop_path + Decl.TSMATCH_DIR, this)) break;
                        
                        // 1) проверим, есть ли такой файл в каталоге MyPath
                        // 2) есть ли этот файл на Рабочем столе
                        // 3) есть ли на рабочем столе папка TSmatch
                        // 4) есть ли RAR, ZIP с такоим файлом
                        break;
                    case Decl.RESOURCE_FAULT_REASON.NoTekla:
                        //--- check if Tekla up and running ---
                        // 1. check if(isTeklaActive() == true) ..
                        //      1.1 getTeklaDir(exceldesign)  (default -1)
                        //      1.2 getTeklaDir(ModelDir)     (возвращается в ModelInfo текущая модель, открытая в Tekla)
                        // 2. else return false
                        //---- если пришлю сюда, надо 
                        // 1) принять решение, что работаем без Tekla
                        // 2) найти Pach excheldesign по ТОС, и
                        //      2.1 попытаемся воспользоваться переменнтой среды Windows PATH - в ней должен быть перечислен TOCdir
                        // 3) Model.Dir по последней открытой модели или спросить у пользователя
                        ////TOCdir = @"C:\ProgramData\Tekla Structures\21.1\Environments\common\exceldesign";
                        ////
                        //                       string setEnv = Environment.SetEnvironmentVariable("TSmatch_Dir", TOCdir);

                        TOCdir = Environment.GetEnvironmentVariable(Decl.WIN_TSMATCH_DIR);
                        if(TOCdir == null) Msg.F("Windows Parametr TSmatch_Dir not defined");
                        if (checkResource(Decl.R_TSMATCH)) { result = true; break; }
                        Msg.F("No File TSmatch");
//                        Environment.SetEnvironmentVariable(Decl.WIN_TSMATCH_DIR, gerEnv + @"\База комплектующих");
                        throw new NotImplementedException();
                        break;
                }
                return result;
            }
            private void getFile()
            {
                Log.set("Bootstrap.getFile()");
                bool result = false;
                // 1. check if this file exists on Windows PC Desktop
                if (FileOp.isFileExist(TOCdir, name))
                {
                    Docs.tocStart(desktop_path);
                    result = FileOp.CopyFile(desktop_path, name, TOCdir, overwrite: true);
                    if (result) checkResource();
                }
                string path = desktop_path + "\\" + Decl.TSMATCH_DIR;
                if (FileOp.isDirExist(path)) result = FileOp.CopyFile(path, name, TOCdir, overwrite: true);
                if (result) checkResource();
                //////// 1. Is this file on PC Desktop?
                //////string desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                //////result = FileOp.CopyFile(desktop_path, name, TOCdir, overwrite: true);
                //////if (result) checkResource();
                //////string path = desktop_path + "\\" + Decl.TSMATCH_DIR;
                //////if (FileOp.isDirExist(path)) result = FileOp.CopyFile(path, name, TOCdir, overwrite: true);
                //////if (result) checkResource();
                //////////////bool Directory.
                //////////////if(directory.)
                ////////////////----------------------
                //////////////if (MyPath != TOCdir)
                //////////////{                   // Copy MyPath\TSmatch.exe TOCdir   (!) тут можно попытаться скопировать из TSmatch.ZIP если он есть в MyPath
                //////////////    result = FileOp.CopyFile(MyPath, Decl.TSMATCH_EXE, TOCdir, overwrite: true);
                //////////////}
                // 1) проверим, есть ли такой файл в каталоге MyPath
                // 2) есть ли этот файл на Рабочем столе
                // 3) есть ли на рабочем столе папка TSmatch
                // 4) есть ли RAR, ZIP с такоим файлом
                Log.exit();
            }
            private static bool checkTOCdate(string path, Resource r)
            {
                Docs toc = Docs.tocStart(path);
                DateTime tocD = Lib.getDateTime(toc.Body[1, 1]);
                bool ok = tocD >= r.date;
                if (!ok)
                {
                    toc.Close();
                    FileOp.fileRenSAV(path, Decl.F_MATCH);
                }
                else if(path != TOCdir) FileOp.CopyFile(desktop_path, r.name, TOCdir, overwrite: true);
                return ok;
      //!! еще тут можно попробовать поискать ТSmatch.xlsx в этой же директории в ZIP
            }
        } // end class Resource
        ////static void ZZ()
        ////{
        ////    string zipPath = @"c:\example\start.zip";
        ////    string extractPath = @"c:\example\extract";

        ////    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        ////    {
        ////        foreach (ZipArchiveEntry entry in archive.Entries)
        ////        {
        ////            if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        ////            {
        ////                entry.ExtractToFile(Path.Combine(extractPath, entry.FullName));
        ////            }
        ////        }
        ////    }
        ////}
    } // end class Bootsrap
} // end namespace
