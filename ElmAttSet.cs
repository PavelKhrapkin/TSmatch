/*--------------------------------------------------------------------------------------
 * ElmAttSet -- Definitions of Properties, and their Names of the Elements in the Model 
 * 
 *  30.9.2016  Pavel Khrapkin
 * 
 * ----- TODO 30.9.2016 ------
 * - закомментировать неиспользуемые методы группировки (Ctrl/F12 empty)
 * - разобраться в MatTypeGroup: задействовать те же коды, что в Group, если не получится - совсем закомментировать
 * - заменить Dictionary Elements на поле в Model
 * - убрать все static
 *----- History ------------------------------------------
 * 01.06.2016 - created from structure AttSet in Tekla.Open_API module
 * 19.06.2016 - move Group and Mgroup classes from module Model
 *  2.08.2016 - adapt to IFC module
 * 16.08.2016 - LINQ Groupping methods - not implemented (!) почистить ElmAttSet
 * 22.08.2016 - методы Scale и SetScale
 * 30.09.2016 - clean up, Group class audited
 * -------------------------------------------
 * public class ElmAttSet - set of model component attribuyes, extracted from Tekla or IFC by method Read
 * public class Group     - Group elements by Materials and Profile
 * public class Mgroup    - group elements by Materials
 */
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

using Decl = TSmatch.Declaration.Declaration;
using Msg = TSmatch.Message.Message;
using Log = match.Lib.Log;
using Lib = match.Lib.MatchLib;
using Ifc = TSmatch.IFC.IfcManager.Core.IfcManager.IfcElement;
using Mtch = TSmatch.Matcher.Matcher;


namespace TSmatch.ElmAttSet
{
    public class ElmAttSet : IComparable<ElmAttSet> , IEquatable<ElmAttSet>
    {
        public static readonly ILog log = LogManager.GetLogger("ElmAttSet");

        public string guid = "";
        public string mat  = "";       
        public string mat_type = "";   
        public string prf  = "";       
        public double length = 0.0;      
        public double weight = 0.0;    
        public double volume = 0.0;    
        public double price  = 0.0;

        public static Dictionary<string, ElmAttSet> Elements = new Dictionary<string, ElmAttSet>();

        //public string[] TAG = { "GUID", "MATERIAL", "MATERIAL_TYPE", "PROFILE", "LENGTH", "WEIGHT", "VOLUME", "PRICE" };
        //public enum sumFields { length, weight, volume, price }

        public ElmAttSet(string _guid, string _mat, string _mat_type, string _prf
            , double _lng, double _weight, double _volume, double _price)
        {
            guid = _guid;
            mat  = _mat;
            mat_type = _mat_type;
            prf  = _prf;
            length = _lng;
            weight = _weight;
            volume = _volume;
            price  = _price;

            Elements.Add(_guid, this);
        }
        public ElmAttSet(Ifc ifc_elm)
        {
            guid = ifc_elm.guid;
            mat = ifc_elm.material;
            mat_type = ifc_elm.type_material;
            prf = ifc_elm.profile;
            length = Lib.ToDouble(ifc_elm.length);
            weight = Lib.ToDouble(ifc_elm.weight);
            volume = Lib.ToDouble(ifc_elm.volume);
            price = Lib.ToDouble(ifc_elm.price);

            Elements.Add(guid, this);
        }
        public bool Equals(ElmAttSet other)
        {
            return mat.Equals(other.mat) && prf.Equals(other.prf) && volume.Equals(other.volume);
        }       
        public int CompareTo(ElmAttSet other)
        {
            int result = mat.CompareTo(other.mat);
            if (result == 0) result = prf.CompareTo(other.prf);
            if (result == 0) return -length.CompareTo(other.length);
            return result;
        }
        /// <summary>
        /// ElementsMD5 -- calculate hash code MD5 for the list of elements of the model
        /// </summary>
        /// <returns></returns>
        /// <remarks>It could take few minutes or more for the large model</remarks>
        /// <history>21/6/2016 moved here from TS_OpenAPI
        /// </history>
        public static string ElementsMD5()
        {
            //            DateTime t0 = DateTime.Now;  
            string str = "";
            foreach (var elm in Elements.Values) str += elm.mat + elm.prf + elm.length.ToString();
            string ModelMD5 = Lib.ComputeMD5(str);
            return ModelMD5;
            //            new Log("MD5 time = " + (DateTime.Now - t0).ToString());
        }
        public class ElmAttSetCompararer : IEqualityComparer<ElmAttSet>
        {
            public bool Equals(ElmAttSet p1, ElmAttSet p2)
            {
                return p1.Equals(p2);
            }
            public int GetHashCode(ElmAttSet p)
            {
                int hCode = (p.guid + p.mat + p.prf + p.length.ToString()
                    + p.volume.ToString() + p.weight.ToString()).GetHashCode();
                return hCode.GetHashCode();
            }
        } // end ElmAttSetCompararer
    } // end class ElmAttSet
/*
    #region Groups
    public class Groups
    {
        public class materialType //: IComparable<materialType>
        {
            public readonly string matTypeGr;
            public readonly List<string> guids;
            public readonly double totalWeight;
            public readonly double totalVolume;
            public readonly double totalPrice;

            public materialType(string materialType)
            {
                this.matTypeGr = materialType;
                List<string> guids = new List<string>();
 ///               List<ElmAttSet> mtgrElms = from elm in this by elm.
            }
//            var mgroups = from elm in elements group elm by elm.mat;
        } // end class Groups.materialType

        public class Material : IComparable<Material>
        {

        } // end class Groups.Material

        public class MatPrf : IComparable<MatPrf>
        {

        } // end class Groups.MatPrf

        public class Supplier : IComparable<Supplier>
        {

        } // end class Groups.Supplier

    } // end class Groups
    #endregion Groups
*/
    #region MaterialTypeGroup, MGroup, Group

