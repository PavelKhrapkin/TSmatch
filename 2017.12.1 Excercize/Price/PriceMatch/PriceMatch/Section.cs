/*----------------------------------------------------------------
 * Section -- class dealing with the fragment of string related to
 *            some section - f.e. Material, or Price
 *
 *  9.2.2018 Pavel Khrapkin
 *
 * --- Unit Tests ---
 * 2017.1.10  - UT_Section, UT_SecType, UT_InitSectionTab, UT_SectionTab_Is OK
 *--- History ---
 *  7.03.2017 made from other module fragments
 * 19.03.2017 re-written with SectionTab as a argument of Constructor
 * 21.03.2017 call Bootstrap.initSection() for SectionTab
 * 28.03.2017 munli-header Section like "M: Def: body"
 *  8.08.2017 static constructor as a singleton initializator of SectionTab
 * 13.09.2017 cosmetic, multilanguage Msg
 * 28.11.2017 bug fix - "ед:" not recognozed SecType; UT_SecType re-made without Regex
 *  9.12.2017 re-written with string key SType instead of enom, Section Headers in Declaration
 * 15.12.2017 SectionTab separated as a field in class SectionTab
 * 28.12.2017 string partse to Dictionary of Sections
 *  2.01.2018 use SLst class for SectionTab
 * 10.01.2018 Initialyze SectionTab as static Singletone; don't use stDIC outside Section
 * 24.01.2018 GetFieldNumbers - parse text or List<text> to get List<int> - number of Fields
 * ------ Fields ------
 * string STtype - Section type, f.e. Material, Price etc = Key in SectionTab Dictionary
 * string body   - text string, contained in Section between ':' and ';' or end of string
 * ----- Methods: -----
 * Get(Text, [whichSection])   - recognized whichSection in Text, or left piece of Text
 * Get(List<Section> sections, SType) - return Section.body with type SType in the List
 * Parse(Text)  - return List of recognyzed Sections
 * GetFieldNumbers(Text)    - recognize {1} - Fields references in Text - section body
 * GetFieldNumbers(List<string>txtLst,[SType_whichSection]) - overley for List<string>
 * SecType(str) - recognyze section header, return SType key
 * ----- Assistance class SectionTab contains Dictionary of Section Headers with their synonuims
 * SectionTabAdd(key, strings) - add Dictionary entry for key with the strings - header temps
 * Merge(Dic)   - combine SectionTab Dic with the current SectionTab
 * -- when start new SectionTab Dictionary, use SectionTab.Dic.Clear;
 */
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Decl = PriceMatch.Declaration;

namespace PriceMatch
{
    public class Section
    {
        public static readonly ILog log = LogManager.GetLogger("Section");

        public string SType;    // Section header - text before ':'
        public string body;     // text after ':', but before first ';'

        protected static SectionTab st;

        public Section() { }

        static Section()
        {
            var x = new SectionTab();
            x.Init(Decl.STypeMAT, "mat", "m", "м");
            x.Init(Decl.STypePRF, "prf", "pro", "пр");
            x.Init(Decl.STypePRICE, "pric", "cost", "цен", "сто");
            x.Init(Decl.STypeDESCR, "des", "оп", "знач");
            x.Init(Decl.STypeLNG, "lng", "leng", "длин");
            x.Init(Decl.STypeVOL, "vol", "об", "v");
            x.Init(Decl.STypeWGT, "wgt", "вес", "w");
            x.Init(Decl.STypeUNIT_VOL, "руб/м3", "рублей/м3", "ст.куб");
            x.Init(Decl.STypeUNIT_WGT, "руб/т", "рублей/т", "ст/т");
            x.Init(Decl.STypeUNIT_W_KG, "руб / кг", "рублей / кг");
            x.Init(Decl.STypeUNIT_LNG, "погонный метр", "за м");
            x.Init(Decl.STypeUNIT_Q, "шт", "за шт", "1", "за 1", "ед", "за ед");
            st = x.Init();
        }

