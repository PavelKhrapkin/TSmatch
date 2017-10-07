/*-----------------------------------------------------------------------------------
 * Bootstrap - provide initial start of TSmatch, when necessary - startup procedure
 * 
 *  6.10.2017  Pavel Khrapkin
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
 * 12.09.2017 - remove Message.Init and all message related resources handling
 *  6.10.2017 - UT_ for Resource check, cleanup
 *  * --- Unit Tests ---
 * 2017.10.6  - UT_Boot_ResxError, UT_Bootstrap   OK
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
//8/10 using Msg = TSmatch.Message.Message;
using Docs = TSmatch.Document.Document;
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
        public Message.Message Msg = new Message.Message();

        public Bootstrap() { }
        public Bootstrap(bool init = true)
        {
            if (!init) return;
            Log.set("Bootstrap");
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
            //--- iniciate Ifc
            IFCschema = Path.Combine(_TOCdir, @"..\inp\IFC2X3.exp");
            ifc.init(IFCschema);        // инициируем IFC, используя файл схемы IFC - обычно из Tekla
            //--check other Resources and we're in air
            CheckResx("Forms", Resx.Forms);
            Log.exit();
        }

        Dictionary<ResType, string> ResTab = new Dictionary<ResType, string>()
        {
            {ResType.Date, "Dat" },
            {ResType.File, "Fil" },
            {ResType.Resx, "Res" },
            {ResType.Doc , "Doc" }
        };
        enum ResType { Date, File, Doc, Resx, Err }
        protected void CheckResx(string rName, string rValue)
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
                    if (!Docs.IsDocExists(rName)) resxError(ResErr.NoDoc, rName);
                    break;
                case ResType.File:
                    if (!FileOp.isFileExist(rName)) resxError(ResErr.NoFile, rName);
                    break;
                case ResType.Date:
                    DateTime d = Lib.getDateTime(v);
                    Docs doc = Docs.getDoc(rName, fatal: false);
                    if (doc == null) resxError(ResErr.NoDoc, rName);
                    string sdd = doc.Body.Strng(1, 1);
                    DateTime dd = Lib.getDateTime(sdd);
                    if (dd < d) resxError(ResErr.Obsolete, rName);
                    break;
                case ResType.Resx:
                    break;
                default: resxError(ResErr.ErrResource, rName); break;
            }
        }
        protected enum ResErr { ErrResource, NoFile, NoDoc, Obsolete }
        protected void resxError(ResErr errType, string resName)
        {
            string myName = "Bootstrap__resError_";
            var v = errType.ToString();
            Msg.FF(myName + v, resName);
        }
    } // end class Bootsrap
} // end namespace