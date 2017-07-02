/*--------------------------------------------------------------------------------------------
 * ProfileUpdate -- Update Group Profiles in accouding Russian GOST
 *  1.07.2017 Pavel Khrapkin
 *  
 *--- History ---
 *  1.07.2017 taken from ModHandled code, separated into this class
 *--- Unit Tests --- 
 * 2017.07.1 UT_ModHandler.UT_Hndl, UT_Pricing OK
 * -------------------------------------------------------------------------------------------
 *      Methods:
 * getGroups()      - groupping of elements of Model by Material and Profile
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
        /// Здесь текст строки, получаемой из Tekla API заменяется, на первое значение
        /// аргумента в перечне PrfNormalyze.
        /// Полнота преобразования кодов проверялась по ГОСТ и среде Tekla Russia.
        /// </Description>

        private string Profile;
        private string profile;
        
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

                    if (!gr.Prf.Contains(mark) && !gr.prf.Contains(mark)) continue;
                    Profile = gr.Prf; profile = gr.prf;
                    gr.Prf = PrfNormStr(Mark.Key);
                    gr.prf = Lib.ToLat(gr.Prf.ToLower());
                    if (gr.Prf != Profile) break;
                }
            }
        }

        ////////////public List<ElmGr> PrfUpdate(List<ElmGr> grp)
        ////////////{
        ////////////    PrfNormalize(ref grp, PrfOpMode.Full, "—", "PL", "Полоса");
        ////////////    PrfNormalize(ref grp, PrfOpMode.Mark, "L", "Уголок");
        ////////////    PrfNormalize(ref grp, PrfOpMode.Mark, "I", "Балка");
        ////////////    PrfNormalize(ref grp, PrfOpMode.Full, "[", "U", "Швеллер");
        ////////////    PrfNormalize(ref grp, PrfOpMode.Full, "Гн.[]", "PP", "Тр.", "Труба пр");
        ////////////    PrfNormalize(ref grp, PrfOpMode.Full, "Гн.", "PK", "Тр.");
        ////////////    return grp;
        ////////////}
        /////////////// <summary>
        /////////////// PrfNormalize operate in <Full>, or in <Mark> mode:
        /////////////// <para>  - Mark: only setup Mark (i.e. Profile type) as pointed in first argument, or</para>
        /////////////// <para>  - Full: setup Mark, and sort digital parameter values the profile template list;</para> 
        /////////////// </summary>
        ////////////enum PrfOpMode { Full, Mark }
        ////////////private void PrfNormalize(ref List<ElmGr> grp, PrfOpMode md, params string[] prfMark)
        ////////////{
        ////////////    foreach (var gr in grp)
        ////////////    {
        ////////////        foreach (string s in prfMark)
        ////////////        {
        ////////////            if (!gr.Prf.Contains(s) && !gr.prf.Contains(s)) continue;
        ////////////            string initialPrf = gr.Prf;
        ////////////            gr.Prf = PrfNormStr(gr.prf, prfMark[0], Lib.GetPars(gr.Prf));
        ////////////            gr.prf = Lib.ToLat(gr.Prf.ToLower());
        ////////////            log.Info("--- " + initialPrf + " -> " + "Prf=" + gr.Prf + "gr.prf=" + gr.prf);
        ////////////            break;
        ////////////        }
        ////////////    }
        ////////////}

        string PrfNormStr(string mark)
        {
            string type = string.Empty;
            List<int> pars = Lib.GetPars(Profile);
            switch (mark)
            {
                case "I":
                    if (PrfSub("Б", "b%", out type)) goto OK;
                    if (PrfSub("К", "(k%A)|(k%)", out type)) goto OK;
                    if (PrfSub("Ш", "h%", out type)) goto OK;
                    OK: mark += pars[0] + type;
                    break;
#if Not_ready
                    mark += pars[0];
                    if (str.Contains("b1")) { mark += "Б1"; break; }
                    if (str.Contains("b2")) { mark += "Б2"; break; }
                    if (str.Contains("b3")) { mark += "Б3"; break; }
                    if (pars.Count != 1) Msg.F("Internal error");
                    break;
                case "[":
                    mark += pars[0];
                    if (str.Contains("ap")) { mark += "аП"; break; }
                    if (str.Contains("p")) { mark += "П"; break; }
                    if (str.Contains("ay")) { mark += "аУ"; break; }
                    if (str.Contains("y")) { mark += "У"; break; }
                    if (str.Contains("e")) { mark += "Э"; break; }
                    if (str.Contains("l")) { mark += "Л"; break; }
                    if (str.Contains("ca")) { mark += "Cа"; break; }
                    if (str.Contains("cb")) { mark += "Cб"; break; }
                    if (str.Contains("c")) { mark += "C"; break; }
                    if (pars.Count != 1) Msg.F("Internal error");
                    break;
#endif //Not_ready
                case "Гн.[]":
                    break;
                case "Гн.":
                    break;
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
            return mark;
        }

        // обнаруживает подстроки вида "Б3" или "K3A" и возвращает, если найдено, в type
        private bool PrfSub(string Type, string v, out string type)
        {
            type = string.Empty;
            bool ok = Regex.IsMatch(profile, v.Replace('%', '.'), RegexOptions.IgnoreCase);
            if (!ok) return ok;

            string typeDig = string.Empty, typeLtr = string.Empty;
            string[] v_parts = v.Split('|');
            foreach (string vp in v_parts)
            {
                if (!Regex.IsMatch(profile, vp.Replace('%', '.'), RegexOptions.IgnoreCase)) continue;
                v = vp.Replace("(", "").Replace(")", "");
                int iV = v.IndexOf("%");
                int iPrf = profile.IndexOf(v[iV - 1]) + 1;
                if (iV > 0 && iV < v.Length && iPrf > 0 && iPrf < profile.Length)
                    typeDig = profile.Substring(iPrf, 1);
                if (++iV < v.Length && ++iPrf < profile.Length) typeLtr = v.Substring(iV);
                int iTypeDig = Convert.ToInt32(typeDig);
                if (iTypeDig < 0 || iTypeDig > 5) Msg.F("Wrong Gr.Prg", Profile);
                break;
            }
            type = Type + typeDig + typeLtr;
            return ok;
        }
    } // end class ModHandler : Model
} // end namespace Model.Handler
