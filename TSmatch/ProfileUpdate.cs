/*--------------------------------------------------------------------------------------------
 * ProfileUpdate -- Update Group Profiles in accouding Russian GOST
 *  2.07.2017 Pavel Khrapkin
 *  
 *--- History ---
 *  2.07.2017 code taken from ModHandled code, separated into this class
 *--- Unit Tests --- 
 * 2017.07.2 UT_ProfileUpdate_I OK
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
            PrfTab.Add("—", "PL");      //полоса
            PrfTab.Add("L", "L");       //уголок
            PrfTab.Add("I", "I");       //балка
            PrfTab.Add("[", "U");       //швеллер
            PrfTab.Add("Гн.", "PK");    //замкнутый профиль - квадрат
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
                    string mark = Mark.Value;
                    if (gr.Prf[0] != Mark.Value[0] ||
                        Mark.Value.Length > 1 && Mark.Value[1] != gr.Prf[1]) continue;
                    Profile = gr.Prf; profile = gr.prf;
                    gr.Prf = PrfNormStr(Mark.Key);
                    gr.prf = Lib.ToLat(gr.Prf.ToLower());
                    if (gr.Prf != Profile) break;
                }
            }
        }

        string PrfNormStr(string mark)
        {
            string type = string.Empty;
            pars = Lib.GetParsStr(Profile);
            switch (mark)
            {
                case "I":
                    if (PrfSub("Б", "b%", "", out type)) goto OK_I;
                    if (PrfSub("К", "k%A", "А", out type)) goto OK_I;
                    if (PrfSub("К", "k%", "", out type)) goto OK_I;
                    if (PrfSub("Ш", "h%", "", out type)) goto OK_I;
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
                    if(pars.Count == 2) mark += pars[0] + "x" + pars[1];
                    if(pars.Count == 3) mark += pars[0] + "x" + pars[1] + "x" + pars[2];
                    if (pars.Count != 2  && pars.Count != 3) goto ERR;
                    break;
                case "—":
                    if (pars.Count == 1) mark += pars[0];
                    if (pars.Count == 2) mark += pars[0] + "x" + pars[1];
                    break;
                case "Гн.[]":
                    break;
                case "Гн.":
                    break;
            }
            return mark;
            ERR: Msg.F("ProfileUpdate Internal error", Profile, pars.Count);
            return null;
        }
            //////////switch (pars.Count)
            //////////{
            //////////    case 1:
            //////////        if (mark == "[")
            //////////        {
            //////////            mark += pars[0];
            //////////            if (str.Contains("ap")) { mark += "аП"; break; }
            //////////            if (str.Contains("p"))  { mark += "П";  break; }
            //////////            if (str.Contains("ay")) { mark += "аУ"; break; }
            //////////            if (str.Contains("y"))  { mark += "У";  break; }
            //////////            if (str.Contains("e"))  { mark += "Э";  break; }
            //////////            if (str.Contains("l"))  { mark += "Л";  break; }
            //////////            if (str.Contains("ca")) { mark += "Cа"; break; }
            //////////            if (str.Contains("cb")) { mark += "Cб"; break; }
            //////////            if (str.Contains("c"))  { mark += "C";  break; }
            //////////            break;
            //////////        }
            //////////        if (mark == "I")
            //////////        {
            //////////            mark += pars[0];
            //////////            if (str.Contains("b1")) { mark += "Б1"; break; }
            //////////            if (str.Contains("b2")) { mark += "Б2"; break; }
            //////////            if (str.Contains("b3")) { mark += "Б3"; break; }
            //////////            break;
            //////////        }
            //////////        if (mark == "—") mark += pars[0];
            //////////        break;
            //////////    case 2:
            //////////        if (mark == "I")
            //////////        {
            //////////            mark += pars[0];
            //////////            if (str.Contains("b")) { mark += "Б" + pars[1]; break; }
            //////////            if (str.Contains("k"))
            //////////            {
            //////////                mark += "К" + pars[1];
            //////////                if (str.Contains("a")) mark += "A";
            //////////                break;
            //////////            }
            //////////        }
            //////////        if (mark == "Гн.") { mark += pars[0] + "x" + pars[1]; break; }
            //////////        mark += pars.Min() + "x" + pars.Max();
            //////////        break;
            //////////    case 3:
            //////////        if (md == PrfOpMode.Mark)
            //////////        {
            //////////            mark += pars[0] + 'x' + pars[1] + 'x' + pars[2];
            //////////            break;
            //////////        }
            //////////        if (mark == "Гн.[]")
            //////////        {
            //////////            if (pars[0] == pars[1]) return "Гн." + pars.Max() + "x" + pars.Min();
            //////////            mark += pars[0] + "x" + pars[1] + "x" + pars[2];
            //////////            break;
            //////////        }
            //////////        int p1 = pars.Min();
            //////////        pars.Remove(p1);
            //////////        int p3 = pars.Max();
            //////////        pars.Remove(p3);
            //////////        mark += p1 + "x" + pars[0] + "x" + p3;
            //////////        break;
            //////////    default: Msg.F("ModHandler.grPrfPars not recognized Profile"); break;
            //////////}

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
