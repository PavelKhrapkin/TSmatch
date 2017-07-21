/*----------------------------------------------------------------------------
 * Components -- Supplier's price-list load and handling 
 * 
 * 15.07.2017  П.Храпкин
 *
 * --- Unit Testing ---
 * 2017.07.15 UT_Component_checkComp, UT_Component_rulePar, UT_isOK OK
 * --- History ---
 * 30.11.2016 made as separate module, CompSet is now in another file
 * 30.12.2016 fill matFP, prfFP in setComp()
 * 28.02.2017 fill component fields incliding List<FP> from price-list in constructor
 * 22.04.2017 Component match isMatchGrRule=true with empty Section.body
 * 24.04.2017 getMatch method updated
 *  9.05.2017 Msg.W in Str() about wrong CompSet Load Description
 *  4.06.2017 IsMatch(..) and ruleRep(pattern, str) made for nest matching
 * ---------------------------------------------------------------------------
 *      Methods:
 * getCompSet(name, Supplier) - getCompSet by  its name in Supplier' list
 * setComp(doc) - инициальзация данных для базы компонентов в doc
 * getComp(doc) - загружает Excel файл - список комплектующих от поставщика
 * isMatch(gr,rule) - check if this Component in match with Rule and Group
 * UddateFrInternet() - обновляет документ комплектующих из Интернет  
 *    --- Miscelleneous methods ---
 * UpgradeFrExcel(doc, strToDo) - обновление Документа по правилу strToDo
 */
using System.Linq;
using System.Collections.Generic;
using log4net;
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Docs = TSmatch.Document.Document;
using DP = TSmatch.DPar.DPar;
using Sec = TSmatch.Section.Section;
using SType = TSmatch.Section.Section.SType;
using System;
using TSmatch.Section;

namespace TSmatch.Component
{
    public class Component
    {
        public static readonly ILog log = LogManager.GetLogger("Component");

        public DP compDP;

        /// <summary>
        /// constructor Component(doc, i, List<FP>cs_fps, List<FP>rule_fps) - get Component from price-list in doc line i
        /// </summary>
        /// <param name="doc">document - price-list</param>
        /// <param name="i">line number in doc</param>
        /// <param name="csDP">DP - parsed LoadDescroption from CompSet</param>
        public Component(Docs doc, int i, DP csDP)
        {
            compDP = new DP("");
            foreach (SType sec in csDP.dpar.Keys)
            {
                var II = csDP.Col(SType.UNIT_Weight);
                int col = csDP.Col(sec);
                if (col > 0 && col <= doc.Body.iEOC())
                    compDP.Ad(sec, doc.Body.Strng(i, col));
            }
        }

#if DEBUG   //-- 30-Mar-2017 -- вариант конструктора только для тестирования
        public Component(DP comp = null, DP csDP = null)
        {
            compDP = comp;
        }
#endif  // DEBUG -- вариант для тестирования

        // for excetrnal use, including String representation in Viewer
        public string viewComp(SType stype)
        {
            string str = "";
            try { str = compDP.dpStr[stype]; }
            catch { str = "##NOT_AVAILABLE##"; }
            return str;
        }
        // for internal use, f.e. for comparision
        public string viewComp_(SType stype)
        {
            string str = "";
            try { str = compDP.dpar[stype]; }
            catch { str = "##NOT_AVAILABLE##"; }
            return Lib.ToLat(str.ToLower());
        }
#if OLD
        ///////////////////// <summary>
        ///////////////////// setComp(doc) - fill price list of Components from doc
        ///////////////////// setComp(doc_name) - overload
        ///////////////////// </summary>
        ///////////////////// <param name="doc">price-list</param>
        ///////////////////// <returns>List of Components</returns>
        ///////////////////// <history>26.3.2016
        /////////////////////  3.4.2016 - setComp(doc_name) overload
        /////////////////////  8.4.2016 - remove unnecteesary fields - bug fix
        ///////////////////// 14.4.2016 - field mat = Material fill 
        /////////////////////  </history>
        //////////////////public static List<Component> setComp(string doc_name)
        //////////////////{ return setComp(Docs.getDoc(doc_name)); }
        //////////////////public static List<Component> setComp(Docs doc)
        //////////////////{
        //////////////////    Log.set("setComp(" + doc.name + ")");
        //////////////////    List<int> docCompPars = Lib.GetPars(doc.LoadDescription);
        //////////////////    //-- заполнение массива комплектующих Comps из прайс-листа металлопроката
        //////////////////    List<Component> Comps = new List<Component>();
        //////////////////    for (int i = doc.i0; i <= doc.il; i++)
        //////////////////    {
        //////////////////        try
        //////////////////        {
        //////////////////            string descr = doc.Body.Strng(i, docCompPars[0]);
        //////////////////            double lng = 0;
        //////////////////            //-- разбор параметров LoadDescription

