/*-----------------------------------------------------------------------------------
 * Bootstrap - provide initial start of TSmatch, when necessary - startup procedure
 * 
 *  10.09.2017  Pavel Khrapkin
 *
 *--- History ---
 * 25.3.2016 started 
 * 30.3.2016 - Template, Resource classes, HealthCheck() method
 * 19.4.2016 - Resource recover, when it is absent or obsolete
 *  2.6.2016 - Add Resource IFC2X3, get this file when Tekla not active
 *  2.7.2016 - FORMS Resours check add
 *  6.8.2016 - Add method Init to initiate the Modules of the C# code
 * 28.11.2016 - HashSet<Rule> Rules is defined here
 * 17.03.2017 - Section.init() with Oleg Turetsky recommendation
 *  6.04.2017 - version for TSM_Select
 *  9.04.2017 - tested and tuned with UnitTest
 * 17.04.2017 - SavedModel class implementd
 *  3.05.2017 - Model Journal initialization
 * 24.05/2017 - Rules and Model journal are not global resources anymore
 * 11.07.2017 - public DibugDir
 * 17.07.2017 - check Property.TSmatch resources
 * 23.08.2017 - IFC init add for ChechIFCguids() method
 * 10.09.2017 - MessageBox on top of SplashScreen
 *  * --- Unit Tests ---
 * 2017.07.15  UT_Bootstrap   OK
 * ---------------------------------------------------------------------------
 *      Bootstrap Methods:
 * Bootstrap()      - check all resources and start all other modules

 * checkResx([name], ResName) - check Resource name
 * Recover(fault reason) - try to restore or recover Resource
 *! checkFile(name, date) - check if file name exists and not obsolete; Recover it if necessary
 *! getFile()           - get file with Resource name to replace or resore the Resource
 * checkTOCdate(dir)   - get TSmatch.xlsx file from directory dir, and check if it is not obsolete
 *! readZIP(name)       - read from ZIP TSmatch components
 *      sub-class Template Methods:
 *! Start()             - fill #templates with and without Tekla
 * getPath(templ)       - return Path for the #template templ
 * initModJournal()     - Model Journal read from TSmatch.xlsx/Models withour initialization
 */

using System;
using System.IO;
using System.Collections.Generic;

using Resx = TSmatch.Properties.TSmatch;
using Decl = TSmatch.Declaration.Declaration;
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using FileOp = match.FileOp.FileOp;
using TS = TSmatch.Tekla.Tekla;
using Ifc = TSmatch.IFC.IFC;
using Msg = TSmatch.Message.Message;
using Docs = TSmatch.Document.Document;
using SType = TSmatch.Section.Section.SType;
using Mod = TSmatch.Model.Model;

namespace TSmatch.Bootstrap
{
    public class Bootstrap
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Bootstrap");

        public readonly bool isTeklaActive = TS.isTeklaActive();
        //----- Directories - TSmatch evironment
        string desktop_path;
        string debug_path;  //7/4 { get { return desktop_path; } }
        string _TOCdir;     // directory, where TSmatch.xlsx located- usualy in Tekla\..\exceldesign
        public string TOCdir
        {
            get { return _TOCdir; }
            private set { _TOCdir = value; }
        }
        public Docs docTSmatch;
        static string ComponentsDir;        // all price-lists directory
        public string ModelDir = "";        // Model Report catalog
        static string TMPdir = "";          // temporary catalog
        static string macroDir = "";        // directory in Tekla\Environment to store button TSmatch             
        static string IFCschema = "";       // IFC2X3.exd in Tekla Environment\common\inp as IFC schema 
        public string DebugDir = string.Empty;

        //------------ Main TSmatch classes --------------------------
        public List<Mod> models;            // CAD model list used in TSmatch, model journal
        public Ifc ifc = new Ifc();         // IFC class reference
        public object classCAD;
        public Mod model;

