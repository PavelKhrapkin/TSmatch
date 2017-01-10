/*----------------------------------------------------------------
 * FingerPrin (FP) -- characteristic name fragments and parameters
 *                    of model element attribute, Component, Rule
 *
 * 10.01.2017 Pavel Khrapkin
 *
 *--- History ---
 * 28.12.2016 made from other module fragments
 * 10.01.2017 class fields and identification maethod changed
 *--- <ToDo> 2017.1.10:
 *  - реализовать разбор синонимов в конструкторе 
 *  - реализовать Equals - идентификацию
 * ----- Methods: -----
 * - FP1.Equals.FP2 - FP1 and FP2 have same FP or, at least one of them (Rule) is in match with another
 *    --- miscelleneous ---
 * - RecognyzeSection(string str)           - return enum Section, which matches to str
 * - isSection(string str, string pattern)  - return TRUE, when str is in match with pattern section
 * - parParser(type _type, string par)      - return Dictionary, containes parsed parametr par
 * - digPar(string str, double minVal = -32000, double maxVal = 32000) - return parsed str between min and max
 * - strINFO()                              - return string containes FP for trace purpaces
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;

namespace TSmatch.FingerPrint
{
    public class FingerPrint : IEquatable<FingerPrint>
    {
        public enum type { Rule, CompSet, Component }

        type typeFP;
        Section section;
        readonly string tx = string.Empty;
        public readonly Dictionary<string, object> pars = new Dictionary<string, object>();
        readonly Dictionary<string, string> synonyms = new Dictionary<string, string>();

        public FingerPrint(type _type, string str)
        {

            typeFP = _type;
            section = (Section) RecognyzeSection(str);
            tx = Lib.ToLat(str.Substring(str.IndexOf(':'))).ToUpper().Trim();
            Regex parametr = new Regex(@"\{.+?\}", RegexOptions.IgnoreCase);
            Match m = parametr.Match(str);
            while (m.Success)
            {
                tx = parametr.Replace(tx, "").Trim();
                pars = parParser(_type, m.Groups[0].ToString().Trim());
                m = m.NextMatch();
            }
        }

        public enum Section
        {
            Material, Profile, Description, Price,
            Use, Unit, WeightPerUnit, LengthPerUnit, VolPerUnit
        }
        private Section? RecognyzeSection(string str)
        {
            const string rMat = "m";         //for "Profile" abbreviation must be at least "Pr", or "Пр"
            const string rPrf = "(пp|pr)";   //.. to avoid mixed parse of russian 'р' in "Материал"
            const string rCst = "(c|ц)";     //Cost = Цена - Price Section
            const string rDesc = "(d|o)";    //Description = Описание
            const string rWght = "(w|b)";    //Weight per unit = Вес
            const string rVol = "v";
            const string rLng = "l";
            const string rUnit = "(U|E)";    //Unit = Ед.
            const string rUse = "(US|и|прим)";  // Use = использование = применение, примечание

            if (isSection(str, rMat)) return Section.Material;
            if (isSection(str, rPrf)) return Section.Profile;
            if (isSection(str, rCst)) return Section.Price;
            if (isSection(str, rDesc)) return Section.Description;
            if (isSection(str, rLng)) return Section.LengthPerUnit;
            if (isSection(str, rVol)) return Section.VolPerUnit;
            if (isSection(str, rWght)) return Section.WeightPerUnit;
            if (isSection(str, rUnit)) return Section.Unit;
            if (isSection(str, rUse)) return Section.Use;

            Msg.F("Section Not Recorgized", str);
            return null;
        }
        private bool isSection(string str, string reg)
        {
            str = Lib.ToLat(str).ToLower();
            reg = Lib.ToLat(reg + ".*?:").ToLower();
            return Regex.IsMatch(str, reg, RegexOptions.IgnoreCase);
        }

        internal Dictionary<string, object> parParser(type _type, string par)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            string s = Regex.Replace(par, @"\{|\}", "");
            string[] sParts = s.Split('=');
            object val = null;
            string sval = sParts[sParts.Length - 1];
            if (_type == type.CompSet)
                val = digPar(sval, 1, 22); //для CompSet параметры - номера колонок
            else
                val = digPar(sval);        //для Rule или Component - double или string
            int parCnt = sParts.Length == 1 ? 1 : sParts.Length - 1;
            for (int i = 0; i < parCnt; i++) result.Add(sParts[i], val);
            return result;
        }

        internal static object digPar(string str, double minVal = -32000, double maxVal = 32000)
        {
            object result = str;
            int i = -1;
            double x = 0.0;
            if (Int32.TryParse(str, out i) && i >= minVal && i <= maxVal) return i;
            if (Double.TryParse(str, out x) && x >= minVal && x <= maxVal) result = x;
            return result;
        }

        //TODO 10.1.17 переписать!
        public bool Equals(FingerPrint other)
        {
            bool ok = false;
            ////foreach (var tx in txs)
            ////    foreach (var otx in other.txs)
            ////        ok &= otx == tx;
            foreach (var par in pars)
                foreach (var opar in other.pars)
                {
                    ////////////////////if (FPtype == type.Rule && Lib.IContains(must, par.Key)
                    ////////////////////    || other.FPtype == type.Rule && Lib.IContains(other.must, opar.Key))
                    //// for test //////{
                    ////////////////////    ok &= opar.Value == par.Value;
                    ////////////////////}
                }
            return ok;
        }

        internal string strINFO()
        {
            string result = "tx=" + tx;
            result += "\tpars={";
            int ip = 0;
            foreach (var p in pars)
            {
                result += p.Key + "=" + p.Value;
                ip++;
                if (ip != pars.Count) result += ", ";
            }
            //////////result += "}\tmust={";
            //////////ip = 0;
            //////////foreach (var p in pars)
            //////////{
            //////////    result += p.Key + "=" + p.Value;
            //////////    ip++;
            //////////    if (ip != pars.Count) result += ", ";
            //////////}
            result += "}\tsyn={";
            ip = 0;
            foreach (var p in pars)
            {
                result += p.Key + "=" + p.Value;
                ip++;
                if (ip != pars.Count) result += ", ";
            }
            result += "}";
            return result;
        }
    } //end class Fingerprint
} // end namespace