        //////////////////            List<int> strPars = Lib.GetPars(descr);
        //////////////////            string docDescr = doc.LoadDescription;
        //////////////////            int parShft = 0;
        //////////////////            while (docDescr.Contains('/'))
        //////////////////            {
        //////////////////                string[] s = doc.LoadDescription.Split('/');
        //////////////////                List<int> c = Lib.GetPars(s[0]);
        //////////////////                int pCol = c[c.Count() - 1];    // колонка - последний параметр до '/'
        //////////////////                List<int> p = Lib.GetPars(s[1]);
        //////////////////                lng = strPars[p[0] - 1];    // длина заготовки = параметр в str; индекс - первое число после '/'
        // 29/3/2017 /////                docDescr = docDescr.Replace("/", "");
        //  устарело /////                parShft++;
        //////////////////            }
        //////////////////            if (lng == 0)
        //////////////////                lng = doc.Body.Int(i, docCompPars[1]) / 1000;    // для lng указана колонка в LoadDescription   
        //////////////////            double price = doc.Body.Double(i, docCompPars[2] + parShft);
        //////////////////            double wgt = 0.0;   //!!! времянка -- пока вес будем брать только из Tekla
        //////////////////            string mat = "";    //!!! времянка -- материал нужно извлекать из description или описания - еще не написано!
        //////////////////            Comps.Add(new Component(descr, mat, lng, wgt, price));
        //////////////////        }
        //////////////////        catch { Msg.F("Err in setComp", doc.name); }
        //////////////////    }
        //////////////////    Log.exit();
        //////////////////    return Comps; 
        //////////////////}
#endif
        public bool isMatch(ElmAttSet.Group gr, Rule.Rule rule = null)
        {
            if (!isMatchGrRule(SType.Material, gr, rule)) return false;
            if (!isMatchGrRule(SType.Profile, gr, rule)) return false;
            return true;
        }

        bool isMatchGrRule(SType stype, ElmAttSet.Group gr, Rule.Rule rule)
        {
            if (rule == null || !compDP.dpar.ContainsKey(stype)) return true;
            string sb = new Sec(rule.text, stype).body;
            if (sb == "") return true;
            var ruleSyns = rule.synonyms;
            string comMatPrf = viewComp_(stype);
            string grMatPrf = stype == SType.Material ? gr.mat : gr.prf;
            if (ruleSyns != null && ruleSyns.ContainsKey(stype))
            {
                List<string> Syns = ruleSyns[stype].ToList();
                if (!Lib.IContains(Syns, comMatPrf) || !Lib.IContains(Syns, grMatPrf)) return false;
                string c = strExclude(comMatPrf, Syns);
                string g = strExclude(grMatPrf, Syns);
                if (c == g) return true;
                string pattern = new Sec(rule.text, stype).body.Replace("=", "");
                foreach (var s in Syns) pattern = strExclude(pattern, Syns);
                return isOK(pattern, c, g);
            }
            return comMatPrf == grMatPrf;
        }

        //check if с - part of current Component, and g - part of group
        //.. is in match in terms of template pattern string with wildcards
        public bool isOK(string pattern, string c, string g)
        {
            var p_c = rulePar(pattern, c);
            var p_g = rulePar(pattern, g);
            int cnt = Math.Min(p_c.Count, p_g.Count);
            for (int i = 0; i < cnt; i++)
            {
                if (p_c[i] == p_g[i]) continue;
                if (p_c[i][0] != '@') return false;
                List<int> pc = Lib.GetPars(p_c[i].Substring(1));
                List<int> pg = Lib.GetPars(p_g[i].Substring(1));
                int minPars = Math.Min(pc.Count, pg.Count);
                for (int n = 0; n < minPars; n++) if (pc[n] < pg[n]) return false;
            }
            return true;
        }

        /// <summary>
        /// rulePar(pattern, str) - parse str with rule text pattern with '*' as wildcard
        /// <para>return List of substring str suits to '*'</para>
        /// <para>suffix '@' in pattern (f.e. "*x*@") means the Component could be cutted at this parameted dimension</para>
        /// <para>throw Exception, when str not suit to the Rule.text</para>
        /// <param name="pattern">pattern to parse</param>
        /// <param name="str">string to be parsed with pattern</param>
        /// <returns>list of substrings - parameters</returns>
        public List<string> rulePar(string pattern, string str)
        {
            List<string> par = new List<string>();
            string atFlag = string.Empty;
            foreach (string s in pattern.Split('*'))
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                char uptoCh = s[0];
                int ind_str = str.IndexOf(uptoCh);
                if (ind_str == -1 && uptoCh == 'x')
                {
                    ind_str = str.IndexOf('*'); // не по ГОСТ, но '*' активно используется в Tekla
                    if (ind_str == -1) break;
                }
                if (ind_str < 0) throw new Exception();
                string v = str.Substring(0, ind_str);
                str = str.Substring(ind_str + 1);
                if (v.Length > 0) par.Add(atFlag + v);
                atFlag = (s.Length > 1 && s[1] == '@') ? "@" : string.Empty;
            }
            if (str.Length > 0) par.Add(atFlag + str);
            return par;
        }