    ///// <summary>
    ///// Group Elemets by Material Types
    ///// </summary>
    //public class MaterialTypeGroup : IComparable<MaterialTypeGroup>
    //{
    //    public string matTypeGr;
    //    public List<string> guids;
    //    public double totalWeight;
    //    public double totalVolume;
    //    public double totalPrice;

    //    public static List<MaterialTypeGroup> MatTypeGroups = new List<MaterialTypeGroup>();
    //    public static double[] MatTypeGrSummary = new double[6];

    //    public MaterialTypeGroup(string matTypeGr, List<string> guids)
    //    {
    //        if (guids.Count == 0) return;
    //        this.matTypeGr = matTypeGr;
    //        this.guids = guids;
    //        totalWeight = ElmAttSet.SumAtt(ElmAttSet.sumFields.weight, guids);
    //        totalVolume = ElmAttSet.SumAtt(ElmAttSet.sumFields.volume, guids);
    //        totalPrice  = ElmAttSet.SumAtt(ElmAttSet.sumFields.price, guids);
    //        MatTypeGroups.Add(this);
    //    }
    //    public int CompareTo(MaterialTypeGroup mtg)     //to Sort Groups by Material Types
    //    {
    //        return matTypeGr.CompareTo(mtg.matTypeGr);
    //    }
    //    public static List<MaterialTypeGroup> setMaterialTypeGroups()
    //    {
    //        string curMTG = "";
    //        List<string> curGuids = new List<string>();
    //        foreach (var elm in ElmAttSet.Elements.Values)
    //        {
    //            if (elm.mat_type == curMTG) curGuids.Add(elm.guid);
    //            else
    //            {
    //                new MaterialTypeGroup(curMTG, curGuids);
    //                curMTG = elm.mat_type;
    //                curGuids.Clear();
    //                curGuids.Add(elm.guid);
    //            }
    //        }
    //        new MaterialTypeGroup(curMTG, curGuids);
    //        return MatTypeGroups;
    //    }
    //    /// <summary>
    //    /// getMatTypeGrSummay() - get in MatTypeGrSummary results by Material Type Group
    //    ///   !!  this is hardcode defined for CONCRETE and STEEL groups only !!
    //    /// </summary>
    //    public static void getMatTypeGrSummary()
    //    {
    //        Log.set("getMatTypeGrSummary");
    //        setMaterialTypeGroups();
    //        foreach(var v in MatTypeGroups)
    //        {
    //            if (v.matTypeGr.ToLower().Contains("concrete"))
    //            {       // CONCRETE //
    //                MatTypeGrSummary[0] = v.totalVolume/1000/1000/1000;
    //                MatTypeGrSummary[2] = v.totalWeight;
    //                MatTypeGrSummary[4] = v.totalPrice;
    //            }
    //            else
    //            {       // STEEL //
    //                MatTypeGrSummary[1] = v.totalVolume/1000/1000/1000;
    //                MatTypeGrSummary[3] = v.totalWeight;
    //                MatTypeGrSummary[5] = v.totalPrice;
    //            }
    //        }
    //        Log.exit();
    //    }
    //} // end class MaterialTypeGroup

    /// <summary>
    /// Mgroup - Group Elements by Materials
    /// </summary>
    public class Mgroup : IComparable<Mgroup>
    {
        public readonly string mat;
        public readonly List<string> guids;
        public readonly double totalWeight;
        public readonly double totalVolume;
        public readonly double totalPrice;

        public Mgroup(Dictionary<string, ElmAttSet> Els, string material, List<string> guids)
        {
            this.mat = material;
            this.guids = guids;
            foreach (var id in guids)
            {
                totalVolume += Els[id].volume;
                totalWeight += Els[id].weight;
                totalPrice += Els[id].price;
            }
        }
        public Mgroup(List<ElmAttSet> elements, string material, List<string> guids)
        {
            this.mat = material;
            this.guids = guids;
            totalWeight = totalVolume = totalPrice = 0.0;
            foreach (string id in guids)
            {
                ElmAttSet elm = elements.Find(x => x.guid == id);
                if (elm == null) Msg.F("ElmAttSet: Mgroup(wrong guid)", id);
                totalWeight += elm.weight;
                totalVolume += elm.volume;
                totalPrice += elm.price;
            }
        }
        public int CompareTo(Mgroup mgr)     //to Sort Groups by Materials
        {
            return mat.CompareTo(mgr.mat);
        }

