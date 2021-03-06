﻿/*--------------------------------------------------------------------------------------
 * ElmAttSet -- Definitions of Properties, and their Names of the Elements in the Model 
 * 
 *  29.11.2017  Pavel Khrapkin
 * 
 * ----- TODO 30.9.2016, 20.07.2017, 18.08.2017 ------
 * - закомментировать неиспользуемые методы группировки (Ctrl/F12 empty)
 * - разобраться в MatTypeGroup: задействовать те же коды, что в Group, если не получится - совсем закомментировать
 * - заменить Dictionary Elements на поле в Model
 * - убрать все static
 * - выделить Mgroup и Group в отдельные классы
 *----- History ------------------------------------------
 * 01.06.2016 - created from structure AttSet in Tekla.Open_API module
 * 19.06.2016 - move Group and Mgroup classes from module Model
 *  2.08.2016 - adapt to IFC module
 * 16.08.2016 - LINQ Groupping methods - not implemented (!) почистить ElmAttSet
 * 22.08.2016 - методы Scale и SetScale
 * 30.09.2016 - clean up, Group class audited
 * 27.05.2017 - preparation to XML serialized save - parameterless constructors
 * 20.07.2017 - error message "different meterials in group"  supress, group cleanup
 * 18.08.2017 - add Group.compDescription field
 * 29.08.2017 - module Group separated
 * 15.09.2017 - audit & cleanup
 * 20.11.2017 - removed field ru_prf
 * 29.11.2017 - Msg adoption
 * -------------------------------------------
 * public class ElmAttSet - set of model component attribuyes, extracted from Tekla or IFC by method Read
 * public class Mgroup    - group elements by Materials
 */
using log4net;
using System;
using System.Collections.Generic;
using CmpSet = TSmatch.CompSet.CompSet;
using Ifc = TSmatch.IFC.IfcManager.Core.IfcManager.IfcElement;
using Lib = match.Lib.MatchLib;
using Supl = TSmatch.Suppliers.Supplier;

namespace TSmatch.ElmAttSet
{
    public class ElmAttSet : IComparable<ElmAttSet>, IEquatable<ElmAttSet>
    {
        public static readonly ILog log = LogManager.GetLogger("ElmAttSet");

        public string guid = "";
        public string mat = "";
        public string mat_type = "";
        public string prf = "";
        public double length = 0.0;
        public double weight = 0.0;
        public double volume = 0.0;
        public double price = 0.0;

        public static Dictionary<string, ElmAttSet> Elements = new Dictionary<string, ElmAttSet>();

        public ElmAttSet() { }

        public ElmAttSet(string _guid, string _mat, string _mat_type, string _prf
            , double _lng = 0.0, double _weight = 0.0, double _volume = 0.0
            , string _ru_prf = "", double _price = 0.0)
        {
            guid = _guid;
            mat = _mat;
            mat_type = _mat_type;
            prf = _prf;
            length = _lng;
            weight = _weight;
            volume = _volume;
            price = _price;
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

    #region MaterialTypeGroup, MGroup, Group
    /// <summary>
    /// Mgroup - Group Elements by Materials
    /// </summary>
    public class Mgroup : IComparable<Mgroup>
    {
        Message.Message Msg = new Message.Message();
        public readonly string mat;
        public readonly List<string> guids;
        public readonly double totalWeight;
        public readonly double totalVolume;
        public double totalPrice;
        //---- references to other classes - price-list conteiners
        public CmpSet CompSet;     //список компонентов, с которыми работает правило
        public Supl Supplier;      //Поставщик

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
    } // end class Mgroup
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