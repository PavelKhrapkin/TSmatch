/*----------------------------------------------------------------
 * FingerPrin (FP) -- characteristic name fragments and parameters
 *                    of model element attribute, Component, Rule
 *
 * 17.01.2017 Pavel Khrapkin
 *
 *--- History ---
 * 28.12.2016 made from other module fragments
 * 17.01.2017 class fields and identification method changed
 *--- <ToDo> 2017.1.10:
 *  - реализовать разбор синонимов в конструкторе 
 *  - реализовать Equals - идентификацию
 *--- <ToDo> 2017.1.16
 *  - заменить string tx на List<string>txs. Реализовать заполнение txs в конструкторе FingerPrint
 * --- Constructors ---  
 * static FingerPrint() - initialyze static Dictionary Sections for RecognizeSection
 * public FingerPrint(type, str)            - initialyze FP of all types, but Components (constructor 1)
 * public FingerPrint(str, csFP, ref flag)  - initialyze FP for Component; return flag=true if OK (constructor 2)
 * ----- Methods: -----
 * - FP1.Equals.FP2 - FP1 and FP2 have same FP or, at least one of them (Rule) is in match with another
 *    --- miscelleneous ---
 * - getPar(str, ref startIndex)            - return parametr string in {..}, shift startIndex after }
 * - getParType(str)                        - return enum {string, double, int} type of parametr sting
 * - RecognyseSection(string str)           - return enum Section, which matches to str
 * - synParser(string tx, ref int iTx)      - return synonym, ended with = from tx, starting from iTx char
 * - isSynonym(string str, string pattern)  - return TRUE, when str is in match with pattern section
 * - synParser(string str, ref iTx)         - return Dictionary, containes parsed parametr par
 * - isMatchStr(string str1, string str2)   - check if str1 match to str2 with wildcard (*) accounted
 * - int Int(int parNumber = 0)             - return int pars[parNumber] as int
 * - int Col(int Parnumber = 0)             - return Excel column number -- specially with CompSet
 * - strINFO()                              - return string containes FP for trace purpaces
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

using Log = match.Lib.Log;
using Decl = TSmatch.Declaration.Declaration;
using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using Mtr = match.Matrix.Matr;
using FileOp = match.FileOp.FileOp;
using Docs = TSmatch.Document.Document;
using TST = TSmatch.Test.assert;

namespace TSmatch.FingerPrint
{
    public class FingerPrint : IEquatable<FingerPrint>
    {
        public static readonly ILog log = LogManager.GetLogger("FingerPrint");

        public enum type { Rule, CompSet, Component }
        static Dictionary<string, List<string>> Sections = new Dictionary<string, List<string>>();

        public readonly type typeFP;
        public readonly Section section;
        public readonly List<string> txs = new List<string>();
        public readonly List<object> pars = new List<object>();
        public readonly List<string> synonyms = new List<string>();

        static FingerPrint()
        {
            //-- Columns in "TSmatch.xlsc/Constants/TabSynonyn"
            const int SECTION_NAME = 1;
            const int SECTION_DEF  = 2;
            const int SECTION_DESCR= 3; //this colunn used as a comment only
            const int SECTION_SYNS = 4;

            Mtr rawTab = FileOp.getRange("TabSynonym");
            for (int i=1; i<=rawTab.iEOL(); i++)
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
        public FingerPrint(type _type, string str)
        {
            str = Lib.ToLat(str.Replace(";", "")).ToLower();
            typeFP = _type;
            section = (Section)RecognyseSection(str);
            int ind = str.IndexOf(':') + 1;
            string sectionBody = str.Substring(ind).Trim();
            while(ind < str.Length)
            {
                int iStart = ind, lng;
                bool isBrackets;
                string par = getPar(str, ref ind, out isBrackets);
                if (isBrackets && par.Length > 0)
                {
//                    if (isBrackets && typeFP == type.Component) Msg.F("FP TMP unexpected {}");
                    if (typeFP == type.CompSet) pars.Add(Lib.ToInt(par));
                    else pars.Add(par);
                    lng = ind - iStart - par.Length - 2;
                }
                else lng = str.Length - iStart;
                string tx = str.Substring(iStart, lng).Trim();
                int iTx = 0;
                while (isSynonym(tx, iTx)) synonyms.Add(synParser(tx, ref iTx));
                txs.Add(tx.Substring(iTx));
            }
        }

        /// <summary>
        /// FingerPrint(str, FP csFP, ref flag) -- FP constructor- specially for Component
        /// </summary>
        /// <param name="str">string to be parsed with CompSet FPs</param>
        /// <param name="_section">section of str the FP belongs to</param>
        /// <param name="csFPs">FP  from CompSet</param>
        /// <note>
        /// - parse str with csFP to put price-list parameters in this FP
        /// - quantity of tx in txs is always pars.Count (may be +1)
        /// - 
        /// </note>
        public FingerPrint(string str, List<FingerPrint> csFPs, ref bool flag)
        {
            typeFP = type.Component;
            flag = false;
            str = Lib.ToLat(str).ToLower();
            section = (Section)RecognyseSection(str);
            FingerPrint csFP = csFPs.Find(x => x.section == section);
            if (csFP == null) Msg.F("No Section \"" + section + " in csFPs");
            int ind = str.IndexOf(':') + 1;
            foreach (var tx in csFP.txs)
            {
                int lng = Math.Min(str.Length, tx.Length);
                while (string.IsNullOrWhiteSpace(str[ind].ToString())) ind++;
                string s_str = str.Substring(ind, lng);
                string s_tx  = tx.Substring(0, lng);
                if (!isMatchStr(s_str, s_tx)) return;   //по несоответствию tx надо разбираться с синонимами - потом
                flag = true;
                txs.Add(s_tx);
                ind += tx.Length;
                bool isBrackets;
                string par = getPar(str, ref ind, out isBrackets);
                pars.Add(par);
                // 27/02 //                if (isBrackets) pars.Add(par); else txs.Add(par);
            }
        }

        private string getPar(string str, ref int startIndex, out bool isBrackets)
        {
            const string PARAMETR = @"\{.+?\}";
            Regex parametr = new Regex(PARAMETR, RegexOptions.IgnoreCase);
            Match m = parametr.Match(str, startIndex);
            string result;
            if (m.Value.Length > 0)
            {   // string like "{result}"
                startIndex = m.Index + m.Value.Length;
                isBrackets = true;
                result = Regex.Replace(m.Value, @"\{|\}", "");
            }
            else
            {   // result is part of str, recognised as a parameter value
                result = str.Substring(startIndex).Trim();
                startIndex = str.Length;
                isBrackets = false;
            }
            return result;
        }
        private enum ParType { String, Integer, Double }
        private ParType getParType(string str)
        {
            const string PAR_TYPE = @"\{(s|d|i).*?~";
            ParType result = ParType.String;
            Regex parType = new Regex(PAR_TYPE, RegexOptions.IgnoreCase);
            Match m = parType.Match(str);
            if (m.Value == "") return result;
            switch(m.Value[1])
            {
                case 's': break;
                case 'd': result = ParType.Double; break;
                case 'i': result = ParType.Integer; break;
            }
            return result;
        }

        public enum Section
        {
            Material, Profile, Description, Price,
            Use, Unit, WeightPerUnit, LengthPerUnit, VolPerUnit
        }
        public Section? RecognyseSection(string str)  
        {
            foreach (Section sec in Enum.GetValues(typeof(Section)))
            {
                List<string> synonyms = Sections[sec.ToString()].ToList();
                foreach (string syn in synonyms)
                {
                    if (Regex.IsMatch(str, syn + ".*?:", RegexOptions.IgnoreCase)) return sec;
                }
            }
            Msg.F("Section Not Recorgized", str);
            return null;
        }

        private string synParser(string tx, ref int iTx)
        {
            string result = tx.Substring(iTx, tx.IndexOf('='));
            iTx += result.Length + 1;
            return result;
        }
        private bool isSynonym(string tx, int iStart)
        {
            return tx.Substring(iStart).Contains('=');
        }

        bool isMatchStr(string str1, string str2)
        {
            string s1 = str1.Replace("*", ".*?");
            string s2 = str2.Replace("*", ".*?");
            bool r1 = Regex.IsMatch(s1, "^" + s2);
            bool r2 = Regex.IsMatch(s2, "^" + s1);
            return r1 || r2;
        }

        internal int Int(int parNumber = 0)
        {
            if (parNumber < 0 || parNumber >= pars.Count)
                Msg.F("FP.Int() Not supported pars.Count", pars.Count);
            if(pars[parNumber].GetType() == typeof(string))
            {
                string str = (string)pars[parNumber];
                int result =  Lib.ToInt(str);
                if (result == -1) Msg.F("FP TMP not recognized Int", str);
                return result;
            }
            if (pars[parNumber].GetType() == typeof(int)) return (int)pars[parNumber];
            Msg.F("ErrFP string or integer parametr exected");
            return -1;
        }
        internal int Col(int parNumber = 0)
        {
            int result = Int(parNumber);
            if (result < 1) Msg.F("FP.Col wrong parametr", result);
            return result;
        }

        //TODO 10.1.17 переписать!
        public bool Equals(FingerPrint other)
        {
            bool ok = false;
            if (this == other) return true;
            if (this == null || other == null) return false;
//!!            ok = EqLst(txs, other.txs) && EqLst(synonyms, other.synonyms);
            
            return ok;
        }

        bool EqLst(List<object> str1, List<object> str2)
        {
            int lng = Math.Min(str1.Count, str2.Count);
            for (int i = 0; i < lng; i++)
            {
                if (str1[i].ToString() != str2[i].ToString()) return false;
            }
            return true;
        } 

        internal string Info()
        {
            string result = "FP.Info: " + typeFP.ToString();
            result += " Section=" + section.ToString();
//////////            result += " tx=" + tx;
            result += "\tpars={";
            int ip = 0;
            foreach (var p in pars)
            {
                // 17.1 //                result += p.Key + "=" + p.Value;
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
                // 17.1 //                result += p.Key + "=" + p.Value;
                ip++;
                if (ip != pars.Count) result += ", ";
            }
            result += "}";
            return result;
        }

        #region ------ test FP -----
#if DEBUG
        internal static void testFP()
        {
            Log.set("testFP");
            FingerPrint fp = new FingerPrint(type.Rule, "M:");
            testFP_Int_Col();
            testFP_getPar(fp);
            testFP_isMatchStr(fp);
            testFP_synonym(fp);
            testFP_constuctor1(fp);
            testFP_constuctor2(fp);
            testFP_testEqLst(fp);
            testFP_Equals(fp);
            Log.exit();
        }

        private static void testFP_Equals(FingerPrint fp)
        {
            Log.set("testFP_Equals");
            FingerPrint A = new FingerPrint(type.Rule, "M:ABCD");
            FingerPrint B = null;
            TST.Eq(A.Equals(B), false);

            FingerPrint C = new TSmatch.FingerPrint.FingerPrint(type.Rule, "M:C");
            TST.Eq(A.Equals(C), false);
            TST.Eq(C.Equals(A), false);
            TST.Eq(C.Equals(B), false);

            Log.exit();
        }

        private static void testFP_testEqLst(FingerPrint fp)
        {
            Log.set("testFP_testEqLst");

            List<object> str1 = new List<object>();
            List<object> str2 = new List<object>();
            str1.Add("A"); str1.Add("B");
            str2.Add("A"); str2.Add("B"); str2.Add("C");


            TST.Eq(fp.EqLst(str1, str2), true);
            TST.Eq(fp.EqLst(str2, str1), true);
            str1.Add("XX");
            TST.Eq(fp.EqLst(str1, str2), false);

            str1.Clear();
            TST.Eq(fp.EqLst(str1, str2), true);
            str2.Clear();
            TST.Eq(fp.EqLst(str1, str2), true);

            str1.Add("xX");
            TST.Eq(fp.EqLst(str1, str2), true);
            str2.Add("X");
            TST.Eq(fp.EqLst(str1, str2), false);

            str1.Clear(); str2.Clear();
            str1.Add(1); str2.Add("1");
            TST.Eq(fp.EqLst(str1, str2), true);
            str1.Add(2.58); str2.Add("2,58");
            string z1 = str1[1].ToString();
            string z2 = str2[1].ToString();
            TST.Eq(fp.EqLst(str1, str2), true);

            Log.exit();
        }

        private static void testFP_constuctor1(FingerPrint fp)
        {
            Log.set("testFP_constructor1");

            FingerPrint xr2 = new FingerPrint(type.Rule, "Проф: L{сторона1}x{сторона2}x{длина} сварной");
            TST.Eq(xr2.section, "Profile");
            TST.Eq(xr2.pars.Count, 3);
            TST.Eq(xr2.txs.Count, 4);
            TST.Eq(xr2.txs[3], Lib.ToLat("сварной"));

            FingerPrint rule = new FingerPrint(type.Rule, "M:B{класс бетона};");
            TST.Eq(rule.pars.Count, 1);
            TST.Eq(rule.typeFP, "Rule");
            TST.Eq(rule.section.ToString(), "Material");
            TST.Eq(rule.pars[0], Lib.ToLat("класс бетона"));
            TST.Eq(rule.txs.Count, 1);
            TST.Eq(rule.txs[0], "b");

            xr2 = new FingerPrint(type.Rule, "M:B12,5");
            TST.Eq(xr2.section.ToString(), "Material");
            TST.Eq(xr2.pars.Count, 0);
            TST.Eq(xr2.txs.Count, 1);
            TST.Eq(xr2.txs[0].ToString(), "b12,5");

            xr2 = new FingerPrint(type.Rule, "Профиль:");
            TST.Eq(xr2.section.ToString(), "Profile");

            FingerPrint xr1 = new FingerPrint(type.CompSet, "Описание: {3}");
            TST.Eq(xr1.pars.Count, 1);
            TST.Eq(xr1.typeFP, "CompSet");
            TST.Eq(xr1.section, "Description");
            TST.Eq(xr1.Col(), 3);

            xr1 = new FingerPrint(type.CompSet, "Цена: {4} если нет другого материала в описании");
            TST.Eq(xr1.pars.Count, 1);
            TST.Eq(xr1.typeFP, "CompSet");
            TST.Eq(xr1.section.ToString(), "Price");
            TST.Eq(xr1.Col(), 4);
            TST.Eq(xr1.txs.Count, 2);
            TST.Eq(xr1.txs[0], "");
            TST.Eq(xr1.txs[1].Length > 10, true);

            xr1 = new FingerPrint(type.CompSet, "Цена: {4} НДС {12}{14}%");
            TST.Eq(xr1.pars.Count, 3);
            TST.Eq(xr1.txs[3], "%");
            TST.Eq(xr1.Col(2), 14);
            TST.Eq(xr1.txs.Count, 4);
            TST.Eq(xr1.txs[3], "%");
            TST.Eq(xr1.typeFP, "CompSet");
            TST.Eq(xr1.section.ToString(), "Price");
            TST.Eq(xr1.Col(), 4);

            Log.exit();
        }

        private static void testFP_constuctor2(FingerPrint fp)
        {
            Log.set("testFP_constructor2 - constructor Component.FP");

            FingerPrint csMonolit = new FingerPrint(type.CompSet, "M:B{1};");
            TST.Eq(csMonolit.pars.Count, 1);
            TST.Eq(csMonolit.Col(), 1);
            TST.Eq(csMonolit.txs.Count, 1);
            TST.Eq(csMonolit.txs[0], "b");
            List<FingerPrint> csFPs = new List<FingerPrint>();
            csFPs.Add(csMonolit);
            bool flag = false;
//13/2            FingerPrint comp = new FingerPrint("В12,5", csMonolit, ref flag);
            FingerPrint comp = new FingerPrint("M:В12,5", csFPs, ref flag);
            TST.Eq(flag, true);
            TST.Eq(comp.pars.Count, 1);
            TST.Eq(comp.pars[0], "12,5");

            //////////////////string sL = "Профиль: L=Уголок*{1}x{2}{материал уголков};";
            //////////////////FingerPrint LoadDescriptor = new FingerPrint(type.CompSet, sL);
            //////////////////bool ok = false;
            //////////////////string sComp = "Угoлoк cтaльнoй paвнoпoл. 100 x 7 cт3cп/пc5";
            //////////////////FingerPrint comp = new FingerPrint(sComp, LoadDescriptor, ref ok);

            //           xr1 = new FingerPrint("L18x24", Section.Profile, csFPs);
            //            xr1 = new FingerPrint("B20", csFPs);
            //////////////////////////csFPs.Add(LoadDescriptor);
            //////////////////////////TST.Eq(xr1.typeFP.ToString(), "Component");
            //////////////////////////TST.Eq(xr1.section.ToString(), "Material");
            /// 21/1 /////////////////TST.Eq(xr1.txs.Count, 1);
            //////////////////////////TST.Eq(xr1.txs[0], "b");
            //////////////////////////TST.Eq(xr1.pars.Count, 1);
            //////////////////////////TST.Eq(xr1.pars[0], "20");

            //            throw new NotImplementedException();
            Log.exit();
        }

        private static void testFP_getPar(FingerPrint fp)
        {
            Log.set("testFP_getPar()");

            int ind = 0;
            bool isBrackets;
            // for StepIn //string s = fp.getPar("{123}", ref ind);
            TST.Eq(fp.getPar("{123}", ref ind, out isBrackets), "123");
            TST.Eq(ind, 5);
            TST.Eq(isBrackets, true);
            ind = 0;
            string st = "параметр-значение";
            TST.Eq(fp.getPar(st, ref ind, out isBrackets), st);
            TST.Eq(ind, st.Length);
            TST.Eq(isBrackets, false);

            ind = 0;
            TST.Eq(fp.getPar("", ref ind, out isBrackets), "");
            TST.Eq(ind, 0);
            TST.Eq(isBrackets, false);

            TST.Eq(fp.getPar("{3", ref ind, out isBrackets), "{3");
            TST.Eq(isBrackets, false);

            ind = 0;
            TST.Eq(fp.getPar("2}", ref ind, out isBrackets), "2}");
            TST.Eq(ind, 2);
            TST.Eq(isBrackets, false);

            ind = 0;
            string str = "tx1{32}tx2";
            string par = fp.getPar(str, ref ind, out isBrackets);
            TST.Eq(par, "32");
            int indx = ind - par.Length - 2;
            TST.Eq(ind, 7);
            TST.Eq(indx, 3);
            TST.Eq(isBrackets, true);
            string tx1 = str.Substring(0, indx);
            TST.Eq(tx1, "tx1");

            str = "{323}tx2";
            ind = 0;
            TST.Eq(fp.getPar(str, ref ind, out isBrackets), "323");
            string tx2 = str.Substring(ind);
            TST.Eq(ind, 5);
            TST.Eq(tx2, "tx2");
            TST.Eq(isBrackets, true);

            ind = 1;
            TST.Eq(fp.getPar("ghj{323}", ref ind, out isBrackets), "323");
            TST.Eq(ind, 8);
            TST.Eq(isBrackets, true);

            ind = 0;
            str = "abc{1}ghj{2}";
            TST.Eq(fp.getPar(str, ref ind, out isBrackets), "1");
            TST.Eq(isBrackets, true);

            int i1 = ind;
            TST.Eq(fp.getPar(str, ref ind, out isBrackets), "2");
            int indx2 = ind - 3    - i1;
            string ghj = str.Substring(i1, indx2);
            TST.Eq(ghj, "ghj");
            TST.Eq(isBrackets, true);

            TST.Eq(fp.getParType("{2}").ToString(), "String");
            TST.Eq(fp.getParType("{s~2}").ToString(), "String");
            TST.Eq(fp.getParType("{i~4}").ToString(), "Integer");
            TST.Eq(fp.getParType("{d~3}").ToString(), "Double");
            TST.Eq(fp.getParType("{digital~3}").ToString(), "Double");
            Log.exit();
        }

        private static void testFP_Int_Col()
        {
            Log.set("test__Int_Col(fp)");
            FingerPrint fp = new FingerPrint(type.CompSet, "PRF: L{2}");
            TST.Eq(fp.Col(), 2);
            Log.exit();
        }

        private static void testFP_isMatchStr(FingerPrint fp)
        {
            Log.set("test_isMatchStr(fp)");

            TST.Eq(fp.isMatchStr("", ""), true);
            TST.Eq(fp.isMatchStr("A", "B"), false);
            TST.Eq(fp.isMatchStr("A*", "AB"), true);
            TST.Eq(fp.isMatchStr("A*", "B"), false);
            TST.Eq(fp.isMatchStr("A*", "ABB"), true);
            TST.Eq(fp.isMatchStr("A*", "BA"), false);
            TST.Eq(fp.isMatchStr("A*", "AB*B"), true);

            Log.exit();
        }

        private static void testFP_synonym(FingerPrint fp)
        {
            Log.set("testFP_synonym(fp)");

            int iStart = 0;
            string str = Lib.ToLat("L=Уголок*").ToLower();
            TST.Eq(fp.isSynonym(str, iStart), true);

            TST.Eq(fp.synParser(str, ref iStart), "l");
            TST.Eq(iStart, 2);
            string tx = str.Substring(iStart);
            TST.Eq(tx, Lib.ToLat("Уголок*").ToLower());
            Log.exit();
        }
#endif //#if DEBUG
        #endregion ------ testFP ------
    } //end class Fingerprint
} // end namespace