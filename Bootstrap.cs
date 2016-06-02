﻿/*-----------------------------------------------------------------------------------
 * Bootstrap - provide initial start of TSmatch, when necessary - startup procedure
 * 
 *  19.4.2016  Pavel Khrapkin
 *
 *--- History ---
 * 25.3.2016 started 
 * 30.3.2016 - Template, Resource classes, HealthCheck() method
 * 19.4.2016 - Resource recover, when it is absent or obsolete
 * ---------------------------------------------------------------------------
 *      Bootstrap Methods:
 * Bootstrap()      - check all resources and start all other modules
 *      sub-class Resource Methods:
 * Start()              - initiate #templates, which would be used in TOC to adapt TSmatch to the user environment
 * checkResource([name]) - check Resource name or all Resources; if resource is absent or obsolete - Recover
 * Recover(fault reason) - try to restore or recover Resource
 *! checkFile(name, date) - check if file name exists and not obsolete; Recover it if necessary
 *! getFile()           - get file with Resource name to replace or resore the Resource
 * checkTOCdate(dir)   - get TSmatch.xlsx file from directory dir, and check if it is not obsolete
 *! readZIP(name)       - read from ZIP TSmatch components
 *      sub-class Template Methods:
 *! Start()             - fill #templates with and without Tekla
 * getPath(templ)       - return Path for the #template templ
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

using TS = TSmatch.Tekla.Tekla;
using Trimble.Connect.Client;
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
        static string MyPath;               // directory, where TSmatch.exe strated
        static string TOCdir;               // directore, where TSmatch.xlsx located. Usually in Tekla\..\exceldesign
        static string ComponentsDir;        // all price-lists directory
        static string ModelDir = "";        // Model Report catalog
        static string TMPdir = "";          // temporary catalog
        static string macroDir = "";        // directory in Tecla\Environment to store button TSmatch
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
            if (!isTeklaActive) ModelDir = Model.Model.RecentModelDir();
            Log.exit();
        }
        //////////static bool Renew(string dir, string name)
        //////////{
        //////////    bool ok = true;
        //////////    if (!FileOp.isFileExist(dir, name) && FileOp.isFileExist(MyPath, name))
        //////////        ok = FileOp.CopyFile(MyPath, name, dir);
        //////////    if (!ok && FileOp.isFileExist(MyPath, Decl.TSMATCH_ZIP))
        //////////    {
        //////////        ////using (ZipArchive zip = OpenRead("TSmatch.zip"))
        //////////        ////{ }
        //////////        ////    zip = ZipArchive.   .Open()
        //////////    }
        //////////    return ok;
        //////////}
        //////////static bool CopyOrExtract(bool update, string dir, string name)
        //////////{
        //////////    bool ok = false;
        //////////    if (!update && FileOp.isFileExist(dir, name)) return true;
        //////////    bool isZIP = FileOp.isFileExist(MyPath, Decl.TSMATCH_ZIP);
        //////////    if (update && !FileOp.isFileExist(MyPath, name) && !isZIP) Msg.F("ERR Bootstrap");
        //////////    if (!update) ok = Renew(macroDir, name);
        //////////    else
        //////////        if (FileOp.isFileExist(MyPath, name)) ok = FileOp.CopyFile(MyPath, name, dir, overwrite: true);
        //////////    else { }
        //////////    return ok;
        //////////}
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

            internal static void Start()
            {
                Log.set("Bootstrap.Resource.Start");
                setR(Decl.R_TEKLA,          Decl.R_TEKLA_TYPE,          Decl.R_TEKLA_DATE);
                setR(Decl.R_TSMATCH,        Decl.R_TSMATCH_TYPE,        Decl.R_TSMATCH_DATE);
                setR(Decl.R_TOC,            Decl.R_TOC_TYPE,            Decl.R_TOC_DATE);
                setR(Decl.R_MSG,            Decl.R_MSG_TYPE,            Decl.R_MSG_DATE);
                setR(Decl.R_SUPPLIERS,      Decl.R_SUPPLIERS_TYPE,      Decl.R_SUPPLIERS_DATE);
                setR(Decl.R_MODELS,         Decl.R_MODELS_TYPE,         Decl.R_MODELS_DATE);
                setR(Decl.R_RULES,          Decl.R_RULES_TYPE,          Decl.R_RULES_DATE);
                setR(Decl.R_CONST,          Decl.R_CONST_TYPE,          Decl.R_CONST_DATE);
                setR(Decl.R_TSMATCH_EXE,    Decl.R_TSMATCH_EXE_TYPE,    Decl.R_TSMATCH_EXE_DATE);
                setR(Decl.R_BUTTON_CS,      Decl.R_BUTTON_CS_TYPE,      Decl.R_BUTTON_CS_DATE);
                setR(Decl.R_BUTTON_BMP,     Decl.R_BUTTON_BMP_TYPE,     Decl.R_BUTTON_BMP_DATE);

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
            /// 19.4.2016 - check Resource data
            /// </journal>
            private bool checkResource()
            {
                bool ok = false;
                switch (name)
                {
                    case Decl.R_TEKLA:
                        ok = isTeklaActive;
                        if (ok) TOCdir = TS.GetTeklaDir(TS.ModelDir.exceldesign);
                        else Recover(Decl.RESOURCE_FAULT_REASON.NoTekla);
                        break;
                    case Decl.R_TSMATCH:
                        ok = FileOp.isFileExist(TOCdir, Decl.F_MATCH);
                        if (!ok) Recover(Decl.RESOURCE_FAULT_REASON.NoFile);
                        if (!checkTOCdate(TOCdir, this)) Recover(Decl.RESOURCE_FAULT_REASON.Obsolete);
                        break;
                    default:    // all other resources check
                        if (type == Decl.TEMPL_TOC)
                        {
                            Docs doc = Docs.getDoc(name, fatal: false);
                            if (doc == null) Recover(Decl.RESOURCE_FAULT_REASON.NoFile);
                            string doc11 = doc.Body.Strng(1, 1);
                            DateTime docT = Lib.getDateTime(doc11);
                            ok = docT > date;
                            if (!ok) Msg.F("Err Bootstrap.checkResource: Resource Obsolete", name, docT, date);
                        }
                        if (type == Decl.RESOURCE_TYPES.File.ToString()) ok = checkFile(name, date);
                        if (isTeklaActive && type == Decl.RESOURCE_TYPES.TeklaFile.ToString())
                        {
                            ok = checkFile(name, date);
                        }
                        break;
                }
                return ok;
            }
            public static bool checkResource(string name = "")
            {
                Log.set("Bootstrap.checkResource(" + name + ")");
                bool ok = false;
                if (name == "")
                {
                    foreach (var rr in Resources) ok &= rr.checkResource();
                }
                else
                {
                    Resource r = Resources.Find(x => x.name == name);
                    if (r == null) Msg.F("Err Bootstrap.checkResource: Unknown Resource", name);
                    ok = r.checkResource();
                }
                Log.exit();
                return ok;
            }
            /// <summary>
            /// Recover(Decl.RESOURCE_FAULT_REASON reason) -- when found inactive or obsolete Resource - Recover it
            /// </summary>
            /// <param name="reason"></param>
            /// <returns>true if Resource recovered successfully</returns>
            /// <journal>19.4.2016</journal>
            private bool Recover(Decl.RESOURCE_FAULT_REASON reason)
            {
                bool ok = false;
                switch (reason)
                {
                    case Decl.RESOURCE_FAULT_REASON.NoFile:
                        getFile();
                        break;
                    case Decl.RESOURCE_FAULT_REASON.Obsolete:
                        if (checkTOCdate(desktop_path, this)) break;
                        if (checkTOCdate(desktop_path + Decl.TSMATCH_DIR, this)) break;

                        // 1) проверим, есть ли такой файл в каталоге MyPath
                        // 2) есть ли этот файл на Рабочем столе
                        // 3) есть ли на рабочем столе папка TSmatch
                        // 4) есть ли RAR, ZIP с такоим файлом
                        break;
                    case Decl.RESOURCE_FAULT_REASON.NoTekla:
                        TOCdir = Environment.GetEnvironmentVariable(Decl.WIN_TSMATCH_DIR, EnvironmentVariableTarget.User);
                        if (TOCdir == null) Msg.F("Windows Parametr TSmatch_Dir not defined");
                        // other parameters for #template (f.e.ModelDir) get later from TSmatch.xlsx when necessary 
                        break;
                }
                return ok;
            }
            private bool Recover()
            { return Recover(Decl.RESOURCE_FAULT_REASON.NoFile) && Recover(Decl.RESOURCE_FAULT_REASON.Obsolete); }
            /// <summary>
            /// checkFile(name, date) - check if file name exists and it in not obsolete
            /// </summary>
            /// <param name="name">file name with the #template</param>
            /// <param name="date">Last modified date should be later than expected in Resource</param>
            /// <returns></returns>
            /// <journal>19.4.2016</journal>
            private bool checkFile(string name, DateTime date)
            {
                Log.set("Bootstrap.checkFile(" + name + ")");
                bool ok = false;
                if (name.Contains("#"))
                {           // if name contains #template -- replace it with the real Path
                    string[] str = name.Split('\\');
                    string dir = Template.getPath(str[0]);
                    this.name = System.IO.Path.Combine(dir, str[1]);
                }
                ok = FileOp.isFileExist(this.name) && FileOp.getFileDate(this.name) > date;
                if (!ok) ok = Recover();

                //--план--
                //1) split name "/"
                //2) get #template from str[0]
                //3) substitute #template to Path
                //4) check if name exist
                //5) check LastWriteTime -- is obsolete
                //6) if obsolete - getFile ?


                Log.exit();
                return ok;
            }
            private void getFile(string toPath = "")
            {
                Log.set("Bootstrap.getFile()");
                if (toPath == "") toPath = TOCdir;
                bool    ok = getFile(desktop_path, toPath);
                if(!ok) ok = getFile(desktop_path + "\\" + Decl.TSMATCH, toPath);
                if (!ok) Msg.F("Err Bootstrap.getFile: File not found", name);
                Log.exit();
            }
            private bool getFile(string frPath, string toPath)
            {
                bool ok = false;
                string nm = System.IO.Path.GetFileName(name);
                if (FileOp.isFileExist(frPath, nm))
                {
                    ok = FileOp.CopyFile(frPath, nm, toPath, overwrite: true);
                    if (ok) checkResource();
                }
                return ok;
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
                return ok;
            }
        } // end class Resource
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
            /// Bootstrap.Templates.Start -- #templates Path filleng
            /// </summary>
            /// <journal> mar-2016 created
            /// 18.4.2016 -- #template Path build without Tekla
            /// </journal>
            public static void Start()
            {
                Log.set("Bootstrap.Template.Start");

                if (isTeklaActive)
                {
                    //  already set earlier TOCdir   = TS.GetTeklaDir(TS.ModelDir.exceldesign);
                    ModelDir = TS.GetTeklaDir(TS.ModelDir.model);
                    macroDir = TS.GetTeklaDir(TS.ModelDir.macro);
                }
                else
                {
                }
                ComponentsDir = TOCdir + @"\База комплектующих";
                string[] FileDirTemplates = Decl.TOC_DIR_TEMPLATES;
                string[] FileDirValues = { TOCdir, ModelDir, ComponentsDir, TMPdir, DebugDir, macroDir};
                int i = 0;
                foreach (var v in FileDirTemplates)
                    Templates.Add(new Template(FileDirTemplates[i], FileDirValues[i++]));
                Log.exit();
            }
            /// <summary>
            /// getPath(templ) - get value of the Path for #template templ
            /// </summary>
            /// <param name="templ">#template</param>
            /// <returns>value of #template</returns>
            /// <journal>19.4.2016</journal>
            public static string getPath(string templ)
            {
                Log.set("Bootstrap.getPath(" + templ + ")");
                Template tstr = Templates.Find( x => x.template == templ);
                if (templ == null ) Msg.F("not found template", templ);
                Log.exit();
                return tstr.templ_val;
            }
        } // end class Template
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
