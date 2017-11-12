/*--------------------------------------------------------------------------------------
 * Group -- element group class - creation and some group handiling code 
 * 
 *  31.08.2017  Pavel Khrapkin
 * 
 *--- Unit Tests ---
 * UT_Group: UT_CheckGroup 31.8.2017 OK
 *----- History ------------------------------------------
 * 29.08.2017 - created from ElmAttSet module
 * 31.08.2017 - add field GrType and fill it im CheckGroup()
 * -------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;

using log4net;
using Msg = TSmatch.Message.Message;
using Lib = match.Lib.MatchLib;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using TSmatch.ElmAttSet;

namespace TSmatch.Group
{
    public class Group : IComparable<Group>
    {
        public static readonly ILog log = LogManager.GetLogger("Group");

        public enum GrType { UsualPrice, SpecPrice, NoPrice, Warning }
        public GrType type = GrType.UsualPrice;
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

        private Dictionary<string, Elm> elmsDic = new Dictionary<string, Elm>();

        public Group() {}

        public Group(IGrouping<string, Elm> group)
        {
            elmsDic = group.ToDictionary(x => x.guid);
            Mat = elmsDic.First().Value.mat;
            Prf = elmsDic.First().Value.prf;
            mat = Lib.ToLat(Mat.ToLower().Replace("*", "x"));
            prf = Lib.ToLat(Prf.ToLower().Replace("*", "x"));
            guids = group.Select(x => x.guid).ToList();
            totalLength = group.Select(x => x.length).Sum();
            totalWeight = group.Select(x => x.weight).Sum();
            totalVolume = group.Select(x => x.volume).Sum();
         }

        /// <summary>
        /// CheckGroups - check all groups of elements, put GrType.Warning if group is incorrect
        /// </summary>
        /// <param name="groups"></param>
        public void CheckGroups(ref Model.Model mod)
        {
//12/11            if (mod == null || mod.elements == null || mod.elements.Count < 1
  //12/11              || mod.elmGroups == null || mod.elmGroups.Count < 1
//12/11                || mod.elements.Count != mod.elmGroups.Sum(x => x.guids.Count)) Msg.F("ChechGroup: bad model", mod.name);
//12/11            var _dic = mod.elements.ToDictionary(x => x.guid);
            foreach(var gr in mod.elmGroups)
            {
//12/11                elmsDic = _dic.Where(x => gr.guids.Contains(x.Value.guid)).ToDictionary(v => v.Key, v => v.Value);
                Mat = elmsDic.First().Value.mat;
                int grIndex = mod.elmGroups.IndexOf(gr);
                bool errFlag = false;
                foreach (var elm in elmsDic)
                {
                    if (elm.Value.mat == Mat) continue;
                    mod.elmGroups[grIndex].type = GrType.Warning;
                    mod.HighLightElements(elmsDic);
                    if (errFlag) continue;
 //12/11                   Msg.W("CheckGroups: various materials in Group", grIndex, gr.Prf, Mat, elm.Value.mat);
                    errFlag = true;
                }
            }
        }

        public int CompareTo(Group gr)     //to Sort Groups by Materials
        {
            int x = mat.CompareTo(gr.mat);
            if (x == 0) x = prf.CompareTo(gr.prf);
            return x;
        }

        public void Method()
        {
            throw new System.NotImplementedException();
        }
    } // end class Group
} // end namespace 
