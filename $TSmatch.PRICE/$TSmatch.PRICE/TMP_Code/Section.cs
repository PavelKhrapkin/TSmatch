/*----------------------------------------------------------------
 * Section -- class dealing with the fragment of string related to
 *            some section - f.e. Material, or Price
 *
 * 30.03.2017 Pavel Khrapkin
 *
 *--- History ---
 *  7.03.2017 made from other module fragments
 * 19.03.2017 re-written with SectionTab as a argument of Constructor
 * 21.03.2017 call Bootstrap.initSection() for SectionTab
 * 28.03.2017 munli-header Section like "M: Def: body"
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
using Boot = TSmatch.Bootstrap.Bootstrap;

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
        public SType refSection; // reference to the Section with data

        Dictionary<string, List<string>> SectionTab;

        public Section(string _text, SType stype = SType.NOT_DEFINED)
        {
            if (SectionTab == null) SectionTab = new Boot.initSection().SectionTab;
            string[] sections = Lib.ToLat(_text).ToLower().Replace(" ", "").Split(';');
            if (stype == SType.NOT_DEFINED)
            {
                type = SecType(sections[0]);
                body = SecBody(sections[0]);
                refSection = SecRef(sections[0]);
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
    } // end class Section
} // end namespace TSmatch.Section
