/* ---------------------------------------------
 * SLts - List<string> with first memter - SType
 * 
 * 10.1.2018
 * 
 * 1.1.2018 UT_SLst: UT_Init, UT_Is, UT_Type OK
 * ------- Methods -----------------
 * Init(params string[] str) - set SLST from srtings
 * Is(str)   - return true, when str contains one of SLts element
 * Type(str) - return First element of STlat if(Is(str)), else - null
 * NormStr(str) - return normalyzed string, i.e. without spaces, '/', '.' 
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace PriceMatch
{
    public class SLst
    {
        public List<string> sLst = new List<string>();

        public SLst() { }

        public SLst(params object[] str)
        {
            foreach (string s in str)
            {
                string v = NormStr(s);
                if (Is(v)) continue;
                if (s == (string)str[0])
                {
                    sLst.Add(Lib.ToLat(v.ToUpper()));
                    continue;
                }
                sLst.Add(Lib.ToLat(v.ToLower()));
            }
        }

        public static string NormStr(string s) =>
            string.IsNullOrEmpty(s) ? ""
            : s.Replace(" ", "").Replace("/", "").Replace(".", "");

        public bool Is(string str)
        {
            string x = Lib.ToLat(NormStr(str.ToLower()));
            foreach (string s in sLst)
            {
                string v = s.ToLower();
                if (x.Length < v.Length) continue;
                int minLng = Math.Min(x.Length, v.Length);
                if (x.Substring(0, minLng) == v) return true;
            }
            return false;
        }

        public string Type() => sLst.First();
        public string Type(string str)
        {
            if (Is(str)) return sLst.First();
            return null;
        }

        public static implicit operator List<string>(SLst v) => v.sLst;
    }
}