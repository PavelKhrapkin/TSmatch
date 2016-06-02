/*--------------------------------------------------------------------------------------
 * ElmAttSet -- Definitions of Properties, and their Names of the Elements in the Model 
 * 
 *  2.6.2016  Pavel Khrapkin
 *  
 *----- History ------------------------------------------
 * 01.06.2016 PKh - created from structure AttSet in Tekla.Open_API module
 * -------------------------------------------
 * public class ElmAttSet - set of model component attribuyes, extracted from Tekla or IFC by method Read
 *
 */
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSmatch.ElmAttSet
{
    public class ElmAttSet : IComparable<ElmAttSet> , IEquatable<ElmAttSet>
    {
        public string guid = "";
        public string mat  = "";       
        public string mat_type = "";   
        public string prf  = "";       
        public double lng  = 0.0;      
        public double weight = 0.0;    
        public double volume = 0.0;    
        public double price  = 0.0;    

        public string[] TAG = { "GUID", "MATERIAL", "MATERIAL_TYPE", "PROFILE", "LENGTH", "WEIGHT", "VOLUME", "PRICE" };

        public static List<ElmAttSet> Elements = new List<ElmAttSet>();

        ElmAttSet(string _guid, string _mat, string _mat_type, string _prf
            , double _lng, double _weight, double _volume, double _price)
        {
            guid = _guid;
            mat  = _mat;
            mat_type = _mat_type;
            prf  = _prf;
            lng = _lng;
            weight = _weight;
            volume = _volume;
            price  = _price;
        }

        public bool Equals(ElmAttSet other)
        {
            return mat.Equals(other.mat) && prf.Equals(other.prf) && volume.Equals(other.volume);
        }
        
        public int CompareTo(ElmAttSet other)
        {
            int result = mat.CompareTo(other.mat);
            if (result == 0) result = prf.CompareTo(other.prf);
            if (result == 0) return -lng.CompareTo(other.lng);
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
                int hCode = (p.guid + p.mat + p.prf + p.lng.ToString()
                    + p.volume.ToString() + p.weight.ToString()).GetHashCode();
                return hCode.GetHashCode();
            }
        } // end ElmAttSetCompararer
    } // end class ElmAttSet
} // end namespace
