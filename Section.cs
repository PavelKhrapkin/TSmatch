/*----------------------------------------------------------------
 * Section -- class dealing with the fragment of string related to
 *            some section - f.e. Material, or Price
 *
 * 21.03.2017 Pavel Khrapkin
 *
 *--- History ---
 *  7.03.2017 made from other module fragments
 * 19.03.2017 re-written with SectionTab as a argument of Constructor
 * 21.03.2017 call Bootstrap.initSection() for SectionTab
 * ------ Fields ------
 * type section - recognized enum Section type, f.e. Material, Price etc
 * string body  - text string, contained in Section between ':' and ';' or end
 * --- Constructors ---  
 * public Section(string _text [, SType stype]) - if(SectionTab==null) initSection
 * ----- Methods: -----
 * isSectionMatch(string template) - check if this Section is in match with template
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;

namespace TSmatch.Section
{
    public class Section
    {
        public static readonly ILog log = LogManager.GetLogger("Section");

        public enum SType
        {
            NOT_DEFINED,
            Material, Profile, Price, Description,
            LengthPerUnit, VolPerUnit, WeightPerUnit, Unit
        }

        public SType type;      // recognysed type of Section
        public string body;     // text after ':', but before first ';'

        Dictionary<string, List<string>> SectionTab;

        public Section(string _text, SType stype = SType.NOT_DEFINED)
        {
            if (SectionTab == null) { SectionTab = new Bootstrap.Bootstrap.initSection().SectionTab; }
            string[] sections = Lib.ToLat(_text).ToLower().Replace(" ", "").Split(';');
            if (stype == SType.NOT_DEFINED)
            {
                type = SecType(sections[0]);
                body = SecBody(sections[0]);
                return;
            }
            foreach (string str in sections)
            {
                if (SecType(str) != stype) continue;
                type = stype;
                body = SecBody(str);
                return;
            }
            type = SType.NOT_DEFINED;
            body = string.Empty;
        }

        SType SecType(string text)
        {
            if (string.IsNullOrEmpty(text)) return SType.NOT_DEFINED;
            if (text.Contains(':')) text = text.Substring(0, text.IndexOf(':'));
            foreach(SType sec in Enum.GetValues(typeof(SType)))
            {
                if (sec == SType.NOT_DEFINED) continue;
                List<string> synonyms = SectionTab[sec.ToString()].ToList();
                foreach (string syn in synonyms)
                {
                    if (Regex.IsMatch(text, syn + ".*?")) return sec;
                }
            }
            return SType.NOT_DEFINED;
        }

        string SecBody(string str)
        {
            int ind = str.IndexOf(':') + 1;
            if (ind == 0) ind = str.Length;
            return str.Substring(ind);
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

        //////public bool isSectionInStr(string str)
        //////{
        //////    foreach (SType sec in Enum.GetValues(typeof(SType)))
        //////    {
        //////        Section s = new Section(str);
        //////        if
        //////    }
        //////        return ok;
        //////}
    } // end class Section
} // end namespace TSmatch.Section
