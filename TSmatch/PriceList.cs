/*----------------------------------------------------------------------------
 * PriceList - class for Price List checking in data base
 * 
 *  24.5.2017  Pavel Khrapkin
 *
 *--- History ---
 * ---------------------------------------------------------------------------
 *      METHODS:
 * CheckAll()   - check all price-list Listed in TSmatch.xlsx/Supplier are available
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

using Msg = TSmatch.Message.Message;
using Docs = TSmatch.Document.Document;
using Decl = TSmatch.Declaration.Declaration;
using Log = match.Lib.Log;
using Supl = TSmatch.Suppliers.Supplier;
using CS = TSmatch.CompSet.CompSet;

namespace TSmatch.PriceList
{
    public class PriceList
    {
        public static readonly ILog log = LogManager.GetLogger("PriceList");

        public void CheckAll()
        {
            Docs toc = Docs.getDoc();

            //-- Check TSmach.xlsx/Suppliers consystancy
            Docs SupList = Docs.getDoc(Decl.SUPPLIERS);
            for(int i = SupList.i0; i <= SupList.il; i++ )
            {
                Supl s = new Supl(i);
                int nCompSets = SupList.Body.Int(i, Decl.SUPL_LISTCOUNT);
                for (int n = 0; n < nCompSets; n++)
                {
                    string csName = SupList.Body.Strng(i, Decl.SUPL_LISTCOUNT + n + 1);
                    CS cs = null;
                    try { cs = new CS(csName, s); cs.doc.Close(); }
                    catch { }
                    var ruleLst = Rule.Rule.getList(s.name);
                    string msg = string.Format("PriceList \"{0}\" "
                        + " not awalable from Supplier \"{1}\"", csName, s.name);
                    if (ruleLst.Count == 0) msg += ", and no Rule for him";
                    else msg += string.Format(", however, available {0} for him", ruleLst.Count);
                    log.Info(msg);
                }   
            }

            //-- check toc
            for(int i = toc.i0; i <= toc.il; i++)
            {

            }

        }
    }
} // end namespace PriceList
