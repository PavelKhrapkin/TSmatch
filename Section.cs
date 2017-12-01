/*----------------------------------------------------------------
 * Section -- class dealing with the fragment of string related to
 *            some section - f.e. Material, or Price
 *
 * 28.11.2017 Pavel Khrapkin
 *
 * --- Unit Tests ---
 * 2017.11.28  - UT_Section, UT_SecType OK
 *--- History ---
 *  7.03.2017 made from other module fragments
 * 19.03.2017 re-written with SectionTab as a argument of Constructor
 * 21.03.2017 call Bootstrap.initSection() for SectionTab
 * 28.03.2017 munli-header Section like "M: Def: body"
 *  8.08.2017 static constructor as a singleton initializator of SectionTab
 * 13.09.2017 cosmetic, multilanguage Msg
 * 28.11.2017 bug fix - "ед:" not recognozed SecType; UT_SecType re-made without Regex
 * ------ Fields ------
 * type section - recognized enum Section type, f.e. Material, Price etc
 * string body  - text string, contained in Section between ':' and ';' or end
 *                if no ':' found, return input string
 * --- Constructors ---  
 * public Section(string _text [, SType stype]) - if(SectionTab==null) initSection
 * ----- Methods: -----
 * isSectionMatch(string template) - check if this Section is in match with template
 */
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using log4net;
using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using Par = TSmatch.Parameter.Parameter;

namespace TSmatch.Section
{
    public class Section
    {
        public static readonly ILog log = LogManager.GetLogger("Section");

        public enum SType
        {
            NOT_DEFINED,
            Material, Profile, Price, Description,
            LengthPerUnit, VolPerUnit, WeightPerUnit,
            Unit, UNIT_Vol, UNIT_Weight, UNIT_Length, UNIT_Qty
        }

        public SType type;      // recognysed type of Section
        public string body;     // text after ':', but before first ';'
        public SType refSection; // reference to the Section with data

        static Dictionary<string, List<string>> SectionTab = new Dictionary<string, List<string>>();

        public Section() { }

        #region --- Singleton static constructor fill SectionTab Dictionary
        /// <summary>
        /// Singleton static SectionTab initialization
        /// </summary>
        static Section()
        {
            sub(SType.Material, "MAT", "m", "м");
            sub(SType.Profile, "PRF", "pro", "пр");
            sub(SType.Price, "CST", "cost", "pric", "цен", "сто");
            sub(SType.Description, "DES", "оп", "знач");
            sub(SType.LengthPerUnit, "LNG", "leng", "длин");
            sub(SType.VolPerUnit, "VOL", "об", "v");
            sub(SType.WeightPerUnit, "WGT", "вес", "w");
            // применение SType.Unit: заголовок для распознавания
            //.. "составных" секций, например, "ед: руб/т" 
            sub(SType.Unit, "UNT", "eд", "un");
            sub(SType.UNIT_Vol, "UNT_Vo", "руб/м3", "рублей/м3", "ст.куб");
            sub(SType.UNIT_Weight, "UNT_W", "руб/т", "рублей/т", "ст/т");
            sub(SType.UNIT_Length, "UNT_L", "погонный метр", "за м");
            sub(SType.UNIT_Qty, "UNT_Q", "шт", "1");
        }

        private static void sub(SType t, params string[] str)
        {
            List<string> lst = new List<string>();
            foreach (string s in str)
            {
                lst.Add(Lib.ToLat(s).ToLower().Replace(" ", "").Replace(".", "").Replace("/", ""));
            }
            SectionTab.Add(t.ToString(), lst);
        }
        #endregion --- Singleton static constructor fill SectionTab Dictionary

        public Section(string _text, SType stype = SType.NOT_DEFINED)
        {
            string[] sections = Lib.ToLat(_text).ToLower().Replace(" ", "").Split(';');
            foreach(string str in sections)
            {
                type = SecType(str);
                body = SecBody(str);
                refSection = SecRef(str);
                if(type == SType.Unit)
                {
                    type = SecType((body + ":").Replace("/", "").Replace(".", ""));
                    body = string.Empty;
                }
                if (stype == SType.NOT_DEFINED || stype == type) return;
            }
            type = SType.NOT_DEFINED;
            body = string.Empty;
        }

        protected SType SecType(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains(':'))
                return SType.NOT_DEFINED;
            string hdr = text.Substring(0, text.IndexOf(':'));
            hdr = Lib.ToLat(hdr).ToLower().Replace(" ", "");
            foreach (SType sec in Enum.GetValues(typeof(SType)))
            {
                if (sec == SType.NOT_DEFINED) continue;
                List<string> synonyms = SectionTab[sec.ToString()].ToList();
                var secFound = synonyms.Find(x => 
                    x.Length > hdr.Length? false:
                    x == hdr.Substring(0, x.Length));
                if (secFound != null) return sec;
            }
            return SType.NOT_DEFINED;
        }

 

        string SecBody(string str)
        {
            Match m = Regex.Match(str, ".*:");
            int ind = m.Value.Length;
            return str.Substring(ind);
        }

        SType SecRef(string str)
        {
            int indx = str.IndexOf(':');
            if (indx == -1) return SType.NOT_DEFINED;
            return SecType(str.Substring(indx + 1));
        }

#if OLD
        private SType CompSecType(string str)
        {
            foreach (SType sec in Enum.GetValues(typeof(SType)))
            {
                if (!sec.ToString().Contains("UNIT_")) continue;
                List<string> synonyms = SectionTab[sec.ToString()].ToList();
                foreach (string syn in synonyms)
                {
                    string t = syn.Replace("*", "(.*)");
                    Regex tr = new Regex(t);
                    if (tr.IsMatch(str)) return sec;
                }
            }
            return SType.NOT_DEFINED;
        }
        public bool isSectionMatch(string template)
        {
            if(!SectionTab.Any()) Msg.F("SectionTab is empty");
            string[] sections = Lib.ToLat(template).ToLower().Replace(" ", "").Split(';');
            foreach(string str in sections)
            {
                if (SecType(str) != type) continue;
                string templ = SecBody(str).Replace("*", "(.*?)");
                return Regex.IsMatch(body, templ);
            }
            return false;
        }

        public List<Par> secPars(string template)
        {
            List<Par> result = new List<Par>();
            template = Lib.ToLat(template).ToLower().Replace(" ", "");
            template = SecBody(template);
            string pattern = "^" + template.Replace("*", @"(.*?)") + "$";
            Regex r = new Regex(pattern);
            Match m = r.Match(body);
            for (int i = 1; i < m.Groups.Count; i++)
                result.Add(new Par(m.Groups[i].Value));
            return result;
        }
#endif // OLD
    } // end class Section
} // end namespace TSmatch.Section