        public Bootstrap() 
        {
            desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            debug_path = desktop_path;
            if (isTeklaActive)
            {   // if Tekla is active - get Path of TSmatch
                classCAD = new TS();
                _TOCdir = TS.GetTeklaDir(TS.ModelDir.exceldesign);
                ModelDir = TS.GetTeklaDir(TS.ModelDir.model);
                //6/4/17                        macroDir = TS.GetTeklaDir(TS.ModelDir.macro);
                classCAD = new TS();
            }
            else
            {   // if not active - Windows Environment Variable value
                _TOCdir = Environment.GetEnvironmentVariable(Decl.WIN_TSMATCH_DIR,
                    EnvironmentVariableTarget.User);
                ModelDir = desktop_path;
                classCAD = ifc;
            }
            CheckResx(Path.Combine(_TOCdir, "TSmatch.xlsx"), Resx.TSmatch_xlsx);
            //--- initiate Docs with #Templates
            Dictionary<string, string> Templates = new Dictionary<string, string>
                    {
                        {"#TOC",    TOCdir },
                        {"#Model",  ModelDir},
                        {"#Components", TOCdir + @"\База комплектующих"},
                        {"#TMP",    TMPdir},
                        {"#DEBUG",  DebugDir},
                        {"#Macros", macroDir},
                        {"#Envir",  IFCschema}
                    };
            Docs.Start(Templates);
            docTSmatch = Docs.getDoc();
            //--- initiate Messages
            Msg.Init();
            CheckResx("Messages", Resx.Messages);
            //--- iniciate Ifc
            IFCschema = Path.Combine(_TOCdir, @"..\inp\IFC2X3.exp");
            ifc.init(IFCschema);        // инициируем IFC, используя файл схемы IFC - обычно из Tekla
            //--check other Resources and we're in air
            CheckResx("Forms", Resx.Forms);
        }

        Dictionary<ResType, string> ResTab = new Dictionary<ResType, string>()
        {
            {ResType.Date, "Dat" },
            {ResType.File, "Fil" },
            {ResType.Doc , "Doc" }
        };
        enum ResType { Date, File, Doc, Err}
        private void CheckResx(string rName, string rValue)
        {
            ResType type = ResType.Err;
            foreach (var x in ResTab)
            {
                if (!rValue.Contains(x.Value)) continue;
                type = x.Key;
                break;
            }
            int indx = rValue.IndexOf(':') + 1;
            string v = rValue.Substring(indx).Trim();
            switch (type)
            {
                case ResType.Doc:
                    if (!Docs.IsDocExists(rName)) resError(ResErr.NoDoc, rName);
                    break;
                case ResType.File:
                    if (!FileOp.isFileExist(rName)) resError(ResErr.NoFile, rName);
                    break;
                case ResType.Date:
                    DateTime d = Lib.getDateTime(v);
                    Docs doc = Docs.getDoc(rName, fatal:false);
                    if (doc == null) resError(ResErr.NoDoc, rName);
                    string sdd = doc.Body.Strng(1, 1);
                    DateTime dd = Lib.getDateTime(sdd);
                    if (dd < d) resError(ResErr.Obsolete, rName);
                    break;
                default: resError(ResErr.ErrResource, rName); break;
            }
        }
        enum ResErr { ErrResource, NoFile, NoDoc, Obsolete }
        void resError(ResErr errType, string rName)
        {
            switch (errType)
            {
                case ResErr.NoFile:
                    Msg.F("No TSmatch Resource file", rName);
                    break;
                case ResErr.NoDoc:
                    Msg.F("No TSmatch Resource Document", rName);
                    break;
                case ResErr.Obsolete:
                    Msg.F("TSmatch Resource Obsolete", rName);
                    break;
                default: Msg.F("TSmatch internal Resource error", rName);
                    break;
            }
        }
#if OLD //15/7
        /// <summary>
        /// init(name [,arg]) - initiate TSmatch module name
        /// </summary>
        /// <param name="name">name of module</param>
        /// <param name="arg">optional methop paramentr</param>
        /// <returns>return one instnce of 'name' or List<instances> depending on module specifics and arg value</returns>
        /// <history>30.8.2016
        /// 22.11.2016 use RecentModel() instead of Models[0]
        /// 14.12.2016 init(BOOTSTRAP)
        /// 22.12.2016 enum BootInitMode
        /// </history>
        public enum BootInitMode { Bootstrap, Tekla, TSmatch, Model }