        public Section Get(string Text, string whichSection = "")
        {
            string[] sections = Lib.ToLat(SLst.NormStr(Text).ToLower()).Split(';');
            foreach (string str in sections)
            {
                SType = SecType(str);
                body = SecBody(str);
                if (string.IsNullOrEmpty(whichSection)
                    || whichSection == SType) return this;
            }
            SType = body = string.Empty;
            return this;
        }

        public string Get(List<string> txtLst, string whichSection = "")
        {
            foreach (string txt in txtLst)
            {
                if (Get(txt, whichSection).body != string.Empty) break;
            }
            return body;
        }

        public List<Section> Parse(string Text)
        {
            var result = new List<Section>();
            if (string.IsNullOrEmpty(Text)) return result;
            string[] sections = Lib.ToLat(SLst.NormStr(Text).ToLower()).Split(';');
            foreach (string str in sections)
            {
                Section x = new Section();
                x.SType = SecType(str);
                if (x.SType == Decl.STypeNOT_DEFINED) continue;
                x.body = SecBody(str);
                result.Add(x);
            }
            return result;
        }

        public List<int> GetFieldNumbers(string Text)
        {
            List<int> result = new List<int>();
            if (string.IsNullOrEmpty(Text)) return result;
            var match = Regex.Match(Text, @"{\d+}");
            while (match.Success)
            {
                string str = match.Value;
                if (string.IsNullOrEmpty(match.Value)) break;
                str = str.Substring(1, str.Length - 2);
                result.Add(Convert.ToInt32(str));
                match = match.NextMatch();
            }
            return result;
        }

        public List<int> GetFieldNumbers(List<string> txtLst, string whichSection = Decl.STypeNOT_DEFINED)
        {
            List<int> result = new List<int>();
            if (whichSection != Decl.STypeNOT_DEFINED)
            {
                foreach (string txt in txtLst)
                {
                    string str = Get(txt, whichSection).body;
                    if (str == string.Empty) continue;
                    return GetFieldNumbers(str);
                }
                throw new ArgumentException(@"No Section " + whichSection + "\"");
            }
            foreach (string txt in txtLst)
            {
                var lst = GetFieldNumbers(txt);
                foreach (int n in lst)
                {
                    if (result.Contains(n)) continue;
                    result.Add(n);
                    result.Sort();
                }
            }
            return result;
        }

        public string Get(List<Section> sections, string SType) =>
            sections.Find(s => s.SType == SType).body;

        protected string SecType(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains(':')) return string.Empty;

            string hdr = text.Substring(0, text.IndexOf(':'));
            hdr = Lib.ToLat(SLst.NormStr(hdr).ToLower());
            if (st.Is(hdr, out SType)) return SType;
            return string.Empty;
        }

        protected string SecBody(string str)
        {
            Match m = Regex.Match(str, ".*:");
            int ind = m.Value.Length;
            return str.Substring(ind);
        }
    } // end class Section

    public class SectionTab
    {
        public List<SLst> Tab = new List<SLst>();
        private SLst sLst = new SLst();

        public bool Is(string str, out string SType)
        {
            SType = null;
            string x = SLst.NormStr(str);
            foreach (SLst lst in Tab)
            {
                if (!lst.Is(x)) continue;
                SType = lst.Type(x);
                return true;
            }
            return false;
        }

        public SectionTab Init(params string[] str)
        {
            string foo;
            if (str.Count() == 0 || !Is(str[0], out foo))
                Tab.Add(new SLst(str));
            return this;
        }

        public string Type(string str)
        {
            string result = null;
            Is(str, out result);
            return result;
        }

        public static implicit operator SectionTab(List<SLst> v) => v;
        public static implicit operator List<SLst>(SectionTab st) => st.Tab;
    } // end class SectionTab
} // end namespace TSmatch.Section