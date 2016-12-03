/*-----------------------------------------------------------------------------------
 * Bootstrap - provide initial start of TSmatch, when necessary - startup procedure
 * 
 *  28.11.2016  Pavel Khrapkin
 *
 *--- History ---
 * 25.3.2016 started 
 * 30.3.2016 - Template, Resource classes, HealthCheck() method
 * 19.4.2016 - Resource recover, when it is absent or obsolete
 *  2.6.2016 - Add Resource IFC2X3, get this file when Tekla not active
 *  2.7.2016 - FORMS Resours check add
 *  6.8.2016 - Add method Init to initiate the Modules of the C# code
 ///!!* 28.11.2016 - HashSet<Rule> Rules is defined here
 * ---------------------------------------------------------------------------
 *      Bootstrap Methods:
 * Bootstrap()      - check all resources and start all other modules
 * Init(name)       - Inuitiate C# code Module name
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
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
using Ifc = TSmatch.IFC.IFC;

namespace TSmatch.Startup   
{
    public class Bootstrap
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Bootstrap");   //(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static bool isTeklaActive = TS.isTeklaActive();
        static string MyPath;               // directory, where TSmatch.exe strated
        static string TOCdir;               // directory, where TSmatch.xlsx located. Usually in Tekla\..\exceldesign
        static string ComponentsDir;        // all price-lists directory
        static string ModelDir = "";        // Model Report catalog
        static string TMPdir = "";          // temporary catalog
        static string macroDir = "";        // directory in Tekla\Environment to store button TSmatch
        static string IFCschema = "";       // IFC2X3.exd in Tekla Environment\common\inp as IFC schema 
        static string desktop_path = string.Empty;
        static string DebugDir = string.Empty;
        //------------ Main TSmatch classes --------------------------
        internal List<Model.Model> models;                      // CAD model list used in TSmatch, model journal
        Ifc ifc;
        private Model.Model recentModel;

        public void start()
        {
            desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DebugDir = desktop_path;
            Log.set("Bootstrap");
//            resource = new Resource();
            Resource.Start();               // подготавливаем ресурсы - в Bootstrap
            Resource.checkResource(Decl.R_TEKLA);   // проверяем, активна ли Tekla
            Resource.checkResource(Decl.R_TSMATCH); // проверяем, доступен ли файл TSmach.xlsx
            Template.Start();               // подготавливаем #шаблоны - в Bootstrap
            Docs.Start(TOCdir);             // инициируем Документы из TSmatch.xlsx/TOC
            Msg.Start();                    // инициируем Сообщения из TSmatch.xlsx/Messages
            Resource.checkResource();       // проверяем все ресурсы, в частности, актуальность даты в [1,1]          
            if (!isTeklaActive)
            {
                recentModel = Model.Model.RecentModel();
                ModelDir = recentModel.ModelDir();
                Template.Reset(Decl.TEMPL_MODEL, ModelDir);
                ifc = new Ifc();
                ifc.init(IFCschema);        // инициируем IFC, используя файл схемы IFC - обычно из Tekla
            }
            Log.exit();
        }
        /// <summary>
        /// init(name [,arg]) - initiate TSmatch module name
        /// </summary>
        /// <param name="name">name of module</param>
        /// <param name="arg">optional methop paramentr</param>
        /// <returns>return one instnce of 'name' or List<instances> depending on module specifics and arg value</returns>
        /// <history>30.8.2016
        /// 22.11.2016 use RecentModel() instead of Models[0]
        /// </history>
        internal object init(string name, string arg = "")
        {         
            object moduleApp = null;
            switch (name)
            {
                case Decl.MODEL:
                    start();
                    moduleApp = arg == "" ? recentModel : models.Find(x => x.name == arg);
                    if (moduleApp.Equals(null)) moduleApp = Model.Model.newModelOpenDialog(out models); 
                    break;
                case Decl.SUPPLIERS:
//29/11                    moduleApp = suppliers;
                    break;
                default:
                    Msg.F("Bootstrap: not defined Module", name);
                    break;
            }
            return moduleApp;
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
                setR(Decl.R_FORM,           Decl.R_FORM_TYPE,           Decl.R_FORM_DATE);
                setR(Decl.R_SUPPLIERS,      Decl.R_SUPPLIERS_TYPE,      Decl.R_SUPPLIERS_DATE);
                setR(Decl.R_MODELS,         Decl.R_MODELS_TYPE,         Decl.R_MODELS_DATE);
                setR(Decl.R_RULES,          Decl.R_RULES_TYPE,          Decl.R_RULES_DATE);
                setR(Decl.R_CONST,          Decl.R_CONST_TYPE,          Decl.R_CONST_DATE);
                setR(Decl.R_TSMATCH_EXE,    Decl.R_TSMATCH_EXE_TYPE,    Decl.R_TSMATCH_EXE_DATE);
                setR(Decl.R_BUTTON_CS,      Decl.R_BUTTON_CS_TYPE,      Decl.R_BUTTON_CS_DATE);
                setR(Decl.R_BUTTON_BMP,     Decl.R_BUTTON_BMP_TYPE,     Decl.R_BUTTON_BMP_DATE);
                setR(Decl.R_IFC2X3,         Decl.R_IFC2X3_TYPE,         Decl.R_IFC2X3_DATE);

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
            /// <history>31.3.2016
            /// 14.4.2016 - non static overload checkResource()
            /// 19.4.2016 - check Resource data
            /// </history>
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
            /// <param arg1-arg-3>optional parameters for Recovery</param>
            /// <returns>true if Resource recovered successfully</returns>
            /// <history>19.4.2016
            ///  2.6.2016 - get Environment\common\inp for IFC.exd
            /// </history>
            private bool Recover(Decl.RESOURCE_FAULT_REASON reason, string arg1 = "", string arg2 = "", string arg3 = "")
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
                        // -- action plan --
                        // 1) проверим, есть ли такой файл в каталоге MyPath
                        // 2) есть ли этот файл на Рабочем столе
                        // 3) есть ли на рабочем столе папка TSmatch
                        // 4) есть ли RAR, ZIP с такоим файлом
                        throw new NotImplementedException();
                        break;
                    case Decl.RESOURCE_FAULT_REASON.NoTekla:
                        TOCdir = Environment.GetEnvironmentVariable(Decl.WIN_TSMATCH_DIR, EnvironmentVariableTarget.User);
                        if (TOCdir == null && !Recover(Decl.RESOURCE_FAULT_REASON.NoTOCdirEnvVar))
                        {
                            Msg.F("\n\r\n\r================================================================"
                                + "\n\rWindows Parametr TSmatch_Dir not defined. It normally pointed by"
                                + "\n\rWindows Environment Variable. On the virgin new PC you could"
                                + "\n\rtry to setup it manualy. Alternatively, to make it automatically,"
                                + "\n\rput TSmatch.xlsx in the directory, where it should be located,"
                                + "\n\rthan restart the application TSmatch, or run Tekla, to setup it."
                                + "\n\r================================================================\n\r\n\r");
                        }
                        // other parameters for #template (f.e.ModelDir) get later from TSmatch.xlsx when necessary
                        // another Resource - IFC schema take from Tekla Environment; for inactive Tekla - get from TOCdir
                        string inp = Path.Combine(TOCdir, @"..\..", Decl.ENV_INP_DIR, Decl.IFC_SCHEMA);
                        IFCschema = Path.GetFullPath(inp);
                        break;
                    case Decl.RESOURCE_FAULT_REASON.DirRelocation:
                        // -- action plan --
                        //  1) проверить все ресурсы в новом месте
                        //  2) если чего-то не хватает - перенесем из прежнего места
                        //  3) исправим переменную TOCdir в TSmatch и в переменной Registry
                        throw new NotImplementedException();
                        break;
                    case Decl.RESOURCE_FAULT_REASON.NoTOCdirEnvVar:
                        TOCdir = FileOp.fileOpenDir(Decl.F_MATCH);
                        if (TOCdir == null) Msg.F("Bootstrap not found TSmatch.xlsx");
                        ok = true;
                        break;
                }
                return ok;
            }

            private bool recoverTOCdir()
            {
                throw new NotImplementedException();
            }

            private bool Recover()
            { return Recover(Decl.RESOURCE_FAULT_REASON.NoFile) && Recover(Decl.RESOURCE_FAULT_REASON.Obsolete); }
            /// <summary>
            /// checkFile(name, date) - check if file name exists and it in not obsolete
            /// </summary>
            /// <param name="name">file name with the #template</param>
            /// <param name="date">Last modified date should be later than expected in Resource</param>
            /// <returns></returns>
            /// <history>19.4.2016
            /// 4.6.2016 - #template parsing modified for IFC and other "multi-word Dir" templates</history>
            private bool checkFile(string name, DateTime date)
            {
                Log.set("Bootstrap.checkFile(" + name + ")");
                bool ok = false;
                if (name.Contains("#"))
                {           // if name contains #template -- replace it with the real Path
                    string[] str = name.Split('\\');
                    string dir = Template.getPath(str[0]);
                    this.name = Path.Combine(dir, str[str.Length - 1]);
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
            private bool checkTOCdate(string path, Resource r)
            {
                Docs toc = Docs.tocStart(path);
                string realTOCdir = Path.GetDirectoryName(toc.Wb.FullName);
                // read Windows Environment is much faster then setting. So, we don't write Registry without neccesity
                string registryTOCdir = Environment.GetEnvironmentVariable(Decl.WIN_TSMATCH_DIR, EnvironmentVariableTarget.User);
                if (registryTOCdir != realTOCdir || realTOCdir != TOCdir)
                {
                    Environment.SetEnvironmentVariable(Decl.WIN_TSMATCH_DIR, realTOCdir, EnvironmentVariableTarget.User);
                }
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
        ///     - #TOC    - Path to TSmatch.xlxs
        ///     - #Model  - Path to Tekla or IFC Model, used for TSmatchINFO.xlsx path
        ///     - #TMP    - temporary file directory; it could be used for Model transfer
        ///     - #Component - Path to Tekla\Environment\common\exceldesign\База комплектующих
        ///     - #Debug   - Path to debug report file
        ///     - #Macros  - Path to Tekla Macros for TSmatch button definition 
        ///     - #Envir   - Path to Tekla Environments
        /// </summary>
        /// <example> Docs.Start(FileDirTemplates, FileDirValues);
        /// </</example>
        /// <history>30.3.2016
        /// 23.6.2016 - bug fix - unnecessary IFC file name excluded from #Envir template
        /// 18.8.2016 - Add constructor for reset #template functionality 
        /// </history>
        public class Template
        {
            public readonly string template;       //#template, for example "#TOC"
            public readonly string templ_val;      //#template value, f.e. "C:/Users/Pavel/Desktop"

            public static List<Template> Templates = new List<Template>();

            public Template(string _template, string _templ_val)
            { template = _template; templ_val = _templ_val; }
            internal Template(string _template, string _templ_val, string command)
            {
                if (command == "Reset" && this.template == _template) templ_val = _templ_val;
                else Msg.F("Err #template Reset command"); 
            }

            /// <summary>
            /// Bootstrap.Templates.Start -- #templates Path filleng
            /// </summary>
            /// <history> mar-2016 created
            /// 18.4.2016 -- #template Path build without Tekla
            /// </history>
            public static void Start()
            {
                Log.set("Bootstrap.Template.Start");

                if (isTeklaActive)
                {
                    //  already set earlier -- TOCdir   = TS.GetTeklaDir(TS.ModelDir.exceldesign);
                    ModelDir  = TS.GetTeklaDir(TS.ModelDir.model);
                    macroDir  = TS.GetTeklaDir(TS.ModelDir.macro);
                    IFCschema = Path.Combine(TS.GetTeklaDir(TS.ModelDir.environment), Decl.ENV_INP_DIR);
                }
                else
                {
                    // ModelDir = Model.Model.RecentModelDir();  -- do it in Bootstrap.Start, when Models have read in TSmatch.xlsx
                    //                                           -- macroDir not in use without Tekla
                    // IFCschemaDir = Path.Combine(TOCdir, @"..\..", Decl.ENV_INP_DIR); -- made in NoTekla Recovery
                }
                ComponentsDir = TOCdir + @"\База комплектующих";
                string[] FileDirTemplates = Decl.TOC_DIR_TEMPLATES;
                string[] FileDirValues = { TOCdir, ModelDir, ComponentsDir, TMPdir, DebugDir, macroDir, IFCschema};
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
            /// <history>19.4.2016</history>
            public static string getPath(string templ)
            {
                Log.set("Bootstrap.getPath(" + templ + ")");
                Template tstr = Templates.Find( x => x.template == templ);
                if (templ == null ) Msg.F("not found template", templ);
                Log.exit();
                return tstr.templ_val;
            }
            public static void Reset(string templateName, string newValue)
            {
                Template old = Templates.Find(x => x.template == templateName);
                int ind = Templates.IndexOf(old);
                Templates.RemoveAt(ind);
                Template newTemplate = new Template(templateName, newValue);
                Templates.Add(newTemplate);
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
        ////                entry.ExtractToFile(Path.Combine(extractPath, entry.FullName)); 	TSmatch.exe!TSmatch.Program.Main(string[] args)Строка 19	C#

        ////            }
        ////        }
        ////    }
        ////}
    } // end class Bootsrap
} // end namespace
