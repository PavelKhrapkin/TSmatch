/*--------------------------------------------------------------------------------------------
 * ProfileUpdate -- Update Group Profiles in accouding Russian GOST
 *  15.08.2017 Pavel Khrapkin
 *  
 *--- History ---
 *  2.07.2017 code taken from ModHandled code, separated into this class
 *  4.07.2017 multiple PrfTab values separated with '|', f.e "PL|-"
 * 16.07.2017 changed logic for profile "Гн." recognition
 * 10.08.2017 TP and TK recognition with new PrfTab filling and logic
 * 15.08.2017 Shape profile IFC_BREP parse written
 *--- Unit Tests --- .
 * 2017.08.10 UT_ProfileUpdate_I, UT_ProfileUpdate_U, UT ProfileUpdate_L, 
 *            UT_ProfileUpdate_PL, UT_ProfileUpdate_PK_PP, UT_ProfileUpdate_TP_TK   OK
 * -------------------------------------------------------------------------------------------
 *      Methods:
 * ProfileUpdate()      - Modify group profiles in according with Russian Gost
 * Handler()                 
 */
using System.Collections.Generic;
using System.Text.RegularExpressions;

using log4net;
using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using ElmGr = TSmatch.Group.Group;
using System;

namespace TSmatch.ProfileUpdate
{
    public class ProfileUpdate
    {
        public static readonly ILog log = LogManager.GetLogger("ProfileUpdate");
        /// <Description>
        /// Этот модуль преобразует строку - профиль группы в соответствие российским ГОСТ,
        /// так, как это делается в среде Russia для Tekla. По сути, это hardcode, он не 
        /// должен работать вне России.
        /// Здесь текст строки, получаемой из Tekla API заменяется, на значение марки - первого 
        /// аргумента в перечне PrfSub, а остаток строки разбирается в type по шаблону в аргументе 2.
        /// Полнота преобразования кодов проверялась по ГОСТ и среде Tekla Russia.
        /// </Description>

        private string Profile;
        private string profile;
        List<string> pars = new List<string>();

        static readonly Dictionary<string, string> PrfTab = new Dictionary<string, string>();

        static ProfileUpdate()
        {
            PrfTabAd("IFC_BREP", "IFC BREP");   //Shape elements
            PrfTabAd("—", "PL|—");      //полоса
            PrfTabAd("L", "L|Уголок");  //уголок
            PrfTabAd("I", "I|ДВУТАВР"); //балка двутавровая
            PrfTabAd("[", "U|Швеллер"); //швеллер
            PrfTabAd("Гн.", "PK|Профиль|Гн.");  //замкнутый профиль - квадрат
            PrfTabAd("Гн.[]", "PP");    //замкнутый прямоугольный профиль, труба профильная
            PrfTabAd("TP", "TP|Тр.");   //труба бесшовная ГОСТ 8732-78
            PrfTabAd("TK", "TK");       //труба бесшовная ГОСТ 8732-78
        }

        private static void PrfTabAd(string key, string templs)
        {
            PrfTab.Add(key, Lib.ToLat(templs.ToLower()));
        }

        public ProfileUpdate(ref List<ElmGr> elmGroups)
        {
            if (PrfTab == null || PrfTab.Count == 0 || elmGroups == null || elmGroups.Count == 0)
                Msg.F("internal errer");
            foreach (var gr in elmGroups)
            {
                foreach (var Mark in PrfTab)
                {
                    string[] marks = Mark.Value.Split('|');
                    foreach (string mark in marks)
                    {
                        if (gr.prf.Length < mark.Length || gr.prf.Substring(0, mark.Length) != mark) continue;
                        Profile = gr.Prf; profile = gr.prf;
                        gr.Prf = PrfNormStr(Mark.Key);
                        gr.prf = Lib.ToLat(gr.Prf.ToLower().Replace(" ", ""));
                        if (gr.Prf != Profile) goto NextGr;
                    } // end foreach '|' part
                } // end PrfTab entry
                NextGr:;
            } // end elmGroup
        }

