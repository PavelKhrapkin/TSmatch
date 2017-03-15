/*----------------------------------------------------------------
 * Parameter -- class dealing a string like "tx{par}"
 *
 * 9.03.2017 Pavel Khrapkin
 *
 *--- History ---
 *  9.03.2017 made from FingerPrint code fragments
 * ------ Fields ------
 * tx - text before {par}
 * par - part of input string, recognized with 
 *      par could has a prefix like ~s:
 *          ~s or by default - string
 *          ~i - int
 *          ~d - double
 * type {string, int, double}
 *      type could be enum Type {string, int, double}; string by default
 * List<string> synonyms - possible alternative tx equivalents
 * --- Constructor ---  
 * internal Parameter(string str) - fill Parameter fields from string str
 * internal Parameter(string str, ParType _type) - convert str to _type
 * ----- Methods: -----
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;

using TST = TSmatch.Test.assert;
using Log = match.Lib.Log;
using Lib = match.Lib.MatchLib;
using FP = TSmatch.FingerPrint.FingerPrint;

namespace TSmatch.Parameter
{
    internal class Parameter
    {
        public static readonly ILog log = LogManager.GetLogger("Parameter");

        internal ParType type;
        internal string tx;
        internal object par;
        private bool isBrackets = false;
//12/3        private int indx = 0;
//12/3        private List<string> synonyms = new List<string>();

        internal Parameter(string str)
        {
            const string PARAMETR = @"\{.+?\}";
            Regex parametr = new Regex(PARAMETR, RegexOptions.IgnoreCase);
            int indx = 0;
            Match m = parametr.Match(str, indx);
            str = Lib.ToLat(str).ToLower().Replace(" ", "");
            type = getParType(str);
            if (m.Value.Length > 0)
            {   // string like "{result}" with the Brackets
                indx = m.Index + m.Value.Length;
                isBrackets = true;
                par = Regex.Replace(m.Value, @"\{.*?~|\{|\}", "");
                tx = str.Substring(0, m.Index);
            }
            else
            {   // result is part of str, recognised as a parameter value
                par = str.Substring(indx);
                indx = str.Length;
                isBrackets = false;
                tx = str;
            }
        }
        internal Parameter(string str, ParType _type)
        {
            type = _type;
            if (type == ParType.Integer) par = Lib.ToInt(str);
            if (type == ParType.Double) par = Lib.ToDouble(str);
            if (type == ParType.String) par = str;
        }
        #region ?????? for future ??????????
        ////////////////internal Parameter(string str, int n, FingerPrint.FingerPrint ruleFP)
        ////////////////{
        // 12/3/2017 ///    type = ParType.ANY;
        ////////////////    if (ruleFP == null) return;
        ////////////////    string template = (string)ruleFP.pars[0];

        ////////////////    //////////////// преобразование параметра - не дописано
        ////////////////    //11/3           internal Parameter(string str, ParType _type = ParType.String)

        ////////////////    //11/3            type = str.IndexOf('~') > 0 ? getParType(str) : _type;

        ////////////////    //11/3            if (type == ParType.Integer) par = Lib.ToInt((string)par);
        ////////////////    //11/3            if (type == ParType.Double) par = Lib.ToDouble((string)par);

        ////////////////    //////////////string s = (string)par;
        ////////////////    //////////////int ii = s.IndexOf('~');
        ////////////////    //////////////if (ii < 0) ii = 0;
        ////////////////    //////////////par = s.Substring(ii);
        ////////////////}

        ////////////////private string getPar(string str, ref int startIndex, out bool isBrackets)
        ////////////////{
        ////////////////    const string PARAMETR = @"\{.+?\}";
        ////////////////    Regex parametr = new Regex(PARAMETR, RegexOptions.IgnoreCase);
        ////////////////    Match m = parametr.Match(str, startIndex);
        ////////////////    string result;
        ////////////////    if (m.Value.Length > 0)
        ////////////////    {   // string like "{result}"
        ////////////////        startIndex = m.Index + m.Value.Length;
        ////////////////        isBrackets = true;
        ////////////////        result = Regex.Replace(m.Value, @"\{|\}", "");
        ////////////////    }
        ////////////////    else
        ////////////////    {   // result is part of str, recognised as a parameter value
        ////////////////        result = str.Substring(startIndex).Trim();
        ////////////////        startIndex = str.Length;
        ////////////////        isBrackets = false;
        ////////////////    }
        ////////////////    return result;
        ////////////////}
        #endregion ?????? for future ??????????
        internal enum ParType { String, Integer, Double, ANY }
        private ParType getParType(string str)
        {
            const string PAR_TYPE = @"\{(s|d|i).*?~";
            ParType result = ParType.String;
            Regex parType = new Regex(PAR_TYPE, RegexOptions.IgnoreCase);
            Match m = parType.Match(str);
            if (m.Value == "") return result;
            switch (m.Value[1])
            {
                case 's': break;
                case 'd': result = ParType.Double; break;
                case 'i': result = ParType.Integer; break;
            }
            return result;
        }
        #region ------ test Section -----
#if DEBUG
        internal static void testParameter()
        {
            Log.set("testParameter");
            Parameter p = new Parameter("{4}");
            test_getParType(p);
            p.test_Parametr();

 //           TST.Eq(Sections.Count, 8);
            Log.exit();
        }

        private void test_Parametr()
        {
            Log.set("test_Parametr(\"{4}\"");
            TST.Eq(type, "String");
            TST.Eq((string)par, "4");
            TST.Eq(isBrackets, true);

            Parameter p = new Parameter("Ab{123}cD");
            TST.Eq(type, "String");
            TST.Eq((string)p.par, "123");
            TST.Eq(p.tx, "ab");

            p = new Parameter("текст");
            TST.Eq(type, "String");
            TST.Eq(p.isBrackets, false);
            TST.Eq((string)p.par, p.tx);

            p = new Parameter("x{3");
            TST.Eq((string)p.par, "x{3");
            TST.Eq(p.isBrackets, false);

            p = new Parameter("def}fg");
            TST.Eq((string)p.par, "def}fg");
            TST.Eq(p.isBrackets, false);

            p = new Parameter("Da{34{85}uy");
            TST.Eq(p.isBrackets, true);
            TST.Eq(p.tx, "da");
            TST.Eq(p.par, "3485");  // поскольку внутреняя { стирается

            p = new Parameter("цена: {d~3}");
            TST.Eq(p.type, "Double");
            TST.Eq(p.isBrackets, true);
            TST.Eq((string)p.par, "3");

            //???????? 11/3

//12/3            p = new Parameter("abcd", null);
//12/3            TST.Eq(p.type, "ANY");

            FP ruleFP = new FP(FP.type.Rule, "M:B*");
//12/3            p = new Parameter("B12,5", ruleFP, );
            Log.exit();
        }

        private static void test_getParType(Parameter p)
        {
            Log.set("test_getParType()");
            TST.Eq(p.getParType("{2}").ToString(), "String");
            TST.Eq(p.getParType("{s~2}").ToString(), "String");
            TST.Eq(p.getParType("{i~4}").ToString(), "Integer");
            TST.Eq(p.getParType("{d~3}").ToString(), "Double");
            TST.Eq(p.getParType("{digital~3}").ToString(), "Double");
            TST.Eq(p.getParType("текст{i~1}b{d~2,2}ff"), "Integer");
            TST.Eq(p.getParType("другой текст"), "String");
            TST.Eq(p.getParType(""), "String");
            Log.exit();
        }

        //////private static void testFP_getPar(FingerPrint fp)
        //////{
        //////    Log.set("testFP_getPar()");

        //////    int ind = 0;
        //////    bool isBrackets;
        //////    // for StepIn //string s = fp.getPar("{123}", ref ind);
        //////    TST.Eq(fp.getPar("{123}", ref ind, out isBrackets), "123");
        //////    TST.Eq(ind, 5);
        //////    TST.Eq(isBrackets, true);
        //////    ind = 0;
        //////    string st = "параметр-значение";
        //////    TST.Eq(fp.getPar(st, ref ind, out isBrackets), st);
        //////    TST.Eq(ind, st.Length);
        //////    TST.Eq(isBrackets, false);

        //////    ind = 0;
        //////    TST.Eq(fp.getPar("", ref ind, out isBrackets), "");
        //////    TST.Eq(ind, 0);
        //////    TST.Eq(isBrackets, false);

        //////    TST.Eq(fp.getPar("{3", ref ind, out isBrackets), "{3");
        //////    TST.Eq(isBrackets, false);

        //////    ind = 0;
        //////    TST.Eq(fp.getPar("2}", ref ind, out isBrackets), "2}");
        //////    TST.Eq(ind, 2);
        //////    TST.Eq(isBrackets, false);

        //////    ind = 0;
        //////    string str = "tx1{32}tx2";
        //////    string par = fp.getPar(str, ref ind, out isBrackets);
        //////    TST.Eq(par, "32");
        //////    int indx = ind - par.Length - 2;
        //////    TST.Eq(ind, 7);
        //////    TST.Eq(indx, 3);
        //////    TST.Eq(isBrackets, true);
        //////    string tx1 = str.Substring(0, indx);
        //////    TST.Eq(tx1, "tx1");

        //////    str = "{323}tx2";
        //////    ind = 0;
        //////    TST.Eq(fp.getPar(str, ref ind, out isBrackets), "323");
        //////    string tx2 = str.Substring(ind);
        //////    TST.Eq(ind, 5);
        //////    TST.Eq(tx2, "tx2");
        //////    TST.Eq(isBrackets, true);

        //////    ind = 1;
        //////    TST.Eq(fp.getPar("ghj{323}", ref ind, out isBrackets), "323");
        //////    TST.Eq(ind, 8);
        //////    TST.Eq(isBrackets, true);

        //////    ind = 0;
        //////    str = "abc{1}ghj{2}";
        //////    TST.Eq(fp.getPar(str, ref ind, out isBrackets), "1");
        //////    TST.Eq(isBrackets, true);

        //////    int i1 = ind;
        //////    TST.Eq(fp.getPar(str, ref ind, out isBrackets), "2");
        //////    int indx2 = ind - 3 - i1;
        //////    string ghj = str.Substring(i1, indx2);
        //////    TST.Eq(ghj, "ghj");
        //////    TST.Eq(isBrackets, true);

        //////    TST.Eq(fp.getParType("{2}").ToString(), "String");
        //////    TST.Eq(fp.getParType("{s~2}").ToString(), "String");
        //////    TST.Eq(fp.getParType("{i~4}").ToString(), "Integer");
        //////    TST.Eq(fp.getParType("{d~3}").ToString(), "Double");
        //////    TST.Eq(fp.getParType("{digital~3}").ToString(), "Double");
        //////    Log.exit();
        //////}
#endif //#if DEBUG
        #endregion ------ test Section ------
    } // end class Parameter
} // end namespace TSmatch.Parametr