        private string strExclude(string str, List<string> syns)
        {
            foreach (string s in syns)
            {
                if (!str.Contains(s)) continue;
                return str.Substring(s.Length);
            }
            Msg.F("Rule.strExclude error", str, syns);
            return null;
        }
#if OLD
        //24/4                return c.Contains(g);
                ////////////////var p1 = Params(Syns, comMatPrf, );
                //// 27/3 //////var p2 = Params(Syns, grMatPrf);
                ////////////////bool b = p1 != p2  && stype == SType.Material;
                //31/3//////////return Params(Syns, comMatPrf) == Params(Syns, grMatPrf);
        string Params(List<string> lst, string str)
        {
            foreach (string st in lst)
            {
                if (!str.Contains(st)) continue;
                string result = str.Substring(st.Length);
                return str.Substring(st.Length);
            }
            return null;
        }

        /// <summary>
        /// getComp(CompSet cs) - загружает Документ - прайс-лист комплектующих
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        /// <history> 21.2.2016
        /// 24.2.2016 - выделил в отдельный модуль Components
        /// 27.2.2016 - оформил внутреннюю структуру Component и встроил в getComp(doc)
        ///             для разбора, где указана длина комплектующего, используем строку
        ///             вида <col>/<№ параметра в str> 
        /// </history>
        public void getComp()
        {
            Log.set("getComp");
            //            setComp(this.doc);
            try
            {

            }
            catch
            {

            }

            ////List<int> docCompPars = Mtch.GetPars(doc.LoadDescription);
            //////-- заполнение массива комплектующих Comps
            ////Comps.Clear();
            ////for (int i = doc.i0; i <= doc.il; i++)
            ////{
            ////    string str = doc.Body.Strng(i, docCompPars[0]);
            ////    double lng = 0;
            ////    //-- разбор параметров LoadDescription
            ////    List<int> strPars = Mtch.GetPars(str);
            ////    string docDescr = doc.LoadDescription;
            ////    int parShft = 0;
            ////    while (docDescr.Contains('/'))
            ////    {
            ////        string[] s = doc.LoadDescription.Split('/');
            ////        List<int> c = Mtch.GetPars(s[0]);
            ////        int pCol = c[c.Count() - 1];    // колонка - последний параметр до '/'
            ////        List<int> p = Mtch.GetPars(s[1]);         
            ////        lng = strPars[p[0] - 1];    // длина заготовки = параметр в str; индекс - первое число после '/'
            ////        docDescr = docDescr.Replace("/", "");
            ////        parShft++;
            ////    }
            ////    if (lng == 0)
            ////        lng = doc.Body.Int(i, docCompPars[1])/1000;    // для lng указана колонка в Comp    
            ////    double? price = doc.Body.Double(i, docCompPars[2] + parShft);
            ////    double wgt = 0.0;   //!!! времянка -- пока вес будем брать из Tekla 
            ////    Comps.Add(new Component(str, lng, wgt, price));
            ////}
            Log.exit();
        }
        public static Docs UpgradeFrExcel(Docs doc, string strToDo)
        {
            Log.set("UpgradeFrExcel(" + doc.name + ", " + strToDo + ")");
            if (strToDo != "DelEqPar1") Log.FATAL("не написано!");
            //!!            List<string> Comp = getComp(doc);
            //////int i = doc.i0;
            //////foreach (string s in Comp)
            //////{
            //////    string str = Lib.ToLat(s);
            //////    List<int> pars = Mtch.GetPars(s);
            //////    if (pars[0] == pars[1])
            //////    {        
            //////        string toDel = pars[0].ToString() + " x ";
            //////        str = str.Replace(toDel + toDel, toDel); 
            //////    }
            //////    doc.Body[i++, 1] = str;
            //////}
            //////doc.isChanged = true;
            //////Docs.saveDoc(doc);

            //for (int i = doc.i0, iComp = 0; i <= doc.il; i++)
            //{
            //    // doc.Body.Strng(i, 1) = Copm;
            //}
            Log.exit();
            return doc;
        }
#endif //OLD
        /// <summary>
        /// Str(SType stype) - return Sec stype of Component for Viewer
        /// </summary>
        /// <param name="stype">SType Section of Component representation</param>
        /// <returns>representation of Component as a string</returns>
        public string Str(SType stype)
        {
            if (!compDP.dpStr.ContainsKey(stype))
            {
                if (stype == SType.Price) throw new KeyNotFoundException();
                Msg.W("CompSet wrong LoadDescriptor", stype);
                return string.Empty;
            }
            return compDP.dpStr[stype];
        }
    } // end class Component
} // end namespace Component
