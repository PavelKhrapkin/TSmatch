/*-----------------------------------------------------------------------
 * TS_OpenAPI -- Interaction with Tekla Structure over Open API
 * 
 * 29.05.2017  Pavel Khrapkin, Alex Bobtsov
 *
 *----- ToDo ---------------------------------------------
 * - реализовать интерфейс IAdapterCAD, при этом избавится от static
 *----- History ------------------------------------------
 *  3.1.2016 АБ получаем длину элемента
 * 12.1.2016 PKh - добавлено вычисление MD5 по списку атрибутов модели, теперь это public string.
 *               - из имени модели удалено ".db1"
 * 14.1.2016 PKh - возвращаем в pulic string MyName версию этого метода
 * 21.1.2016 PKh - сортировка AttSet 
 * 25.1.2016 PKh - подсчет MD5 и проверку перенес в ModAtrMD5()
 *  5.2.2016 PKh - определяем путь к каталогу exceldesign в среде Tekla
 * 11.2.2016 PKh - Weight и volume атрибуты добавлены в AttSet
 * 19.2.2016 PKh - GetTeklaDir(ModelDir)
 *  4.3.2016 PKh - Add GUID in AttSet; "Fixed" profile is used
 *  6.3.2016 PKh - isTeklaActive() metod included
 * 10.3.2016 PKh - AttSet Compararer implemented
 * 20.4.2016 PKh - GetTeklaDir() rewritten
 *  4.6.2016 PKh - GetTeklaDir(Environment) add
 *  5.6.2016 PKh - isTeklaModel(name) add
 * 21.6.2016 PKh - ElmAttSet module keep all Elements instead of AttSet
 * 30.6.2016 PKh - IsTeklaActive() modified
 * 22.8.2016 PKh - Scale method to account unit in Model
 * 29.5.2017 Pkh - Get Russian GOST profile from UDA
 * -------------------------------------------
 * public Structure AttSet - set of model component attribuyes, extracted from Tekla by method Read
 *                           AttSet is Comparable, means Sort is applicable, and 
 *
 *      METHPDS:
 * Read()           - read current model from Tekla, return List<ElmAttSet> - list of this model attributes.
 *                    Model element atrtibutes conaines in class ElmAttSet:
 *                    * all Element Properties - in the class fields: Material, Profile, Guid, Volume, Weight etc
 *                    * TAG - propertiy names for getting them from Tekla Structures
 *                    * List<ElmAttSet>Elements - static list of properties all Elements
 * ModAtrMD5()      - calculate MD5 - contol sum of the current model
 * GetTeklaDir(mode) - return Path to the model directory, or Path to exceldesign in Tekla environmen
 * isTeklaActive()  - return true, when Tekla up and runing, else false
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using log4net.Config;

using Tekla.Structures;
using TSD = Tekla.Structures.Dialog.ErrorDialog;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Model.Operations;

using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Lib = match.Lib.MatchLib;
using TSM = Tekla.Structures.Model;
using Elm = TSmatch.ElmAttSet.ElmAttSet;


namespace TSmatch.Tekla
{
    public class Tekla //: IAdapterCAD
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Tekla:TS_OpenAPI");

        const string MYNAME = "Tekla.Read v2.1";

        public enum ModelDir { exceldesign, model, macro, environment };
        /* 21.6.2016 -- заменяем на ElmAttSet
                public struct AttSet : IComparable<AttSet>
                {
                    public string guid, mat, mat_type, prf;
                    public double lng, weight, volume;
                    public AttSet(string g, string m, string mt, string p, double l, double w, double v)
                    { guid = g; mat = m; mat_type = mt;  prf = p; lng = l; weight = w; volume = v; }

                    public int CompareTo(AttSet att)
                    {
                        int result = mat.CompareTo(att.mat);
                        if (result == 0) result = prf.CompareTo(att.prf);
                        if (result == 0) return -lng.CompareTo(att.lng);
                        return result;
                    }
                }
                public class AttSetCompararer : IEqualityComparer<AttSet>
                {
                    public bool Equals(AttSet p1, AttSet p2)
                    {
                        if (p1.guid == p2.guid & p1.mat == p2.mat & p1.prf == p2.prf & p1.lng == p2.lng
                            & p1.volume == p2.volume & p1.weight == p2.weight) return true;
                        else return false;
                    }
                    public int GetHashCode(AttSet p)
                    {
                        int hCode = (p.guid + p.mat + p.prf + p.lng.ToString()
                            + p.volume.ToString() + p.weight.ToString()).GetHashCode();
                        return hCode.GetHashCode();
                    }
                } // class AttSetCompararer
                private static List<AttSet> ModAtr = new List<AttSet>();
        21/6/2016 заменяем на ElmAttSet */
        public static TSM.ModelInfo ModInfo;

        public static string MyName = MYNAME;
        ////        public static string ModelMD5;

        ////public List<Elm> Read(string modName) { return Read(); }
        ////public List<Elm> Read(this TSmatch.Model.Model _mod ) { return Read(_mod.name); }
        TSM.Model model = new TSM.Model();
        List<Part> parts = new List<Part>();

        public Tekla() { } // конструктор класса Tekla - пока пустой 6.4.17

        public List<Elm> Read(string dir = "", string name = "")
        {
            Log.set("TS_OpenAPI.Read");
            List<Elm> elements = new List<Elm>();
            // 6.4.17 //TSM.Model model = new TSM.Model();
            ////////////List<Part> parts = new List<Part>();
            ModInfo = model.GetInfo();
            if (dir != "" && ModInfo.ModelPath != dir
                || name != "" && ModInfo.ModelName != String.Concat(name, ".db1")) Msg.F("Tekla.Read: Another model loaded, not", name);
            ModInfo.ModelName = ModInfo.ModelName.Replace(".db1", "");
            TSM.ModelObjectSelector selector = model.GetModelObjectSelector();
            System.Type[] Types = new System.Type[1];
            Types.SetValue(typeof(Part), 0);

            TSM.ModelObjectEnumerator objectList = selector.GetAllObjectsWithType(Types);
            int totalCnt = objectList.GetSize();
            var progress = new TSM.Operations.Operation.ProgressBar();
            bool displayResult = progress.Display(100, "TSmatch", "Reading model. Pass component records:", "Cancel", " ");
            int ii = 0;
            while (objectList.MoveNext())
            {
                TSM.Part myPart = objectList.Current as TSM.Part;
                if (myPart != null)
                {
                    ii++;
                    double lng = 0.0;
                    double weight = 0.0;
                    double vol = 0.0;
                    string guid = "";
                    string mat_type = "";
                    double price = 0.0;
//31/5/17                    string prf = string.Empty;

                    myPart.GetReportProperty("GUID", ref guid);
                    myPart.GetReportProperty("LENGTH", ref lng);
                    myPart.GetReportProperty("WEIGHT", ref weight);
                    myPart.GetReportProperty("VOLUME", ref vol);
                    myPart.GetReportProperty("MATERIAL_TYPE", ref mat_type);

                    string ru_prf = "";
                    myPart.GetReportProperty("PROFILE.TPL_NAME_FULL", ref ru_prf);
                    //31/5/17                    prf = ru_prf == string.Empty ? myPart.Profile.ProfileString : ru_prf;

                    string pp = "";
                    myPart.GetReportProperty("PROFILE", ref pp);

                    lng = Math.Round(lng, 0);
                    elements.Add(new Elm(guid, myPart.Material.MaterialString,
                        mat_type, myPart.Profile.ProfileString, lng, weight, vol, _ru_prf: ru_prf));
 // !!                  if (ii % 500 == 0) // progress update every 500th items
                    {
                        if (progress.Canceled())
                        {
//                            new Log("\n\n======= TSmatch pass model CANCEL!! =======  ii=" + ii);
//                            TSD.Show()
                            break;
                        }
                        progress.SetProgress(ii.ToString(), 100 * ii / totalCnt);
                    }
                }
            } //while
            progress.Close();
            Scale(elements);
            elements.Sort();
            Log.exit();
            return elements;
        } // Read

        static void Scale(List<Elm> elements)
        {
            foreach (var elm in elements)
            {
                // weight [kg]; volume [mm3]->[m3]
                elm.volume = elm.volume / 1000 / 1000 / 1000;     // elm.volume [mm3] -> [m3] 
            }
        }

        public List<Elm> Read(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);
            return Read(dir, name);
        }

        public bool IfLockedWait(string FileName)
        {
            // try 10 times
            int RetryNumber = 20;
            while (true)
            {
                try
                {
                    using (FileStream FileStream = new FileStream(
                    FileName, FileMode.Open,
                    FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        byte[] ReadText = new byte[FileStream.Length];
                        FileStream.Seek(0, SeekOrigin.Begin);
                        FileStream.Read(ReadText, 0, (int)FileStream.Length);
                    }
                    return true;
                }
                catch (IOException)
                {
                    // wait one second
                    Thread.Sleep(500);
                    RetryNumber--;
                    if (RetryNumber == 0)
                        return false;
                }
            }
        }

        public void WriteToReport(string path)
        {
            Operation.CreateReportFromAll("ОМ_Список", path, "MyTitle", "", "");
            while (!File.Exists(path)) Thread.Sleep(500);
            if (!IfLockedWait(path)) Msg.F("No Tekla Report created");
        }

        public string ReadReport(string path)
        {
            String rep = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(path))
                rep = sr.ReadToEnd();
            }
            catch (Exception e) { Msg.F("Tekla ReadReport: can't read file: " + e.Message); }
            return rep;
        }

        public int elementsCount()
        {
            Log.set("TS_OpenAPI.elementsCount()");
            //10/4/17            var i = model.GetPhases();
            //10/4/17            int ii = TSM.Phase .PhaseNumber(); 
            TSM.ModelObjectSelector selector = model.GetModelObjectSelector();
            System.Type[] Types = new System.Type[1];
            Types.SetValue(typeof(Part), 0);
            TSM.ModelObjectEnumerator objectList = selector.GetAllObjectsWithType(Types);
            Log.exit();
            int totalCnt = objectList.GetSize();
            return totalCnt;
        }
        /// <summary>
        /// HeghlightElements(List<Elm>elements, color) - change color of elements in list 
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="color"></param>
        public void HighlightElements(Dictionary<string, Elm> els, int color = 1)
        {
            Log.set("TS_OpenAPI.HighLightElements");
            var colorObjects = new List<ModelObject>();
            foreach (var elm in els)
            {
                Identifier id = new Identifier(elm.Key);
                var obj = model.SelectModelObject(id);
                colorObjects.Add(obj);
            }
            var _color = new Color(0.0, 0.0, 1.0);
            ModelObjectVisualization.SetTransparencyForAll(TemporaryTransparency.SEMITRANSPARENT);
            ModelObjectVisualization.SetTemporaryState(colorObjects, _color);
            Log.exit();
        }

        public void HighlightClear()
        {
            ModelObjectVisualization.SetTransparencyForAll(TemporaryTransparency.VISIBLE);
        }
        /*2016.6.21        /// <summary>
                /// ModAtrMD5() - calculate MD5 of the model read from Tekla in ModAtr
                /// </summary>
                /// <remarks>It could take few minutes for the large model</remarks>
                public static string ModAtrMD5()
                {
                    //            DateTime t0 = DateTime.Now;  
                    string str = "";
                    foreach (var att in ModAtr) str += att.mat + att.prf + att.lng.ToString();
                    ModelMD5 = Lib.ComputeMD5(str);
                    return ModelMD5;
                    //            new Log("MD5 time = " + (DateTime.Now - t0).ToString());
                } // ModAtrMD5
        2016.6.21 */
        public static string GetTeklaDir(ModelDir mode)
        {
            string TSdir = "";
            switch (mode)
            {
                case ModelDir.exceldesign:
                    TeklaStructuresSettings.GetAdvancedOption("XS_EXTERNAL_EXCEL_DESIGN_PATH", ref TSdir);
                    break;
                case ModelDir.model:
                    TSM.Model model = new TSM.Model();
                    ModInfo = model.GetInfo();
                    TSdir = ModInfo.ModelPath;
                    break;
                case ModelDir.macro:
                    TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref TSdir);
                    string[] str = TSdir.Split(';');
                    TSdir = str[0] + @"\modeling";     // this Split is to ignore other, than common TS Enviroments
                    break;
                case ModelDir.environment:
                    TSdir = GetTeklaDir(ModelDir.exceldesign);
                    TSdir = Path.Combine(TSdir, @"..\..");
                    TSdir = Path.GetFullPath(TSdir);
                    break;
            }
            //////////            var ff = TeklaStructuresInfo.GetCurrentProgramVersion();
            //////////            var dd = TeklaStructuresFiles.GetAttributeFile(TSdir);
            //////////            TSdir = TS.TeklaStructuresFiles();
            return TSdir;
        }
        /// <summary>
        /// IsTeklaActice() - return true if TeklaStructures Process exists in Windows, and model is available 
        /// </summary>
        /// <returns>true if Tekla is up and running</returns>
        public static bool isTeklaActive()
        {
            Log.set("isTeklaActive()");
            bool ok = false;
            const string Tekla = "TeklaStructures";
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.ToLower().Contains(Tekla.ToLower()))
                {
                    TSM.Model model = new TSM.Model();
                    if (!model.GetConnectionStatus()) Msg.W("===== No Tekla active =========");
                    try
                    {
                        ModInfo = model.GetInfo();
                        ok = model.GetConnectionStatus() && ModInfo.ModelName != "";
                    }
                    catch { Msg.W("isTeklaActive no model Connection"); }
                    break;
                }
            }
            Log.exit();
            return ok;
        }
        /// <summary>
        /// isTeklaModel(name) -- check if Tekla open with the model name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>true if in Tekla active and open model == name</returns>
        /// <history>5.6.2016</history>
        public static bool isTeklaModel(string name)
        {
            Log.set("TS_OpenAPI.isTeklaModel(\"" + name + "\")");
            bool ok = false;
            if (isTeklaActive())
            {
                TSM.Model model = new TSM.Model();
                ModInfo = model.GetInfo();
                name = Path.GetFileNameWithoutExtension(name);
                //                ModInfo.ModelName = ModInfo.ModelName.Replace(".db1", "");
                string inTS = Path.GetFileNameWithoutExtension(ModInfo.ModelName);
                ok = Path.GetFileNameWithoutExtension(ModInfo.ModelName) == name;
            }
            Log.exit();
            return ok;
        }
        public static string getModInfo()
        {
            string result = String.Empty;
            TSM.Model model = new TSM.Model();
            ModInfo = model.GetInfo();
            result = Path.GetFileNameWithoutExtension(ModInfo.ModelName);
//            if (!isTeklaModel(result)) Msg.F("TS_Open API getModInfo error");
            return result;
        }
    } //class Tekla
} //namespace