/*--------------------------------------------------------------------------------------------
 * ModHandler : Model -- Handle Model for Report preparation
 * 
 *  21.07.2017 Pavel Khrapkin
 *  
 *--- History ---
 *  8.05.2017 taken from Model code
 * 27.05.2017 getRules
 * 20.06.2017 getGroup re-make with LINQ
 * 28.06.2017 bug fix in PrfUpdate()
 *  3.07.2017 ProfileUpdate module add instead of PrfUdate in ModHandler
 * 21.07.2017 Audit GetGrps and Hndl
 *--- Unit Tests --- 
 * 2017.06.19 UT_ModHandler.UT_Hndl, UT_Pricing OK
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
using TSmatch.ElmAttSet;

namespace TSmatch.Model.Handler
{
    public class ModHandler : Mod
    {
        public static readonly ILog log = LogManager.GetLogger("Model.Handler");

        /// <summary>
        /// getGroups() - groupping of elements of Model by Material and Profile
        /// </summary>
        /// <ToDo>30.9.2016 - попробовать перенести этот метод в ElmAttSet.Groups или вообще выделить Groups в отдельный класс</ToDo>
        /// <history> 
        /// 2016.09.29 created
        /// 2017.05.8  перенес в модуль ModHandling, добавил аргумент elements
        /// 2017.06.27 переписано
        /// 2017.07.20 argument errDialo flag add
        /// </history>
        public List<ElmGr> getGrps(List<Elm> elements, bool errDialog = true)
        {
            if (elements == null || elements.Count == 0) Msg.F("getGrps: no elements");
            var gr = new ElmGr(errDialog);
            List<ElmGr> groups = new List<ElmGr>();
            var grps = elements.GroupBy(x => x.prf);
            foreach (var grp in grps) groups.Add(new ElmGr(grp));
            if (elements.Count != groups.Sum(x => x.guids.Count)) Msg.F("getGrps internal error");
            var v = new ProfileUpdate.ProfileUpdate(ref groups);
            return groups;
        }
#if OLD //27/6/17
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
                //7/6                string curPrf = Lib.ToLat(Elements[Mgr.guids[0]].prf.ToLower());
                string curPrf = Elements[Mgr.guids[0]].prf;
                List<string> guids = new List<string>();
                foreach (var g in Mgr.guids)
                {
                    ElmAttSet.ElmAttSet elm = Elements[g];
                    if (elm.prf == curPrf) guids.Add(g);
                    else
                    {
                        elmGroups.Add(new ElmGr(Elements, curMat, curPrf, guids));
                        curPrf = elm.prf;
                        guids = new List<string>();
                        guids.Add(g);
                    }
                }
                if (guids.Count != 0) elmGroups.Add(new ElmGr(Elements, curMat, curPrf, guids));
            }

            elmGroups = PrfUpdate(elmGroups);
//26/6            PrfUpdate();

            log.Info("----- <Material, Profile> Groups Count = " + this.elmGroups.Count);
            int chkSum = 0;
            foreach (var gr in this.elmGroups)
            {
                log.Info("material= " + gr.mat + "\tprofile= " + gr.prf + "\tCount= " + gr.guids.Count);
                chkSum += gr.guids.Count;
            }
            log.Info("-------------- CheckSum: total elements count in all groups = " + chkSum);
        }
#endif //OLD 27/7/17
        /// <summary>
        /// Hndl(model) - find matching Components for model and total_price
        /// </summary>
        /// <param name="mod">model to be handled</param>
        public void Hndl(ref Mod mod)
        {
            Log.set("MH.Hndl");
            mod.elmGroups = getGrps(mod.elements);
            // find matching Components with Rules by Match 
            foreach (var gr in mod.elmGroups)
            {
                bool b = false;
                foreach (var rule in mod.Rules)
                {
log.Info("==>Hndl.MD5=" + mod.MD5 + " =?= " + mod.getMD5(mod.elements));
                    Mtch _match = new Mtch(gr, rule);
                    if (_match.ok == Mtch.OK.Match)
                    {
log.Info("=match.ok=>Hndl.MD5=" + mod.MD5 + " =?= " + mod.getMD5(mod.elements));
                        mod.matches.Add(_match);
                        gr.SupplierName = _match.rule.Supplier.name;
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
#if OLD //27/6/17
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
                match.group.SupplierName = match.rule.Supplier.name;
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
#endif //OLD 27/6/17
        public void Pricing(ref Mod m)
        {
            Log.set("mh.Pricing");
            if (m.Rules == null || m.Rules.Count == 0)
            {
                if (sr == null) sr = new SaveReport.SavedReport();
                sr.getSavedRules();
                m.Rules = sr.Rules;
            }
            foreach (var rule in m.Rules) rule.Init();
log.Info(">m.MD5=" + m.MD5 + " =?= " + m.getMD5(m.elements));
            Hndl(ref m);
log.Info(">m.MD5=" + m.MD5 + " =?= " + m.getMD5(m.elements));
            Log.Trace("      date=\t" + m.date + "\tMD5=" + m.MD5 + "\telementsCount=" + m.elementsCount);
            Log.Trace("price date=\t" + m.pricingDate + "\tMD5=" + m.pricingMD5 + "\ttotal price" + m.total_price);
            Log.exit();
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
#if OLD //3.7.17
        /// <summary>
        ///  PrfUpdate() - Profile code corrections for some groups
        /// </summary>
        /// <Description>
        /// Этот модуль преобразует строку - профиль группы в соответствие российским ГОСТ,
        /// так, как это делается в среде Russia для Tekla. По сути, это hardcode, он не 
        /// должен работать вне России.
        /// Здесь текст строки, получаемой из Tekla API заменяется, на первое значение
        /// аргумента в перечне PrfNormalyze. Если моды Mark- меняется только марка, если
        /// Full - помимо марки, остальная часть строки и параметры могут быть переставлены.
        /// Полнота преобразования кодов проверялась по ГОСТ и среде Tekla Russia.
        /// </Description>
        public List<ElmGr> PrfUpdate(List<ElmGr> grp)
        {
            PrfNormalize(ref grp, "—", "PL", "Полоса");
            PrfNormalize(ref grp, "L", "Уголок");
            PrfNormalize(ref grp, "I", "Балка");
            PrfNormalize(ref grp, "[", "U", "Швеллер");
            PrfNormalize(ref grp, "Гн.[]", "PP", "Тр.", "Труба пр");
            PrfNormalize(ref grp, "Гн.", "PK", "Тр.");
            return grp;
        }
        /// <summary>
        /// PrfNormalize operate in <Full>, or in <Mark> mode:
        /// <para>  - Mark: only setup Mark (i.e. Profile type) as pointed in first argument, or</para>
        /// <para>  - Full: setup Mark, and sort digital parameter values the profile template list;</para> 
        /// </summary>
        private void PrfNormalize(ref List<ElmGr> grp, params string[] prfMark)
        {
            foreach (var gr in grp)
            {
                foreach (string s in prfMark)
                {
                    if (!gr.Prf.Contains(s) && !gr.prf.Contains(s)) continue;
                    string initialPrf = gr.Prf;
                    gr.Prf = PrfNormStr(gr.prf, prfMark[0], Lib.GetPars(gr.Prf));
                    gr.prf = Lib.ToLat(gr.Prf.ToLower());
                    log.Info("--- " + initialPrf + " -> " + "Prf=" + gr.Prf + "gr.prf=" + gr.prf);
                    break;
                }
            }
        }

        string PrfNormStr(string str, string mark, List<int> pars)
        {
            switch (mark)
            {
                case "I":
                    mark += pars[0];
                    if (str.Contains("b1")) { mark += "Б1"; break; }
                    if (str.Contains("b2")) { mark += "Б2"; break; }
                    if (str.Contains("b3")) { mark += "Б3"; break; }
                    if (pars.Count != 1) Msg.F("Internal error");
                    break;
                case "[":
                    mark += pars[0];
                    if (str.Contains("ap")) { mark += "аП"; break; }
                    if (str.Contains("p"))  { mark += "П";  break; }
                    if (str.Contains("ay")) { mark += "аУ"; break; }
                    if (str.Contains("y"))  { mark += "У";  break; }
                    if (str.Contains("e"))  { mark += "Э";  break; }
                    if (str.Contains("l"))  { mark += "Л";  break; }
                    if (str.Contains("ca")) { mark += "Cа"; break; }
                    if (str.Contains("cb")) { mark += "Cб"; break; }
                    if (str.Contains("c"))  { mark += "C";  break; }
                    if (pars.Count != 1) Msg.F("Internal error");
                    break;
                
                case "Гн.[]":
                    break;
                case "Гн.":
                    break;
            }


            //////////switch (pars.Count)
            //////////{
            //////////    case 1:
            //////////        if (mark == "[")
            //////////        {
            //////////            mark += pars[0];
            //////////            if (str.Contains("ap")) { mark += "аП"; break; }
            //////////            if (str.Contains("p"))  { mark += "П";  break; }
            //////////            if (str.Contains("ay")) { mark += "аУ"; break; }
            //////////            if (str.Contains("y"))  { mark += "У";  break; }
            //////////            if (str.Contains("e"))  { mark += "Э";  break; }
            //////////            if (str.Contains("l"))  { mark += "Л";  break; }
            //////////            if (str.Contains("ca")) { mark += "Cа"; break; }
            //////////            if (str.Contains("cb")) { mark += "Cб"; break; }
            //////////            if (str.Contains("c"))  { mark += "C";  break; }
            //////////            break;
            //////////        }
            //////////        if (mark == "I")
            //////////        {
            //////////            mark += pars[0];
            //////////            if (str.Contains("b1")) { mark += "Б1"; break; }
            //////////            if (str.Contains("b2")) { mark += "Б2"; break; }
            //////////            if (str.Contains("b3")) { mark += "Б3"; break; }
            //////////            break;
            //////////        }
            //////////        if (mark == "—") mark += pars[0];
            //////////        break;
            //////////    case 2:
            //////////        if (mark == "I")
            //////////        {
            //////////            mark += pars[0];
            //////////            if (str.Contains("b")) { mark += "Б" + pars[1]; break; }
            //////////            if (str.Contains("k"))
            //////////            {
            //////////                mark += "К" + pars[1];
            //////////                if (str.Contains("a")) mark += "A";
            //////////                break;
            //////////            }
            //////////        }
            //////////        if (mark == "Гн.") { mark += pars[0] + "x" + pars[1]; break; }
            //////////        mark += pars.Min() + "x" + pars.Max();
            //////////        break;
            //////////    case 3:
            //////////        if (md == PrfOpMode.Mark)
            //////////        {
            //////////            mark += pars[0] + 'x' + pars[1] + 'x' + pars[2];
            //////////            break;
            //////////        }
            //////////        if (mark == "Гн.[]")
            //////////        {
            //////////            if (pars[0] == pars[1]) return "Гн." + pars.Max() + "x" + pars.Min();
            //////////            mark += pars[0] + "x" + pars[1] + "x" + pars[2];
            //////////            break;
            //////////        }
            //////////        int p1 = pars.Min();
            //////////        pars.Remove(p1);
            //////////        int p3 = pars.Max();
            //////////        pars.Remove(p3);
            //////////        mark += p1 + "x" + pars[0] + "x" + p3;
            //////////        break;
            //////////    default: Msg.F("ModHandler.grPrfPars not recognized Profile"); break;
            //////////}
            return mark;
        }
#endif //OLD 3.7.17
    } // end class ModHandler : Model
} // end namespace Model.Handler