/*----------------------------------------------------------------
 * FingerPrin (FP) -- characteristic name fragments and parameters
 *                    of model element attribute, Component, Rule
 *
 * 28.12.2016 Pavel Khrapkin
 *
 *--- History ---
 * 28.12.2016 Previously these values were used in modules Matcher,
 *            Rule, others. Now they're collected in FP class.
 *--- <ToDo> 2016.12.5 write down:
 *  - class definition
 *  - Constructors
 *  - getFP(str) method
 *  - bool isFpMatch(otherFP) method
 * ----- Methods: -----
 * FP getFP(str,txs,pars)   - get FP detailes from str
 * bool isFpMtch(Fp other)  - return true if this match other
 *    --- miscelleneous ---
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lib = match.Lib.MatchLib;

namespace TSmatch.FingerPrint
{
    public class FingerPrint : IEquatable<FingerPrint>
    {
        public enum type { Rule, ElmAttSet, Component }

        type FPtype;
        readonly List<string> txs = new List<string>();
        readonly Dictionary<string, string> pars = new Dictionary<string, string>();
        readonly Dictionary<string, string> synonyms = new Dictionary<string, string>();
        readonly List<string> must = new List<string>();

        public FingerPrint(type _type, List<string> _txs, Dictionary<string, string> _pars,
            Dictionary<string, string> _synonyms, List<string> _must)
        {
            this.FPtype = _type;
            txs = _txs;
            pars = _pars;
            synonyms = _synonyms;
            must = _must;
        }

        public FingerPrint(type _type, string str)
        {
            Regex parametr = new Regex(@"\{.+\}", RegexOptions.IgnoreCase);
            Match m = parametr.Match(str);
            int i0 = 0, i1 = 0;
            while (m.Success)
            {
                string s = Regex.Replace(m.Groups[0].ToString(), @"\{|\}", "");
                i1 = m.Groups[0].Index;
                pars.Add(s, "");
                txs.Add(str.Substring(i0, i1 - i0).Trim());
                i0 = i1 + 2 + s.Length;
                m = m.NextMatch();
            }
            string st = str.Substring(i0, str.Length - i0).Trim();
            if (!string.IsNullOrEmpty(st)) txs.Add(st);
            switch(_type)
            {
                case type.Rule:
                    foreach (var par in pars) must.Add(par.Key);
                    // TODO 28.12.2016 тут надо написать разбор секции Правила по Синонимам
                    //                 пока оставляем список синонимов пустым. Вернемся к этому при разборе Правил по профилям.
                    break;
                case type.Component:
                    throw new NotImplementedException();
                    break;
                case type.ElmAttSet:
                    throw new NotImplementedException();
                    break;
            }
        }

        public bool Equals(FingerPrint other)
        {
            bool ok = false;
            foreach (var tx in txs)
                foreach (var otx in other.txs)
                    ok &= otx == tx;
            foreach (var par in pars)
                foreach (var opar in other.pars)
                {
                    if (FPtype == type.Rule && Lib.IContains(must, par.Key)
                        || other.FPtype == type.Rule && Lib.IContains(other.must, opar.Key))
                    {
                        ok &= opar.Value == par.Value;
                    }
                }
            return ok;
        }

        public FingerPrint setFPfrStr(string str)
        {
            throw new NotImplementedException();
        }

        public bool isFpMtch(FingerPrint other)
        {
            bool ok = false;

            throw new NotImplementedException();
            return ok;
        }

        internal string strINFO()
        {
            string result = "txs=";
            foreach (string s in txs) result += " " + s;
            result += "\tpars={";
            int ip = 0;
            foreach (var p in pars)
            {
                result += p.Key + "=" + p.Value;
                ip++;
                if(ip!=pars.Count) result += ", ";
            }
            result += "}\tmust={";
            ip = 0;
            foreach (var p in pars)
            {
                result += p.Key + "=" + p.Value;
                ip++;
                if (ip != pars.Count) result += ", ";
            }
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