        internal object init(BootInitMode mode, string arg = "")
        {
            List<Resource> Resources = new List<Resource>();

            object moduleApp = null;
            switch (mode)
            {
                case BootInitMode.Bootstrap:
                    Resources = Resource.Start();   //Initiate Resorse list with their types and dates
                    Resources.Find(x => x.name == Decl.R_TEKLA).checkResource(Resource.R_name.Tekla);
                    desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    debug_path = desktop_path;
                    if (isTeklaActive)
                    {   // if Tekla is active - get Path of TSmatch
                        classCAD = new TS();
                        _TOCdir = TS.GetTeklaDir(TS.ModelDir.exceldesign);
                        ModelDir = TS.GetTeklaDir(TS.ModelDir.model);
                        //6/4/17                        macroDir = TS.GetTeklaDir(TS.ModelDir.macro);
                        classCAD = new TS();
                    }
                    else
                    {   // if not active - Windows Environment Variable value
                        _TOCdir = Environment.GetEnvironmentVariable(Decl.WIN_TSMATCH_DIR,
                            EnvironmentVariableTarget.User);
                        ModelDir = desktop_path;
                        classCAD = ifc;
                    }
                    //22.12.16 позже, при реинжиниринге TSmatch, нужно будет специально заняться ресурсами - RESX .Net
                    //.. тогда и понадобится эта строчка    Console.WriteLine("IsTeklaActive=" + isTeklaActive + "\tTOCdir=" + TOCdir);
                    Resource.checkResource(Resources, Resource.R_name.TSmatch, TOCdir);

                    Dictionary<string, string> Templates = new Dictionary<string, string>
                    {
                        {"#TOC",    TOCdir },
                        {"#Model",  ModelDir},
                        {"#Components", TOCdir + @"\База комплектующих"},
                        {"#TMP",    TMPdir},
                        {"#DEBUG",  DebugDir},
                        {"#Macros", macroDir},
                        {"#Envir",  IFCschema}
                    };
                    Docs.Start(Templates);
                    docTSmatch = Docs.getDoc();
                    Msg.Start();
                    break;

                    //17/3/2017                    new initSection();
                    recentModel = Model.Model.RecentModel();
                    //            bool flg = Resource.Check();
                    //20/12/16//                  TOCdir = Directory.GetCurrentDirectory();
                    ////////////desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    ////////////DebugDir = desktop_path;
                    ////////////Resource.Start();               // подготавливаем ресурсы - в Bootstrap
                    ////////////Resource.checkResource(Decl.R_TEKLA);   // проверяем, активна ли Tekla
                    ////////////Resource.checkResource(Decl.R_TSMATCH); // проверяем, доступен ли файл TSmach.xlsx
                    ////////////Template.Start();               // подготавливаем #шаблоны - в Bootstrap
                    ////////////Docs.Start(TOCdir);             // инициируем Документы из TSmatch.xlsx/TOC
                    ////////////Msg.Start();                    // инициируем Сообщения из TSmatch.xlsx/Messages
                    ////////////Resource.checkResource();       // проверяем все ресурсы, в частности, актуальность даты в [1,1]          
                    ////////////recentModel = Model.Model.RecentModel();
                    ////////////ModelDir = recentModel.ModelDir();
                    ////////////if (!isTeklaActive)
                    ////////////{
                    ////////////    recentModel = Model.Model.RecentModel();
                    ////////////    ModelDir = recentModel.ModelDir();
                    ////////////    Template.Reset(Decl.TEMPL_MODEL, ModelDir);
                    ////////////    ifc = new Ifc();
                    ////////////    ifc.init(IFCschema);        // инициируем IFC, используя файл схемы IFC - обычно из Tekla
                    ////////////}
     
                case BootInitMode.Tekla:
                    moduleApp = arg == "" ? recentModel : models.Find(x => x.name == arg);
                    if (moduleApp.Equals(null)) moduleApp = Model.Model.newModelOpenDialog(out models);
                    break;
                case BootInitMode.TSmatch:
                    throw new NotImplementedException();
                    //29/11                    moduleApp = suppliers;
                    break;
                case BootInitMode.Model:
                    if (isTeklaActive)
                    {
                        //7/6                        moduleApp = Model.Model.getModelFrTekla();
                        moduleApp = new Model.Model();
                    }
                    else
                    {
                        moduleApp = arg == "" ? recentModel : models.Find(x => x.name == arg);
                        if (moduleApp.Equals(null)) moduleApp = Model.Model.newModelOpenDialog(out models);
                        classCAD = ifc;
                    }
                    break;
            }
            return moduleApp;
        }

