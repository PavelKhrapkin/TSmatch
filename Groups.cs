/*------------------------------------------------------------------------------------------
 * Groups -- variaty of Element of Model sets, groupped by Material, Profile, Suppliers etc 
 * 
 *  18.8.2016  Pavel Khrapkin
 *  
 *----- History ------------------------------------------
 * 18.08.2016 - extracted from ElmAttSet module
 * -------------------------------------------
 * public class ElmSet  - group of elements from ElmAttSet
 * public class materialGroup   - group of elements with the same Material
 * public class matTypeGroup    - group of elements with the same Type of Materials (Concrete, Steel etc)
 * publis class matProfGroup    - group of elements with the same Material and Profile
 * public class supplierGroup   - group of elements supplieed by the same Supplier
 */
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

using Element = TSmatch.ElmAttSet.ElmAttSet;

namespace TSmatch.Groups
{
/*    public class ElmSet
    {
    }
    public class materialGroup : IComparable<materialGroup>
    {
        public static readonly ILog log = LogManager.GetLogger("Groups.materialGroup");

        public readonly string material;
        public readonly List<string> guids;
        public readonly double totalWeight;
        public readonly double totalVolume;
        public readonly double totalPrice;

        public materialGroup(List<Element> elements, string material, List<string> guids)
        {
            this.material = material;
            this.guids = guids;
            totalWeight = ElmAttSet.SumAtt(ElmAttSet.sumFields.weight, guids);
            totalVolume = ElmAttSet.SumAtt(ElmAttSet.sumFields.volume, guids);
            totalPrice = ElmAttSet.SumAtt(ElmAttSet.sumFields.price, guids);
        }
        public int CompareTo(materialGroup matGr)     //to Sort Groups by Materials
        {
            return material.CompareTo(matGr.material);
        }
    } */
} // end namespace Groups
