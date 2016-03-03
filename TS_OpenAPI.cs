/*-----------------------------------------------------------------------
 * TS_OpenAPI -- класс предназначенный для взаимодействия с Tekla Structure Open API
 * 
 *  19.2.2016  П.Храпкин, А.Бобцов
 *  
 * 3.1.2016 АБ получаем длину элемента
 * !6.1.2016 PKh try-catch для корректной диагностики, если Tekla не загружена -- НЕ РАБОТАЕТ try-catch
 * 12.1.2016 PKh - добавлено вычисление MD5 по списку атрибутов модели, теперь это public string.
 *               - из имени модели удалено ".db1"
 * 14.1.2016 PKh - возвращаем в pulic string MyName версию этого метода
 * 21.1.2016 PKh - сортировка AttSet 
 * 25.1.2016 PKh - подсчет MD5 и проверку перенес в ModAtrMD5()
 * 5.2.2016 PKh - определяем путь к каталогу exceldesign в среде Tekla
 * 11.2.2016 PKh - Weight и volume атрибуты добавлены в AttSet
 * 19.2.2016 PKh - GetTeklaDir(ModelDir)
 * -------------------------------------------
 * TSmodelRead(name)    - читает из Tekla текущую модель, возвращает список из наборов атрибутов AttSet,
 *                        относящихсяк каждому отдельному компоненту.
 *                        Сейчас AttSet содержит только пары <материал> и <профиль>
 * GetTeklaDir()        - возвращает путь к каталогу exceldesign среды Tekla
 * ModAtrMD5()          - подсчет MD5 - строки контрольной суммы модели
 */

using System;
using System.Collections.Generic;

using Tekla.Structures;
using Tekla.Structures.Model;

using Log = match.Lib.Log;
using Lib = match.Lib.MatchLib;
using TS = Tekla.Structures;
using TSM = Tekla.Structures.Model;

namespace TSmatch.Tekla
{
    class Tekla
    {
        const string MYNAME = "Tekla.Read v1.2";
        public enum ModelDir : int {exceldesign = 0, model = 1};

        public struct AttSet : IComparable<AttSet>
        {
            public string mat, prf;
            public double lng, weight, volume;
            public AttSet(string m, string p, double l, double w, double v)
            { mat = m; prf = p; lng = l; weight = w; volume = v; }

            public int CompareTo(AttSet att)
            {
                int result = mat.CompareTo(att.mat);
                if (result == 0) result = prf.CompareTo(att.prf);
                if (result == 0) return -lng.CompareTo(att.lng);
                return result;
            }
        }
        private static List<AttSet> ModAtr = new List<AttSet>();
        public static TSM.ModelInfo ModInfo;

        public static string MyName = MYNAME;
        public static string ModelMD5;

        public static List<AttSet> Read()
        {
            Log.set("TS_OpenAPI.Read");
            TSM.Model model = new TSM.Model();
            List<Part> parts = new List<Part>();
            try { if (!model.GetConnectionStatus()) Log.FATAL("Tekla Model does not connected to C#"); }
            catch (Exception e)
            {
                Log.FATAL("Tekla is not activated. Try to Run Tekla Structures."
                    + "\n    Error mesage: \"" + e.Message + "\"");
            }
            ModInfo = model.GetInfo();
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
                    myPart.GetReportProperty("WEIGHT_NET", ref weight);
                    double vol = 0.0;

                    string profile = "";
                    double width = 0.0, height = 0.0;
                    myPart.GetReportProperty("PROFILE", ref profile);
                    myPart.GetReportProperty("WIDTH", ref width);
                    myPart.GetReportProperty("HEIGHT", ref height);
                    myPart.GetReportProperty("WEIGHT", ref weight);

                    myPart.GetReportProperty("VOLUME", ref vol);
                    myPart.GetReportProperty("LENGTH", ref lng);
                    lng = Math.Round(lng, 0);
                    //string cut = "";
                    //myPart.GetReportProperty("CAST_UNIT_TYPE", ref cut);
                    ModAtr.Add(new AttSet(myPart.Material.MaterialString,
                                          profile, lng, weight, vol));
                    //ModAtr.Add(new AttSet(myPart.Material.MaterialString,
                    //                      myPart.Profile.ProfileString, 
                    //                      lng, weight_m, vol));
                    if (ii % 1000 == 0) // progress update every 1000th item
                    {
                        if (progress.Canceled())
                        {
                            new Log("\n\n======= TSmatch pass model CANCEL!! =======  ii=" + ii);
                            break;
                        }
                        progress.SetProgress(ii.ToString(), 100 * ii / totalCnt);
                    }
                }
            } //while
            progress.Close();
            ModAtr.Sort();
            Log.exit();
            return ModAtr;
        } // Read
        /// <summary>
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
        public static string GetTeklaDir(int mode = -1)
        {
            string TSdir = "";
            try
            {
                if (mode == -1 || ModelDir.exceldesign.Equals(mode))
                    TeklaStructuresSettings.GetAdvancedOption("XS_EXTERNAL_EXCEL_DESIGN_PATH", ref TSdir);
                else
                {
                    TSM.Model model = new TSM.Model();
                    ModInfo = model.GetInfo();
                    TSdir = ModInfo.ModelPath;
                }
            }
            catch { Log.FATAL("You address Tekla Strucures, when it is not active. Please, try to run Tekla!"); }
            //            var ff = TeklaStructuresInfo.GetCurrentProgramVersion();
            //            var dd = TeklaStructuresFiles.GetAttributeFile(TSdir);
            //            TSdir = TS.TeklaStructuresFiles();
            return TSdir;
        }
    } //class Tekla
} //namespace