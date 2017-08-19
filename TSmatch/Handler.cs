/*---------------------------------------------------------------------------------
 * Handler -- Handle Model for Report preparation
 * 
 *  18.08.2017 Pavel Khrapkin
 *  
 *--- History ---
 *  8.05.2017 taken from Model code
 * 27.05.2017 getRules
 * 20.06.2017 getGroup re-make with LINQ
 * 28.06.2017 bug fix in PrfUpdate()
 *  3.07.2017 ProfileUpdate module add instead of PrfUdate in Handler
 * 21.07.2017 Audit GetGrps and Hndl
 * 23.07.2017 No heritage with Model we-engineering; rename ModHandler -> Handler
 *  4.08.2017 Handle() elmGroupc.Count and Rule.Count check; Rules Init
 * 16.08.2017 protected GetSavedRules used
 * 18.08.2017 fill elmGroup.compDescription when match found
 *--- Unit Tests --- 
 * 2017.08.4 UT_Handler.UT_Hndl OK -- 20,4 sec модель "Навес над трибунами" 7128 э-тов
 *           UT_Pricing OK
 * -------------------------------------------------------------------------------
 *      Methods:
 * getGroups()      - groupping of elements of Model by Material and Profile
 * Handler()                 
 */
using System;
using System.Collections.Generic;
using System.Linq;

using log4net;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using ElmGr = TSmatch.ElmAttSet.Group;
using Mod = TSmatch.Model.Model;
using SR = TSmatch.SaveReport.SavedReport;
using Mtch = TSmatch.Matcher.Mtch;

namespace TSmatch.Handler
{
    public class Handler
    {
        public static readonly ILog log = LogManager.GetLogger("Handler");

        _SR sr = new _SR();
        bool testMode;

        /// <summary>
        /// getGroups() - groupping of elements of Model by Material and Profile
        /// </summary>
        /// <history> 
        /// 2016.09.29 created
        /// 2017.05.8  перенес в модуль ModHandling, добавил аргумент elements
        /// 2017.06.27 переписано
        /// 2017.07.20 field bool errDialog add
        /// </history>
        public List<ElmGr> getGrps(List<Elm> elements, bool errDialog = true)
        {
            Log.set("getGrps(" + elements.Count + ")");
            if (elements == null || elements.Count == 0) Msg.F("getGrps: no elements");
            var gr = new ElmGr(errDialog);
            List<ElmGr> groups = new List<ElmGr>();
            var grps = elements.GroupBy(x => x.prf);
            foreach (var grp in grps) groups.Add(new ElmGr(grp));
            if (elements.Count != groups.Sum(x => x.guids.Count)) Msg.F("getGrps internal error");
            var v = new ProfileUpdate.ProfileUpdate(ref groups);
            Log.exit();
            return groups;
        }

        /// <summary>
        /// Hndl(model) - find matching Components for model and total_price
        /// </summary>
        /// <param name="mod">model to be handled</param>
        public void Hndl(ref Mod mod)
        {
            Log.set("MH.Hndl");
            mod.elmGroups = getGrps(mod.elements);
            if (mod.elmGroups.Count < 1 || mod.Rules.Count < 1) Msg.F("No Rules or element Groups");
            if (testMode) { Log.exit(); return; }

            foreach (var rules in mod.Rules)
                if (rules.CompSet == null) rules.Init();
            // find matching Components with Rules by module Mtch 
            foreach (var gr in mod.elmGroups)
            {
                bool b = false;
                foreach (var rule in mod.Rules)
                {
                    Mtch _match = new Mtch(gr, rule);
                    if (_match.ok == Mtch.OK.Match)
                    {
                        mod.matches.Add(_match);
                        gr.CompSetName = _match.rule.sCS;
                        gr.SupplierName = _match.rule.sSupl;
                        gr.compDescription = _match.component.Str(Section.Section.SType.Description);
                        b = true; break;
                    }
                }
                if (!b) log.Info("No Match Group. mat= " + gr.mat + "\tprf=" + gr.prf);
            }
            // calculate prices for matches      
            mod.total_price = mod.elmGroups.Sum(x => x.totalPrice);
            mod.pricingDate = DateTime.Now;
            mod.pricingMD5 = mod.get_pricingMD5(mod.elmGroups);
            Log.Trace("price date=\t" + mod.pricingDate + "\tMD5=" + mod.pricingMD5);
            log.Info("Model.Hndl set " + mod.matches.Count + " groups. Total price=" + mod.total_price + " rub");
            Log.exit();
        }

        public void Pricing(ref Mod m, bool unit_test_mode = false)
        {
            Log.set("mh.Pricing");
#if DEBUG
            testMode = unit_test_mode;
            var x = new Mtch(m);
#endif
            if (m.Rules == null || m.Rules.Count == 0)
            {
                m = sr._GetSavedRules(m);
            }
            log.Info(">m.MD5=" + m.MD5 + " =?= " + m.getMD5(m.elements));
            Hndl(ref m);
            log.Info(">m.MD5=" + m.MD5 + " =?= " + m.getMD5(m.elements));
            Log.Trace("      date=\t" + m.date + "\tMD5=" + m.MD5 + "\telements.Count=" + m.elements.Count);
            Log.Trace("price date=\t" + m.pricingDate + "\tMD5=" + m.pricingMD5 + "\ttotal price" + m.total_price);
            Log.exit();
        }
    } // end class Handler : Model
    class _SR : SR
    {
        internal Mod _GetSavedRules(Mod model)
        {
            return GetSavedRules(model, init: true);
        }
    } // end interface class _SR for access to SavedReport method
} // end namespace Model.Handler