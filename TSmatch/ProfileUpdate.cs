/*--------------------------------------------------------------------------------------------
 * ProfileUpdate -- Update Group Profiles in accouding Russian GOST
 *  7.07.2017 Pavel Khrapkin
 *  
 *--- History ---
 *  2.07.2017 code taken from ModHandled code, separated into this class
 *  4.07.2017 multiple PrfTab values separated with '|', f.e "PL|-"
 *--- Unit Tests --- .
 * 2017.07.15 UT_ProfileUpdate_I, UT_ProfileUpdate_U, UT ProfileUpdate_L, 
 *            UT_ProfileUpdate_PL, UT_ProfileUpdate_PK_PP                    OK
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
using ElmGr = TSmatch.ElmAttSet.Group;
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
            PrfTab.Add("—", "PL|—");      //полоса
            PrfTab.Add("L", "L|Уголок");  //уголок
            PrfTab.Add("I", "I|ДВУТАВР"); //балка двутавровая
            PrfTab.Add("[", "U|Швеллер"); //швеллер
            PrfTab.Add("Гн.", "PK|Профиль");    //замкнутый профиль - квадрат
            PrfTab.Add("Гн.[]", "PP");  //замкнутый прямоугольный профиль, труба профильная

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
                        if (mark.Length > 0 && mark[0] != gr.Prf[0]) continue; // for -, L, I, U
                        if (mark.Length > 1 && mark[1] != gr.Prf[1]) continue; // for PP and PK check
                        if (mark.Length > 2 && mark[2] != gr.Prf[2]) continue; // Уголок, Швеллер и тп

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
                        || Regex.IsMatch(type, @"\d") && pars.Count != 2) goto ERR;
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
                    if (pars.Count != 1) goto ERR;
                    break;
                case "L":
                    if (pars.Count == 2) mark += pars[0] + "x" + pars[1];
                    if (pars.Count == 3) mark += pars[0] + "x" + pars[1] + "x" + pars[2];
                    if (pars.Count != 2 && pars.Count != 3) goto ERR;
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
                    mark += pars[0] + "x" + pars[1] + "x" + pars[2];
                    if (pars.Count != 3) goto ERR;
                    break;
                case "Гн.":
                    if (Profile.Contains("Профиль(кв.)") && pars[0] == pars[1])
                    {
                        if (pars.Count != 3) goto ERR;
                        mark += pars[0] + "x" + pars[2].Replace(".0", "");
                    }
                    else
                    {
                        mark += pars[0] + "x" + pars[1];
                        if (pars.Count != 2) goto ERR;
                    }
                    break;
            }
            return mark;
            ERR: Msg.F("ProfileUpdate Internal error", Profile, pars.Count);
            return null;
        }
        //////////    default: Msg.F("ModHandler.grPrfPars not recognized Profile"); break;

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