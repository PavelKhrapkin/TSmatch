/*----------------------------------------------------------------
 * FingerPrin (FP) -- characteristic text fragments and parameters
 *                    of CAD-model element attribute, Component, Rule
 *
 * 31.03.2017 Pavel Khrapkin
 *
 *--- History ---
 * 28.12.2016 made from other module fragments
 * 17.01.2017 class fields and identification method changed
 *  9.03.2017 Section class used
 * 21.03.2017 Parameter class use
 * 31.03.2017 Упрощенный FP c отделенным PRICE  
 *--- <ToDo> 2017.03.31:
 *  - упростить поля FP для работы только с уже обработанными прайс-листами
 * --- Constructors ---  
 ---* static FingerPrint() - initialyze static Dictionary Sections for RecognizeSection
 * public FingerPrint(type, str)            - initialyze FP of all types, but Components (constructor 1)
 * public FingerPrint(str, csFP, ref flag)  - initialyze FP for Component; return flag=true if OK (constructor 2)
 * ----- Methods: -----
 * - FP1.Equals.FP2 - FP1 and FP2 have same FP or, at least one of them (Rule) is in match with another
 *    --- miscelleneous ---
 * - getPar(str, ref startIndex, out bool isBrackets) - return parametr string in {..}, or string value
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
using Par = TSmatch.Parameter.Parameter;
using ParType = TSmatch.Parameter.Parameter.ParType;
using Sec = TSmatch.Section.Section;
using SType = TSmatch.Section.Section.SType;
using TST = TSmatch.Test.assert;

namespace TSmatch.FingerPrint
{
    public class FingerPrint : IEquatable<FingerPrint>
    {
        public static readonly ILog log = LogManager.GetLogger("FingerPrint");

        public string par;

        public FingerPrint(string str)
        {
            par = str;
        }

        public bool Equals(FingerPrint other)
        {
            if (other == null) return false;
            return par == other.par;
        }
        public int Int() { return Lib.ToInt(par); }
        public int Col() { return Int(); }
#if OLD_FP
        //31/3 попробую..        public enum type { Rule, CompSet, Component }

        //31/3        public readonly type typeFP;
        //31/3        public readonly Sec section;
        //31/3        public readonly List<string> txs = new List<string>();
        //31/3        public List<Par> pars = new List<Par>();
        //31/3        public SType refSection;

        //--- FP constructor 1 for Rule and CompSet
        public FingerPrint(type _type, string str)
        {
            typeFP = _type;    
            section = new Sec(str);
            refSection = section.refSection;
            Par p = new Par(str);
            pars.Add(p);
            txs.Add(p.tx);
        }

        //--- FP constructor 2 for Component
#if DEBUG   //--- 29-Mar-2017 Вариант конструктора для UT
        public FingerPrint(SType stype, dynamic obj)
        {
            typeFP = type.Component;
            if (stype == SType.Description)
            {
                Par p = new Par(obj);
                p.par = p.tx = obj;
                pars.Add(p);
                return;
            }
            section = new Sec(stype.ToString() + ":");
            string str = "";
            if (obj.GetType() == typeof(string)) str = (string)obj;
            if (obj.GetType() == typeof(double)) str = ((double)obj).ToString();
            pars.Add(new Par(str));
        }
#endif      //--- 29-Mar-2017 Вариант конструктора для UT
        ////////////////public FingerPrint(Sec.SType stype, string str, ParType parType = ParType.String)
        ////////////////{
        // 21/3 ////////    Param par = new Param(str, parType);
        ////////////////    pars.Add(par);
        ////////////////}

        public FingerPrint(string str, Rule.Rule rule, Sec sec)
        {
            FingerPrint ruleFP = rule.ruleFPs[sec.type];
            //        FingerPrint csFP = rule.CompSet.csFPs.Find(x => x.section.type == sec.type);
            SType stype = sec.type;
            var vv = rule.CompSet.csFPs;

//20/3 ЗАГЛУШКА!!            FingerPrint csFP = rule.CompSet.csFPs[stype];
            section = sec;
            string ruleText = sec.body;
            List<string> sPars = ReverseFomat(str, ruleText);
            int i = 0;
            foreach(var p in sPars)
            {
                //20/3 ЗАГЛУШКА!!                 Parameter.Parameter csPar = (Parameter.Parameter)csFP.pars[i++];
                //20/3 ЗАГЛУШКА!!                 Parameter.Parameter par = new Parameter.Parameter(str, csPar.ptype);
                //20/3 ЗАГЛУШКА!! pars.Add(par);
            }

            // str = Section.body в строке прайс-листа
            // template - Section.body в Rule.text
            // заполняет pars в FP для Component
            //12/3            foreach (var par in ReverseFormat(str, ruleText)) pars.Add(par);
        }



        /// <summary>
        /// ReverseFormat(str, template) - get fragmants os str where * wildcards are
        ///     into List<string> result -- idea from stackowerflow.com
        /// </summary>
        /// <param name="str"></param>
        /// <param name="template"></param>
        /// <returns>List<string> - str fragmants on "*" places</returns>
        private List<string> ReverseFomat(string str, string template)
        {
            List<string> parameters = new List<string>();
            string pattern = "^" + template.Replace("*", @"(.*?)") + "$";
            Match m = Regex.Match(str, pattern);
            for (int i = 1; i < m.Groups.Count; i++)
            {
                parameters.Add(m.Groups[i].Value);
            }
            return parameters;
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
            //////////////////public FingerPrint(string str, List<FingerPrint> csFPs, ref bool flag)
            //////////////////{
            //////////////////    typeFP = type.Component;
            //////////////////    flag = false;
            //////////////////    str = Lib.ToLat(str).ToLower();
            //////////////////    section = (Section)RecognyseSection(str);
            //////////////////    FingerPrint csFP = csFPs.Find(x => x.section == section);
            //////////////////    if (csFP == null) Msg.F("No Section \"" + section + " in csFPs");
            //////////////////    int ind = str.IndexOf(':') + 1;
            //////////////////    foreach (var tx in csFP.txs)
            //////////////////    {
            //////////////////        int lng = Math.Min(str.Length, tx.Length);
            /// 28/2/2017 ////        while (string.IsNullOrWhiteSpace(str[ind].ToString())) ind++;
            //////////////////        string s_str = str.Substring(ind, lng);
            //////////////////        string s_tx  = tx.Substring(0, lng);
            //////////////////        if (!isMatchStr(s_str, s_tx)) return;   //по несоответствию tx надо разбираться с синонимами - потом
            //////////////////        flag = true;
            //////////////////        txs.Add(s_tx);
            //////////////////        ind += tx.Length;
            //////////////////        bool isBrackets;
            //////////////////        string par = getPar(str, ref ind, out isBrackets);
            //////////////////        pars.Add(par);
            //////////////////        // 27/02 //                if (isBrackets) pars.Add(par); else txs.Add(par);
            //////////////////    }
            //////////////////}

        public FingerPrint(string str, FingerPrint csFP, out bool flag)
        { 
            flag = false;
            typeFP = type.Component;
            if (string.IsNullOrEmpty(str) || csFP == null) return;
            flag = true;
            section = csFP.section;
            if(str.Contains("*"))
            {
                pars = section.secPars(str);
            }
            Par par = new Par(str);
            pars.Add(par);
        }

        /// <summary>
        /// FingerPrint() - for call this class only
        /// </summary>
        public FingerPrint() {}

        public bool Equals(FingerPrint other)
        {
            bool ok = false;
            if (this == other) return true;
            if (this == null || other == null) return false;
//21/3            ok = EqLst(txs, other.txs) 
//21/3                && EqLst(synonyms, other.synonyms) 
//21/3                && EqLst(pars, other.pars);
            return ok;
        }

        private bool EqLst(List<object> pars1, List<object> pars2)
        {
            int lng = Math.Min(pars1.Count, pars2.Count);
            for(int i=0; i<lng; i++)
            {
                if (pars1[i].ToString() != pars2[i].ToString()) return false;
            }
            return true;
        }

        bool EqLst(List<string> str1, List<string> str2)
        {
            int lng = Math.Min(str1.Count, str2.Count);
            for (int i = 0; i < lng; i++)
            {
                if (str1[i].ToString() != str2[i].ToString()) return false;
            }
            return true;
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

        public int Int(int parNumber = 0)
        {
            if (parNumber < 0 || parNumber >= pars.Count)
                Msg.F("FP.Int() Not supported pars.Count", pars.Count);
            if(pars[parNumber].GetType() == typeof(string))
            {
                string str = (string)pars[parNumber].par;
                int result =  Lib.ToInt(str);
                if (result == -1) Msg.F("FP TMP not recognized Int", str);
                return result;
            }
            if (pars[parNumber].par.GetType() == typeof(int))
                return (int)pars[parNumber].par;
            if (pars[parNumber].par.GetType() == typeof(string))
                return Lib.ToInt((string)pars[parNumber].par);
            Msg.F("ErrFP string or integer parametr exected");
            return -1;
        }
        public int Col(int parNumber = 0)
        {
            int result = Int(parNumber);
            if (result < 1) Msg.F("FP.Col wrong parametr", result);
            return result;
        }

        public string parN(int n = 0)
        {
            return pars[n].par.ToString();
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
            FingerPrint fp = new FingerPrint();
            //-- Main constructors test:
//17/3            testFP_constuctor1(fp); // Rule and CompSet test
//13/3            testFP_constuctor2(fp); // Component test

            //-- Equal test
            testFP_Equals(fp);
            testFP_testEqLst(fp);

            //-- other FP servise methods 
//7/3            testFP_RecognizeSection(fp);
            testFP_Int_Col();
            testFP_getPar(fp);
            testFP_isMatchStr(fp);
            testFP_synonym(fp);

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

            List<string> str1 = new List<string>();
            List<string> str2 = new List<string>();
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

            //-- test with int and double elements
            List<object> p1 = new List<object>();
            List<object> p2 = new List<object>();
            p1.Add(1); p2.Add("1");
            TST.Eq(fp.EqLst(p1, p2), true);
            p1.Add(2.58); p2.Add("2,58");
            TST.Eq(fp.EqLst(p1, p2), true);

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
            FingerPrint comp = new FingerPrint("В12,5", csMonolit, out flag);
            TST.Eq(flag, true);
            TST.Eq(comp.pars.Count, 1);
            TST.Eq(comp.pars[0], "12,5");

            comp = new FingerPrint("", csMonolit, out flag);
            TST.Eq(flag, false);

            string sL = "Угoлoк cтaльнoй paвнoпoл. 100 x 8 cт3cп / пc5";
            Docs doc = Docs.getDoc("Уголок Стальхолдинг");
            FingerPrint csAngle = new FingerPrint(type.CompSet, doc.LoadDescription);
            comp = new FingerPrint(sL, csAngle, out flag);

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

//////////////////TST.Eq(fp.getParType("{2}").ToString(), "String");
//////////////////TST.Eq(fp.getParType("{s~2}").ToString(), "String");
//// 20/3/17 /////TST.Eq(fp.getParType("{i~4}").ToString(), "Integer");
//////////////////TST.Eq(fp.getParType("{d~3}").ToString(), "Double");
//////////////////TST.Eq(fp.getParType("{digital~3}").ToString(), "Double");
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
#endif //OLD_FP
    } //end class Fingerprint
} // end namespace