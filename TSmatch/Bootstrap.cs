/*-----------------------------------------------------------------------------------
 * Bootstrap - provide initial start of TSmatch, when necessary - startup procedure
 * 
 *  3.05.2017  Pavel Khrapkin
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
 * initModJournal()     - Model Journal read from TSmatch.xlsx/Models withour initialization
 */

using System;
using System.Collections.Generic;

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
        //static string desktop_path = string.Empty;
        static string DebugDir = string.Empty;

        //------------ Main TSmatch classes --------------------------
        public List<Mod> models;            // CAD model list used in TSmatch, model journal
        Ifc ifc;
        public object classCAD;
        public Mod model;

        public Bootstrap()
        {
            init(BootInitMode.Bootstrap);
        }
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
                    initModJournal();
                    break;
#if OLD
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
#endif // OLD  7/4/17 
            }
            return moduleApp;
        }

        /// <summary>
        /// Resource - internal resource of TSmatch being checked in Bootstrap
        /// </summary>
        /// <history>
        /// 2016.12.14 - re-created without static
        /// </history>
        private class Resource
        {
            private string type;
            public string name;
            private DateTime date;

            public Resource(string _name, string _type, string _dat)
            {
                type = _type;
                name = _name;
                date = Lib.getDateTime(_dat);
            }

            internal static List<Resource> Start()
            {
                Log.set("Bootstrap.Resource.Start");
                List<Resource> result = new List<Resource>();
                result.Add(new Resource(Decl.R_TEKLA, Decl.R_TEKLA_TYPE, Decl.R_TEKLA_DATE));
                result.Add(new Resource(Decl.R_TSMATCH, Decl.R_TSMATCH_TYPE, Decl.R_TSMATCH_DATE));
                result.Add(new Resource(Decl.R_TOC, Decl.R_TOC_TYPE, Decl.R_TOC_DATE));
                result.Add(new Resource(Decl.R_MSG, Decl.R_MSG_TYPE, Decl.R_MSG_DATE));
                result.Add(new Resource(Decl.R_FORM, Decl.R_FORM_TYPE, Decl.R_FORM_DATE));
                result.Add(new Resource(Decl.R_SUPPLIERS, Decl.R_SUPPLIERS_TYPE, Decl.R_SUPPLIERS_DATE));
                result.Add(new Resource(Decl.R_MODELS, Decl.R_MODELS_TYPE, Decl.R_MODELS_DATE));
                result.Add(new Resource(Decl.R_RULES, Decl.R_RULES_TYPE, Decl.R_RULES_DATE));
                result.Add(new Resource(Decl.R_CONST, Decl.R_CONST_TYPE, Decl.R_CONST_DATE));
                result.Add(new Resource(Decl.R_TSMATCH_EXE, Decl.R_TSMATCH_EXE_TYPE, Decl.R_TSMATCH_EXE_DATE));
                result.Add(new Resource(Decl.R_BUTTON_CS, Decl.R_BUTTON_CS_TYPE, Decl.R_BUTTON_CS_DATE));
                result.Add(new Resource(Decl.R_BUTTON_BMP, Decl.R_BUTTON_BMP_TYPE, Decl.R_BUTTON_BMP_DATE));
                result.Add(new Resource(Decl.R_IFC2X3, Decl.R_IFC2X3_TYPE, Decl.R_IFC2X3_DATE));
                Log.exit();
                return result;
            }
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
                throw new NotImplementedException();
            }
        } // end class Resource

        /// <summary>
        /// class Section initialization - fill SectionTab Dictionary
        /// </summary>
        /// 2017.03.17 as advised Oleg Turetsky
        /// 2017.03.25 add Material synonym
        public class initSection
        {
            public readonly Dictionary<string, List<string>> SectionTab = new Dictionary<string, List<string>>();

            public initSection()
            {
                sub(SType.Material, "MAT", "m", "м");
                sub(SType.Profile, "PRF", "pro", "пр");
                sub(SType.Price, "CST", "cost", "pric", "цен", "сто");
                sub(SType.Description, "DES", "оп", "знач");
                sub(SType.LengthPerUnit, "LNG", "leng", "длин");
                sub(SType.VolPerUnit, "VOL", "об", "v");
                sub(SType.WeightPerUnit, "WGT", "вес", "w");
                // применение SType.Unit: заголовок для распознавания
                //.. "составных" секций, например, "ед: руб/т" 
                sub(SType.Unit, "UNT", "ед", "un");
                sub(SType.UNIT_Vol, "UNT_Vo", "руб*м3", "ст.куб");
                sub(SType.UNIT_Weight, "UNT_W", "руб*т", "ст*т");
                sub(SType.UNIT_Length, "UNT_L", "п*метр", "за*м");
                sub(SType.UNIT_Qty, "UNT_Q", "шт", "1");
            }

            void sub(SType t, params string[] str)
            {
                List<string> lst = new List<string>();
                foreach (string s in str)
                    lst.Add(Lib.ToLat(s).ToLower().Replace(" ", ""));
                SectionTab.Add(t.ToString(), lst);
            }
        } // end class initSection

        void initModJournal()
        {
            models = new List<Mod>();
            Docs doc = Docs.getDoc(Decl.MODELS);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                models.Add(new Mod(i, doInit: false));
            }
        }
    } // end class Bootsrap
} // end namespace