        /// <summary>
        /// Resource - internal resource of TSmatch being checked in Bootstrap
        /// </summary>
        /// <history>
        /// 2016.12.14 - re-created without static
        /// </history>
        private class All_Resources
        {
            List<Resource> Resources = new List<Resource>();
            public All_Resources()
            {
                Resources.Add(new Resource(Decl.R_TEKLA,       Decl.R_TEKLA_TYPE,       Decl.R_TEKLA_DATE));
                Resources.Add(new Resource(Decl.R_TSMATCH,     Decl.R_TSMATCH_TYPE,     Decl.R_TSMATCH_DATE));
                Resources.Add(new Resource(Decl.R_TOC,         Decl.R_TOC_TYPE,         Decl.R_TOC_DATE));
                Resources.Add(new Resource(Decl.R_MSG,         Decl.R_MSG_TYPE,         Decl.R_MSG_DATE));
                Resources.Add(new Resource(Decl.R_FORM,        Decl.R_FORM_TYPE,        Decl.R_FORM_DATE));
                Resources.Add(new Resource(Decl.R_SUPPLIERS,   Decl.R_SUPPLIERS_TYPE,   Decl.R_SUPPLIERS_DATE));
                Resources.Add(new Resource(Decl.R_CONST,       Decl.R_CONST_TYPE,       Decl.R_CONST_DATE));
                Resources.Add(new Resource(Decl.R_TSMATCH_EXE, Decl.R_TSMATCH_EXE_TYPE, Decl.R_TSMATCH_EXE_DATE));
                Resources.Add(new Resource(Decl.R_BUTTON_CS,   Decl.R_BUTTON_CS_TYPE,   Decl.R_BUTTON_CS_DATE));
                Resources.Add(new Resource(Decl.R_BUTTON_BMP,  Decl.R_BUTTON_BMP_TYPE,  Decl.R_BUTTON_BMP_DATE));
                Resources.Add(new Resource(Decl.R_IFC2X3,      Decl.R_IFC2X3_TYPE,      Decl.R_IFC2X3_DATE));
            }

            internal void Check(string rName)
            {
                var r = Resources.Find(x => x.name == rName);
                switch (r.type)
                {

                }
        //        r.checkResource(Resources, rName);
                //Resources.Find(x => x.name == Decl.R_TEKLA).checkResource(Resource.R_name.Tekla);
            }
        }
        private class Resource
        {
            public string type;
            public string name;
            private DateTime date;

            public Resource(string _name, string _type, string _dat)
            {
                type = _type;
                name = _name;
                date = Lib.getDateTime(_dat);
            }

