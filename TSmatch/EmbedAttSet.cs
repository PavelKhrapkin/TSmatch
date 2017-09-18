/*--------------------------------------------------------------------------------------
 * EmbedAttSet -- Properties and Methods of Emberded Custom Parts
 * 
 *  15.09.2017  Pavel Khrapkin
 * 
 *----- History ------------------------------------------
 * 15.09.2019 - created from Excercize EmbedsQuotation
 */
using System.Collections.Generic;

namespace TSmatch.EmbedAttSet
{
    public class EmbedAttSet
    {
        public string mark, vendor;
        public int quantity;
        public double priceOne, priceTotal;
        public List<string> partGuids = new List<string>();

        internal EmbedAttSet() { }
    }
}
