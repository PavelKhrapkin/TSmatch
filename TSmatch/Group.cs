/*--------------------------------------------------------------------------------------
 * Group -- element group class - creation and some group handiling code 
 * 
 *  29.08.2017  Pavel Khrapkin
 * 
 *  *--- Unit Tests ---
 * UT_ 18.8.2017 OK
 *----- History ------------------------------------------
 * 29.08.2017 - created from ElmAttSet module
 * -------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;

using log4net;
using Msg = TSmatch.Message.Message;
using Lib = match.Lib.MatchLib;
using Elm = TSmatch.ElmAttSet.ElmAttSet;

namespace TSmatch.Group
{
    public class Group : IComparable<Group>
    {
        public static readonly ILog log = LogManager.GetLogger("Group");

        public string mat;
        public string prf;
        public string Mat;
        public string Prf;
        public List<string> guids;
        public double totalLength;
        public double totalWeight;
        public double totalVolume;
        public double totalPrice;
        
        //---- references to other classes - price-list conteiners
        public string CompSetName;  //список компонентов, с которыми работает правило
        public string SupplierName; //Поставщик
        public string compDescription;  //Description of supplied Component, when found

        public Matcher.Mtch match;  // reference to the matched Supplier, rule and CompSet

        private Dictionary<string, Elm> Elements = new Dictionary<string, Elm>();

        public Group() {}

        public Group(IGrouping<string, Elm> group)
        {
            Elements = group.ToDictionary(x => x.guid);
            Mat = Elements.First().Value.mat;
            Prf = Elements.First().Value.prf;
            mat = Lib.ToLat(Mat.ToLower().Replace("*", "x"));
            prf = Lib.ToLat(Prf.ToLower().Replace("*", "x"));
            guids = group.Select(x => x.guid).ToList();
            totalLength = group.Select(x => x.length).Sum();
            totalWeight = group.Select(x => x.weight).Sum();
            totalVolume = group.Select(x => x.volume).Sum();
            //check Materials in group -- they should be the same
            foreach (var gr in group)
            {
                if (gr.mat == Mat) continue;
                var mod = new Model.Model();
                mod.HighLightElements(Elements);
                Msg.W("ElmGr: various materials in Group", Prf, Mat, gr.mat);
            }
        }

        public int CompareTo(Group gr)     //to Sort Groups by Materials
        {
            int x = mat.CompareTo(gr.mat);
            if (x == 0) x = prf.CompareTo(gr.prf);
            return x;
        }
    } // end class Group
} // end namespace 
