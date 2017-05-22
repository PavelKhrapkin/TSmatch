/*--------------------------------------------------------------------------------------------
 * ModHandler : Model -- Handle Model for Report preparation
 * 
 *  17.05.2017 Pavel Khrapkin
 *  
 *--- History ---
 *  8.05.2017 get from Model code
 *--- Unit Tests --- 
 * -------------------------------------------------------------------------------------------
 *      Methods:
 * getGroups()      - groupping of elements of Model by Material and Profile
 * Handler()                 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using Decl = TSmatch.Declaration.Declaration;
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Docs = TSmatch.Document.Document;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using ElmGr = TSmatch.ElmAttSet.Group;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Mtch;

namespace TSmatch.Model.Handler
{
    public class ModHandler : Mod
    {
        public static readonly ILog log = LogManager.GetLogger("Model.Handler");

        public void Handler(Mod mod)
        {
            Log.set("MH.Handler(\"" + mod.name + "\")");
            getGroups(mod.elements);
            log.Info("- total elements = " + mod.elements.Count + " in " + mod.elmGroups.Count + "groups");
            foreach (var gr in mod.elmGroups)
            {
                bool b = false;
                foreach (var rule in mod.Rules)
                {
                    Mtch _match = new Mtch(gr, rule);
                    if (_match.ok == Mtch.OK.Match) { mod.matches.Add(_match); b = true; break; }
                }
                if (!b) log.Info("No Match Group. mat= " + gr.mat + "\tprf=" + gr.prf);
            }
            int cnt = 0;
            var elms = new Dictionary<string, Elm>();
            elms = mod.elements.ToDictionary(elm => elm.guid);
            foreach (var match in mod.matches)
            {
                double price_per_t = match.group.totalPrice / match.group.totalVolume;
                foreach (var guid in match.group.guids)
                {
                    elms[guid].price = price_per_t * elms[guid].volume;
                    cnt++;
                }
            }

            log.Info("- found " + mod.matches.Count + " price matches for " + cnt + " elements");
            elements = elms.Values.ToList();

            Log.Trace("<MH>Rules.Count=", mod.Rules.Count);
            Log.Trace("<MH>Price match for ", mod.matches.Count, " / ", mod.elmGroups.Count);
            Log.exit();
        }

        public void Pricing(ref Mod m)
        {
            getRules(m);
            Handler(m);
        }

        public List<Elm> getPricingFrGroups()
        {
            Log.set("Models.getPricing()");
            var elms = new Dictionary<string, Elm>();
            foreach (var elm in elements) elms.Add(elm.guid, elm);
            foreach (var gr in elmGroups)
            {
                double price_per_t = gr.totalPrice / gr.totalVolume;
                foreach (string guid in gr.guids)
                    elms[guid].price = price_per_t * elms[guid].volume;
            }
            foreach (var mgr in elmMgroups)
            {
                foreach (string guid in mgr.guids)
                    mgr.totalPrice += elms[guid].price;
            }
            Log.exit();
            return elms.Values.ToList();
        }

        internal void getRules(Mod mod)
        {
            foreach (int n in Lib.GetPars(mod.strListRules))
                mod.Rules.Add(new Rule.Rule(n));
            foreach (var rule in mod.Rules) rule.CompSet.doc.Close();
        }
        /// <summary>
        /// getGroups() - groupping of elements of Model by Material and Profile
        /// </summary>
        /// <ToDo>30.9.2016 - попробовать перенести этот метод в ElmAttSet.Groups</ToDo>
        /// <history> 
        /// 2016.09.29 created
        /// 2017.05.8  перенес с модуль ModHandling, добавил аргумент elements
        /// </history>
        public void getGroups(List<Elm> elements)
        {
            elmMgroups.Clear();
            elmGroups.Clear();
            Dictionary<string, ElmAttSet.ElmAttSet> Elements = new Dictionary<string, ElmAttSet.ElmAttSet>();
            try { Elements = elements.ToDictionary(elm => elm.guid); }
            catch { Msg.F("Model.getGroups inconsystent elements "); }

            //-- группы по Материалам elm.mat
            var matGroups = from elm in elements group elm by elm.mat;
            foreach (var matGr in matGroups)
            {
                List<string> guids = new List<string>();
                foreach (ElmAttSet.ElmAttSet element in matGr)
                {
                    //                      log.Info("mgr.mat=" + matGr.Key + " mgr.element.guid=" + element.guid);
                    guids.Add(element.guid);
                }
                ElmAttSet.Mgroup Mgr = new ElmAttSet.Mgroup(elements, matGr.Key, guids);
                elmMgroups.Add(Mgr);
            }
            log.Info("----- Material Groups Count = " + elmMgroups.Count + "\tfrom " + elements.Count + " elements ----");
            foreach (var mtGr in elmMgroups)
                log.Info("material= " + mtGr.mat + "\tCount= " + mtGr.guids.Count + "\tвес=" + mtGr.totalWeight + "\tобъем=" + mtGr.totalVolume);

            //-- группы по Материалу и Профилю elm.mat && elm.prf
            foreach (var Mgr in elmMgroups)
            {
                string curMat = Mgr.mat;
                string curPrf = Elements[Mgr.guids[0]].prf;
                List<string> guids = new List<string>();
                foreach (var g in Mgr.guids)
                {
                    ElmAttSet.ElmAttSet elm = Elements[g];
                    if (elm.prf == curPrf) guids.Add(g);
                    else
                    {
                        this.elmGroups.Add(new ElmGr(Elements, curMat, curPrf, guids));
                        curPrf = elm.prf;
                        guids = new List<string>();
                        guids.Add(g);
                    }
                }
                if (guids.Count != 0) elmGroups.Add(new ElmGr(Elements, curMat, curPrf, guids));
            }
            log.Info("----- <Material, Profile> Groups Count = " + this.elmGroups.Count);
            int chkSum = 0;
            foreach (var gr in this.elmGroups)
            {
                log.Info("material= " + gr.mat + "\tprofile= " + gr.prf + "\tCount= " + gr.guids.Count);
                chkSum += gr.guids.Count;
            }
            log.Info("-------------- CheckSum: total elements count in all groups = " + chkSum);
        }
    } // end class ModHandler : Model
} // end namespace Model.Handler