            //////internal static List<Resource> Start()
            //////{
            //////    Log.set("Bootstrap.Resource.Start");
            //////    List<Resource> result = new List<Resource>();
            //////    result.Add(new Resource(Decl.R_TEKLA, Decl.R_TEKLA_TYPE, Decl.R_TEKLA_DATE));
            //////    result.Add(new Resource(Decl.R_TSMATCH, Decl.R_TSMATCH_TYPE, Decl.R_TSMATCH_DATE));
            //////    result.Add(new Resource(Decl.R_TOC, Decl.R_TOC_TYPE, Decl.R_TOC_DATE));
            //////    result.Add(new Resource(Decl.R_MSG, Decl.R_MSG_TYPE, Decl.R_MSG_DATE));
            //////    result.Add(new Resource(Decl.R_FORM, Decl.R_FORM_TYPE, Decl.R_FORM_DATE));
            //////    result.Add(new Resource(Decl.R_SUPPLIERS, Decl.R_SUPPLIERS_TYPE, Decl.R_SUPPLIERS_DATE));
            //////    result.Add(new Resource(Decl.R_CONST, Decl.R_CONST_TYPE, Decl.R_CONST_DATE));
            //////    result.Add(new Resource(Decl.R_TSMATCH_EXE, Decl.R_TSMATCH_EXE_TYPE, Decl.R_TSMATCH_EXE_DATE));
            //////    result.Add(new Resource(Decl.R_BUTTON_CS, Decl.R_BUTTON_CS_TYPE, Decl.R_BUTTON_CS_DATE));
            //////    result.Add(new Resource(Decl.R_BUTTON_BMP, Decl.R_BUTTON_BMP_TYPE, Decl.R_BUTTON_BMP_DATE));
            //////    result.Add(new Resource(Decl.R_IFC2X3, Decl.R_IFC2X3_TYPE, Decl.R_IFC2X3_DATE));
            //////    Log.exit();
            //////    return result;
            //////}
            /// <summary>
            /// checkResource([name]) - check Resource name with the date in [1,1]. Default - check all
            /// </summary>
            /// <param name="name">if name == "" - check all resources</param>
            /// <returns>true if Resource in uptodate</returns>
            /// <history>31.3.2016
            /// 14.4.2016 - non static overload checkResource()
            /// 19.4.2016 - check Resource data
            /// 12.12.2016 - Msg.F("TOCdir = "")
            /// 21.12.2016 - 
            /// </history>
            public enum R_name { Tekla, TSmatch }

            internal void checkResource(R_name name, string arg = "")
            {
                bool ok = false;
                switch (name)
                {
                    case R_name.Tekla:
                        //9/4                        ok = TS.isTeklaActive();
                        break;
                    case R_name.TSmatch:
                        throw new NotImplementedException();
                        break;
                }
            }
            internal static bool checkResource(List<Resource> Resources, R_name name, string arg = "")
            {
                bool ok = false;
                switch (name)
                {
                    case R_name.Tekla:
                        ok = TS.isTeklaActive();
                        break;
                    case R_name.TSmatch:
                        string TOCdir = arg;
                        if (String.IsNullOrEmpty(TOCdir)) Msg.F(
                            "\n\r\n\r================================================"
                          + "\n\rTSmatch application is not initiated properly:"
                          + "\n\rTSmatch.xlsx not available, because TOCdir=null."
                          + "\n\r================================================\n\r");
                        ok = FileOp.isFileExist(TOCdir, Decl.F_MATCH);
                        Resource r = Resources.Find(x => x.name == Decl.TSMATCH);

                        //////                     if (!ok) Recover(Decl.RESOURCE_FAULT_REASON.NoFile);
                        //////                     if (!checkTOCdate(TOCdir, this)) Recover(Decl.RESOURCE_FAULT_REASON.Obsolete);
                        break;
                }
                return ok;
            }
        } // end class Resource
#endif // OLD
    } // end class Bootsrap
} // end namespace