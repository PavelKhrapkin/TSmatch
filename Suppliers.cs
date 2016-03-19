/*----------------------------------------------------------------------------
 * Suppliers - componets supplier organisations
 * 
 *  19.3.2016  Pavel Khrapkin
 *
 *--- JOURNAL ---
 * 
 * ---------------------------------------------------------------------------
 *      METHODS:
 *
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = match.Lib.Log;
using Component = TSmatch.Components.Component;

namespace TSmatch.Suppliers
{
    public class Supplier
    {
        private static List<Supplier> Suppliers = new List<Supplier>();

        public string name;
        public string City;
        public string Street;
        public string Country;
        public string telephone;
        public string contact;
        public List<Components.CompSet> CompSets = new List<Components.CompSet>();

        public void Start()
        {
            ////Component rr= null;
            ////Component.
        }
        /// <summary>
        /// getSupplier(string name) - get supplier data from the list of supplers by the name
        /// </summary>
        /// <param name="name">name of the supplier to find</param>
        /// <returns>found supplier of null</returns>
        /// <journal>19.3.2016</journal>
        internal static Supplier getSupplier(string name)
        {
            Log.set("getSupplier(" + name + ")");
            Supplier result = null;
            result = Suppliers.Find(x => x.name == name);
            ////////////foreach (var s in Suppliers)
            ////////////{
            ////////////    if (s.name != name) continue;
            ////////////    result = s;
            ////////////    break;
            ////////////}
//            throw new NotImplementedException();
            Log.exit();
            return result;
        }
    } // end class Supplier
} // end namespace Suppliers