        string PrfNormStr(string mark)
        {
            string type = string.Empty;
            pars = Lib.GetParsStr(Profile);
            switch (mark)
            {
                case "I":
                    if (PrfSub("Б", "b%", "", out type)) goto OK_I;
                    if (PrfSub("Б", "б%", "", out type)) goto OK_I;
                    if (PrfSub("К", "k%A", "А", out type)) goto OK_I;
                    if (PrfSub("К", "k%", "", out type)) goto OK_I;
                    if (PrfSub("Ш", "h%", "", out type)) goto OK_I;
                    if (PrfSub("Ш", "ш%", "", out type)) goto OK_I;
                    if (PrfSub("Д", "d%A", "А", out type)) goto OK_I;
                    if (PrfSub("У", "y%A", "А", out type)) goto OK_I;
                    if (PrfSub("М", "м%", "", out type)) goto OK_I;
                    if (PrfSub("С", "с%", "", out type)) goto OK_I;
                    OK_I: mark += pars[0] + type;
                    if (type == "" && pars.Count != 1
                        || Regex.IsMatch(type, @"\d") && pars.Count != 2) error(pars);
                    break;
                case "[":
                    if (PrfSub("[", "aY", "аУ", out type)) goto OK_U;
                    if (PrfSub("[", "y", "У", out type)) goto OK_U;
                    if (PrfSub("[", "ap", "аП", out type)) goto OK_U;
                    if (PrfSub("[", "p", "П", out type)) goto OK_U;
                    if (PrfSub("[", "e", "Э", out type)) goto OK_U;
                    if (PrfSub("[", "l", "Л", out type)) goto OK_U;
                    if (PrfSub("[", "ca", "Cа", out type)) goto OK_U;
                    if (PrfSub("[", "cb", "Cб", out type)) goto OK_U;
                    if (PrfSub("[", "c", "C", out type)) goto OK_U;
                    OK_U: mark += pars[0] + type;
                    if (pars.Count != 1) error(pars);
                    break;
                case "L":
                    if (pars.Count == 2) mark += pars[0] + "x" + pars[1];
                    if (pars.Count == 3) mark += pars[0] + "x" + pars[1] + "x" + pars[2];
                    if (pars.Count != 2 && pars.Count != 3) error(pars);
                    break;
                case "—":
                    if (pars.Count == 1) mark += pars[0];
                    if (pars.Count == 2)
                    {
                        double p0 = Lib.ToDouble(pars[0]);
                        double p1 = Lib.ToDouble(pars[1]);
                        if (p0 < p1) mark += pars[0] + "x" + pars[1];
                        else mark += pars[1] + "x" + pars[0];
                    }
                    break;
                case "Гн.[]":
                case "Гн.":
                    if (pars.Count == 2) mark = "Гн." + pars[0] + "x" + pars[1].Replace(".0", "");
                    if (pars.Count == 3)
                    {
                        if(pars[0] == pars[1]) mark = "Гн." + pars[0] + "x" + pars[2].Replace(".0", "");
                        else mark = "Гн.[]" + pars[0] + "x" + pars[1] + "x" + pars[2].Replace(".0", "");
                    }
                    if (pars.Count != 2 && pars.Count != 3) error(pars);
                    break;
                case "TK":
                case "TP":
                    if(pars.Count >= 2) mark += pars[0] + "x" + pars[1];
                    if (pars.Count == 3) mark += "x" + pars[2];
                    if (pars.Count != 2 && pars.Count != 3) error(pars);
                    break;
                case "IFC_BREP":
                    if(pars.Count == 1) mark += "-" + pars[0];
                    if (pars.Count > 1) error(pars);
                    break;
                default: error(pars); break;
            }
            return mark;
        }

        private void error(List<string> pars)
        { Msg.F("ProfileUpdate Internal error", Profile, pars.Count); }

        // обнаруживает подстроки вида "Б3" или "K3A" и возвращает, если найдено, в type
        private bool PrfSub(string Type, string v, string sufix, out string type)
        {
            type = string.Empty;
            bool ok = Regex.IsMatch(profile, v.Replace('%', '.'), RegexOptions.IgnoreCase);
            if (!ok) return ok;

            string typeDig = string.Empty, typeLtr = string.Empty;
            string[] v_parts = v.Split('|');
            foreach (string vp in v_parts)
            {
                if (!Regex.IsMatch(profile, vp.Replace('%', '.'), RegexOptions.IgnoreCase)) continue;
                if (!vp.Contains("%"))
                {
                    type = sufix;
                    return ok;
                }
                typeDig = pars[1];
                int iTypeDig = Convert.ToInt32(typeDig);
                if (iTypeDig < 0 || iTypeDig > 5) Msg.F("Wrong Group.Profile", Profile);
                break;
            }
            type = Type + typeDig + sufix;
            return ok;
        }
    } // end class ModHandler : Model
} // end namespace Model.Handler