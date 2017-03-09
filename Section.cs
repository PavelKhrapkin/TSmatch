/*----------------------------------------------------------------
 * Section -- class dealing with the fragment of string related to
 *            some section - f.e. Material, or Price
 *
 * 7.03.2017 Pavel Khrapkin
 *
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

        static Dictionary<string, List<string>> Sections = new Dictionary<string, List<string>>();

        static Section()
        {
            //-- Columns in "TSmatch.xlsc/Constants/TabSynonyn"
            const int SECTION_NAME  = 1;
            const int SECTION_DEF   = 2;
            const int SECTION_DESCR = 3; //this colunn in table used as a comment only
            const int SECTION_SYNS  = 4;

            Mtr rawTab = FileOp.getRange("TabSections");
            for (int i = 1; i <= rawTab.iEOL(); i++)
            {
                string sectionHdr = rawTab.Strng(i, SECTION_NAME);
                string[] synonymsRaw = rawTab.Strng(i, SECTION_SYNS).Split(',');
                List<string> synonym = new List<string>();
                synonym.Add(Lib.ToLat(rawTab.Strng(i, SECTION_DEF).ToLower()));
                foreach (string syn in synonymsRaw)
                    synonym.Add(Lib.ToLat(syn).ToLower().Trim());
                Sections.Add(sectionHdr, synonym);
            }
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
                List<string> synonyms = Sections[sec.ToString()].ToList();
                foreach (string syn in synonyms)
                {
                    if (Regex.IsMatch(text, syn + ".*?", RegexOptions.IgnoreCase))
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
        internal static void testSection()
        {
            Log.set("testSection");
            //-- test static Sections Dictionary to be initialized at 1st start
            TST.Eq(Sections.Count, 8);
            TST.Eq(Sections["Material"][0], "mat");
            TST.Eq(Sections[Type.Price.ToString()][3], "ц");
            TST.Eq(Sections[Type.Profile.ToString()][2], "пp");

            Section sec = new Section("");
            TST.Eq(sec.body, "");
            TST.Eq(sec.type == Type.NOT_DEFINED, true);

            string str = "Mater: B12,5; Prf: L 20x4";
            sec = new Section(str);
            TST.Eq(sec.type == Type.Material, true);
            TST.Eq(sec.body, "b12,5");
            Log.exit();

            //----- error input text handling -----
            sec = new Section("Цена 2540");
            TST.Eq(sec.type == Type.Price, true);
            TST.Eq(sec.body, "");

            sec = new Section("Цена 2540;");
            TST.Eq(sec.type == Type.Price, true);
            TST.Eq(sec.body, "");

            sec = new Section("; профиль: L");
            TST.Eq(sec.type == Type.NOT_DEFINED, true);
            TST.Eq(sec.body, "");
        }

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
}