        ////internal class Build : Mgroup
        ////{
        ////    private List<ElmAttSet> elements;

        ////    public Build(List<ElmAttSet> elements)
        ////    {
        ////        this.elements = elements;
        ////    }
        ////}
    } // end class Mgroup

    public class Group : IComparable<Group>
    {
        public string mat;
        public string prf;
        public List<string> guids;
        public double totalLength;
        public double totalWeight;
        public double totalVolume;
        public double totalPrice;
        public Mtch.OK matchRef;        // Reference to the matched supply source (i.e.line in price list)

        public Group(Dictionary<string, ElmAttSet>Els, string _mat, string _prf, List<string> _guids)
        {
            mat = Lib.ToLat(_mat);
            prf = Lib.ToLat(_prf);
            guids = _guids;
            totalLength = totalWeight = totalVolume = totalPrice = 0.0;
            foreach(var id in guids)
            {
                totalLength += Els[id].length;
                totalVolume += Els[id].volume;
                totalWeight += Els[id].weight;
                totalPrice  += Els[id].price;
            }
        }
        public int CompareTo(Group gr)     //to Sort Groups by Materials
        {
            int x = mat.CompareTo(gr.mat);
            if (x == 0) x = prf.CompareTo(gr.prf);
            return x;
        }
    } // end class Group
    #endregion
    /* 21/6/2016
        public class Group : IComparable<Group>
        {
            public static List<Group> Groups = new List<Group>();

            ////public string mat, mat_type, prf;
            ////public double lng, wgt, vol;
            public readonly List<string> GUIDs; // List of ID Parts in the Group

            public Group(string _mat, string _mat_type, string _prf,
                         double _lng, double _wgt, double _vol,
                         List<string> _guids)
            {
                this.  .ElmAttSet.mat = Lib.ToLat(_mat);
                mat_type = _mat_type;
                prf = Lib.ToLat(_prf);
                lng = _lng;
                wgt = _wgt;
                vol = _vol;
                GUIDs = _guids;
            }
            public int CompareTo(Group grp)     //to Sort Groups by Material and Profile
            {
                int x = this.mat.CompareTo(grp.mat);
                if (x == 0) x = this.prf.CompareTo(grp.prf);
                return x;
            }


            //public static void lngGroup(dynamic atr)
            //{
            //    Log.set("lngGroup");
            //    if (atr.GetType() != typeof(List<TS.AttSet>)) Log.FATAL("ПОКА Я УМЕЮ РАБОТАТЬ ТОЛЬКО С TSread, но вскоре...");
            //    List<TS.AttSet> Elements = atr;
            //    Elements.Sort();
            //    foreach (var elm in Elements)
            //    {
            //        Group grp = new Group(elm.mat, elm.prf);
            //    }
            //    Log.exit();
            //}
        } // end class Group
        /// <summary>
        /// Mgroup - return Elements groupped by field Material, i.e. in the list of Elements 
        ///          the elements with the same Material value get combined: numberic fields - summarised,
        ///          and their GUIDS add to the list.
        ///          Full list of Mgroups stores in List<Mgroup> Mgroups
        /// </summary>
        /// <example> Mgroup Elements.Mroup </example>
        public class Mgroup : IComparable<Mgroup>
        {
            static List<Mgroup> Mgroups = new List<Mgroup>();

            String mat;
            double volume, weight;
            List<Group> groups = new List<Group>();

            public Mgroup(string mat, double vol, double wgt, List<Group> grps)
            {
                this.mat = mat;
                this.volume = vol;
                this.weight = wgt;
            }
            public int CompareTo(Mgroup mgr) { return mgr.mat.CompareTo(mgr); }    //to Sort Mgroups by Material

            internal static void setMgr()
            {
                Log.set("setMgr");
                Mgroups.Clear();
                Groups.Sort();
                string mat = "";
                double vol = 0, wgt = 0;
                List<Group> grps = new List<Group>();
                foreach (var g in Groups)
                {
                    if (mat == g.mat)
                    {
                        grps.Add(g);
                        vol += g.vol;
                        wgt += g.wgt;
                    }
                    else
                    {
                        if (mat != "") Mgroups.Add(new Mgroup(mat, vol, wgt, grps));
                        mat = g.mat; vol = 0; wgt = 0;
                        grps = new List<Group>();
                    }
                }
                if (vol > 0) Mgroups.Add(new Mgroup(mat, vol, wgt, grps));
                Log.exit();
            }
        } // end class Mgroup
    */ // 21/6/2016 отладить позже
} // end namespace
