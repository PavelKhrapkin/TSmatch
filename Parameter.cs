/*----------------------------------------------------------------
 * Parameter -- class dealing a string like "tx{par}"
 *
 * 21.03.2017 Pavel Khrapkin
 *
 *--- History ---
 *  9.03.2017 made from FingerPrint code fragments
 * 14.03.2017 Unit Test implemented
 * 20.03.2017 public constructor and fields inseat of internal
 * 21.03.2017 par stored str after ':' till ';'
 * ------ Fields ------
 * tx - text before {par}
 * par - part of input string, recognized with 
 *      par could has a prefix like ~s:
 *          ~s or by default - string
 *          ~i - int
 *          ~d - double
 * type {string, int, double, ANY}
 *          ANY that any value of Parameter could be in match in Mtch
 *      type is enum Type {string, int, double}; string by default
 !! List<string> synonyms - possible alternative tx equivalents
 * --- Constructor ---  
 * public Parameter(string str) - fill Parameter fields from string str
 * public Parameter(string str, ParType _type) - convert str to _type
 * ----- Methods: -----
 */

using System.Text.RegularExpressions;
using log4net;
using Lib = match.Lib.MatchLib;

namespace TSmatch.Parameter
{
    public class Parameter
    {
        public static readonly ILog log = LogManager.GetLogger("Parameter");

        public ParType ptype;
        public string tx;
        public object par;

        public Parameter(string str)
        {
            str = Lib.ToLat(str).ToLower().Replace(" ", "");
            int indx = str.IndexOf(':') + 1;
            Regex parametr = new Regex(@"\{.+?\}");
            Match m = parametr.Match(str, indx);
            ptype = getParType(str);
            if (m.Value.Length > 0)
            {   // string like "{result}" with the Brackets
                par = Regex.Replace(m.Value, @"\{.*?~|\{|\}", "");
                tx = str.Substring(indx, m.Index - indx);
            }
            else
            {   // result is part of str, recognised as a parameter value
                int end = str.IndexOf(';');
                if (end < indx) end = str.Length;
                tx = str.Substring(indx, end - indx);
                par = tx;
            }
        }
        public Parameter(string str, ParType _type) : this(str)
        {
            ptype = _type;
            if (ptype == ParType.Integer) par = Lib.ToInt((string)par);
            if (ptype == ParType.Double) par = Lib.ToDouble((string)par);
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
        public enum ParType { String, Integer, Double, ANY }
        public ParType getParType(string str)
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
} // end namespace TSmatch.Parametr