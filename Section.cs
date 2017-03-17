/*----------------------------------------------------------------
 * Section -- class dealing with the fragment of string related to
 *            some section - f.e. Material, or Price
 *
 * 16.03.2017 Pavel Khrapkin
 *
 *---<ToDo> 2017.03.16 - Section,init() для пустого Dictionary
 *--- History ---
 *  7.03.2017 made from other module fragments
 * ------ Fields ------
 * type section - recognized enum Section type, f.e. Material, Price etc
 * string body  - text string, contained in Section between ':' and ';' oe end
 * --- Constructors ---  
 * static Section() - initialyze static Dictionary Sections for RecognizeSection
 * public Section(text)     - initialyze Section fiels from sring text
 * ----- Methods: -----
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

using TST = TSmatch.Test.assert;
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Mtr = match.Matrix.Matr;
using FileOp = match.FileOp.FileOp;

namespace TSmatch.Section
{
    public class Section
    {
        public static readonly ILog log = LogManager.GetLogger("Section");

        public enum Type
        {
            NOT_DEFINED,
            Material, Profile, Price, Description,
            LengthPerUnit, VolPerUnit, WeightPerUnit, Unit
        }

        public Type type;       // recognysed type of Section
        public string body;     // text after ':', but before first ';'

        static Dictionary<string, List<string>> SectionTab = new Dictionary<string, List<string>>();

        static Section()
        {
            sub(Type.Material,      "MAT", "m");
            sub(Type.Profile,       "PRF", "pro", "пр");
            sub(Type.Price,         "CST", "cost", "pric", "цен", "сто");
            sub(Type.Description,   "DES", "оп", "знач");
            sub(Type.LengthPerUnit, "LNG", "leng", "длин");
            sub(Type.VolPerUnit,    "VOL", "об");
            sub(Type.WeightPerUnit, "WGT", "вес", "w");
            sub(Type.Unit,          "UNT", "ед", "un");
        }

        static void sub(Type t, params string[] str)
        {
            List<string> lst = new List<string>();
            foreach(string s in str) lst.Add(Lib.ToLat(s).ToLower());
            SectionTab.Add(t.ToString(), lst);
        }

        public Section(string _text)
        {
            string[] sections = Lib.ToLat(_text).ToLower().Replace(" ", "").Split(';');
            string text = sections[0];  //when text contains few sections - recognise the first only
            type = Type.NOT_DEFINED;
            body = string.Empty;
            if (string.IsNullOrEmpty(text)) return;
            // recognyze type of Section in text
            foreach (Type sec in Enum.GetValues(typeof(Type)))
            {
                if (sec == Type.NOT_DEFINED) continue;
                List<string> synonyms = SectionTab[sec.ToString()].ToList();
                foreach (string syn in synonyms)
                {
                    if (Regex.IsMatch(text, Lib.ToLat(syn).ToLower() + ".*?"))
                    {
                        type = sec;
                        goto Found;
                    }
                }
            }
            //  if we come here - Section Not Recorgized
Found:      int ind = text.IndexOf(':') + 1;
            if (ind == 0) ind = text.Length;
            body = text.Substring(ind);
        }

        private void init()
        {
            throw new NotImplementedException();
        }

        internal static bool isSectionMatch(Type type, string mat_prf, string textRule)
        {
            bool ok = false;
            string[] sections = textRule.Split(';');
            foreach(string s in sections)
            {
                Section sec = new Section(s);
                if (sec.type != type) continue;
                //---- тут сравним, напр. "L20x4" и "Уголок=L*x*"
                //????????????????????????????????
                ok = true;
            }
            if (!ok) return true;   // если в Правиле нет секции -- годится все (??)

            throw new NotImplementedException();
        }

        /*
   private bool isSectionMatch(FP.Section sec, string grPar, string text)
   {

       Log.set("isSectionMatch(" + sec.ToString() + ", \"" + grPar + "\", rule.text");
       bool ok = false;
       if (string.IsNullOrWhiteSpace(text)) return ok;
       string sectionText = getSectionText(sec, text);

       //            throw new NotImplementedException();

       Log.exit();
       return ok;
   }

   private string getSectionText(FP.Section sec, string text)
   {
       Log.set("getSectionText(" + sec.ToString() + ", \"" + text + "\")");
       text = Lib.ToLat(text).ToLower().Replace(" ", "");
       string[] sections = text.Split(';');
       FP fp = new FP();
       foreach (string s in sections)
       {
           //               if(!fp.recognizeSection()
       }
       TST.Eq(false, true);
       throw new NotImplementedException();
       Log.exit();
   }
*/
        #region ------ test Section -----
#if DEBUG
        ////internal static void testSection()
        ////{
        ////    Log.set("testSection");
        ////    //-- test static Sections Dictionary to be initialized at 1st start
        ////    TST.Eq(Sections.Count, 8);
        ////    TST.Eq(Sections["Material"][0], "mat");
        ////    TST.Eq(Sections[Type.Price.ToString()][3], "ц");
        ////    TST.Eq(Sections[Type.Profile.ToString()][2], "пp");

        ////    Section sec = new Section("");
        ////    TST.Eq(sec.body, "");
        ////    TST.Eq(sec.type == Type.NOT_DEFINED, true);

        ////    string str = "Mater: B12,5; Prf: L 20x4";
        ////    sec = new Section(str);
        ////    TST.Eq(sec.type == Type.Material, true);
        ////    TST.Eq(sec.body, "b12,5");
        ////    Log.exit();

        ////    //----- error input text handling -----
        ////    sec = new Section("Цена 2540");
        ////    TST.Eq(sec.type == Type.Price, true);
        ////    TST.Eq(sec.body, "");

        ////    sec = new Section("Цена 2540;");
        ////    TST.Eq(sec.type == Type.Price, true);
        ////    TST.Eq(sec.body, "");

        ////    sec = new Section("; профиль: L");
        ////    TST.Eq(sec.type == Type.NOT_DEFINED, true);
        ////    TST.Eq(sec.body, "");
        ////}

        ////////private void test_getSectionText()
        ////////{
        ////////    Log.set("test_getSectionTest(Section.Material, text");
        ////////    TST.Eq(getSectionText(FP.Section.Material, "Профиль: L 20 x 5; M: C245; Price: 2690"), "c245");
        ////////    Log.exit();
        ////////}

        ////////private void test_isSectionMatch()
        ////////{
        ////////    Log.set("isSectionMatch(Section.Material, C245, rule.text)");

        ////////    bool ok = isSectionMatch(FP.Section.Material, "C245", "Профиль: L * x * ст*; длина: * = * м; M: ст *;");

        ////////    Log.exit();
        ////////}
#endif //#if DEBUG
        #endregion ------ test Section ------
    }

    public class uts
    {
        public string res;

        public uts(string str)
        {
            var section = new Section(str);
            res = str;
        }
        public bool ut_isSetionMatch(Section.Type t, string mat_prf, string textRule)
        {
            return TSmatch.Section.Section.isSectionMatch((Section.Type)t, mat_prf, textRule);
        }
    }
} // end namespace TSmatch.Section
