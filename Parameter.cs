/*----------------------------------------------------------------
 * Parameter -- class dealing a string like "tx{par}"
 *
 * 14.03.2017 Pavel Khrapkin
 *
 *--- History ---
 *  9.03.2017 made from FingerPrint code fragments
 * 14.03.2017 Unit Test implemented
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
//        public static readonly ILog log = LogManager.GetLogger("Parameter");

        internal ParType type;
        internal string tx;
        internal object par;
        private bool isBrackets = false;

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
    } // end class Parameter
    #region ------ test Section -----
#if DEBUG
    /// <summary>
    /// utp - unit test class for Parameter aim is to get public visibility
    ///       of internal class Parameter for Unit Testing
    /// </summary>
    public class utp
    {
        public string type;
        public string tx;
        public object par;

        public utp(string str)
        {
            var parameter = new Parameter(str);
            type = parameter.type.ToString();
            tx = parameter.tx;
            par = parameter.par;
        }
#endif //#if DEBUG
        #endregion ------ test Section ------
    }
} // end namespace TSmatch.Parametr