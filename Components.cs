/*----------------------------------------------------------------------------
 * Components -- работа с документами - прайс-листами поставщиков компонентов
 * 
 * 30.12.2016  П.Храпкин
 *
 *----- ToDo -----
 * 29.12.2016 написать загрузку из прайс-листа setComp(..) c разбором LoadDescriptor
 * --- журнал ---
 * 30.11.2016 made as separate module, CompSet is now in another file
 * 30.12.2016 fill matFP, prfFP in setComp()
 * ---------------------------------------------------------------------------
 *      МЕТОДЫ:
 * getCompSet(name, Supplier) - getCompSet by  its name in Supplier' list
 * setComp(doc) - инициальзация данных для базы компонентов в doc
 * getComp(doc) - загружает Excel файл - список комплектующих от поставщика
 * UddateFrInternet() - обновляет документ комплектующих из Интернет  
 * ----- class CompSet
 *      МЕТОДЫ:
 * getMat ()    - fill mat ftom CompSet.Components and Suplier.TOC
 * 
 *    --- Вспомогательные методы - подпрограммы ---
 * UpgradeFrExcel(doc, strToDo) - обновление Документа по правилу strToDo
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;

using TST = TSmatch.Test.assert;
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using TS = TSmatch.Tekla.Tekla;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Mtch;
using Supl = TSmatch.Suppliers.Supplier;
using FP = TSmatch.FingerPrint.FingerPrint;
using TSmatch.Document;
using TSmatch.Rule;

namespace TSmatch.Component
{
    public class Component
    {
        public static readonly ILog log = LogManager.GetLogger("Component");

        public readonly string description; // строка вида "Угoлoк cтaльнoй paвнoпoл. 25 x 4 cт3cп/пc5"
        public readonly string mat;         // материал компонента. Извлекается из description или из прайс-листа
        public readonly double length;      // длина заготовки в м
        public readonly double weight_m;    // ($W1) вес погонного метра заготовки или за кубометр (для бетона)
        public readonly double? price;      // ($P1) цена за 1 тонну в руб
        public readonly double vol_m;
        public readonly List<FP> fps = new List<FP>();
//        public readonly FP matFP, prfFP;

        public Component(string _description, string _mat, double _length, double _weight, double? _price)
        {
            description = Lib.ToLat(_description);
            mat = _mat;
            length = _length;
            weight_m = _weight;
            price = _price;
        }

        public Component(Docs doc, int i, List<FP> cs_fps, List<FP>rule_fps)
        {
            string[] sections = Lib.ToLat(doc.LoadDescription).ToLower().Split(';');
            bool flag = false;
            foreach (string sec in sections)
            {
                if (string.IsNullOrEmpty(sec)) continue;
// 13/2 /       FP csFP = cs_fps.Find(x => x.section == x.RecognyseSection(sec));
// 13/2 /       if (csFP == null) Msg.F("Component constructor: no CompSet.FP with doc.LoadDescription", sec);
// 13/2 /       int col = csFP.Col();               
// 13/2 /       FP compFP = new FP(str, csFP, ref flag);
                FP compFP = new FP(sec, cs_fps, ref flag);
                int col = compFP.Col();
                string str = doc.Body.Strng(i, col);
                if (flag) fps.Add(compFP);
            }



            ////////////////////////Dictionary<FP.Section, string> tmp_sec = new Dictionary<FP.Section, string>(); 
            ////////////////////////foreach (var fp in cs_fps)
            ////////////////////////{
            ////////////////////////    //                log.Info(fp.Info());
            ////////////////////////    int col = fp.Col();
            ////////////////////////    string str = Lib.ToLat(doc.Body.Strng(i, col));
            ////////////////////////    if (fp.section == FP.Section.Material) mat = str;
            ////////////////////////    if (fp.section == FP.Section.Description) description = str; 
            ////////////////////////    double? val = doc.Body.Double(i, col);
            // 23/1 ////////////////    if (fp.section == FP.Section.Price) price = val;
            ////////////////////////    if (fp.section == FP.Section.WeightPerUnit) weight_m = (double)val;
            ////////////////////////    if (fp.section == FP.Section.LengthPerUnit) length = (double)val;
            ////////////////////////    if (fp.section == FP.Section.VolPerUnit) vol_m = (double)val;
            ////////////////////////    tmp_sec.Add(fp.section, str);
            ////////////////////////}
            ////////////////////////foreach (FP fp in rule_fps)
            ////////////////////////{
            ////////////////////////    string str = tmp_sec[fp.section];
            ////////////////////////    FP ruleTx = rule_fps.Find(x => x.section == fp.section);
            ////////////////////////    if (ruleTx == null) Msg.F("Component constructor has no the Rule Section", doc.name);
            ////////////////////////    // 17.1 //                fps.Add(new FP(FP.type.Component, str, fp.section, rule_fps));
            ////////////////////////}
        }

        /// <summary>
        /// setComp(doc) - fill price list of Components from doc
        /// setComp(doc_name) - overload
        /// </summary>
        /// <param name="doc">price-list</param>
        /// <returns>List of Components</returns>
        /// <history>26.3.2016
        ///  3.4.2016 - setComp(doc_name) overload
        ///  8.4.2016 - remove unnecteesary fields - bug fix
        /// 14.4.2016 - field mat = Material fill 
        ///  </history>
        public static List<Component> setComp(string doc_name)
        { return setComp(Docs.getDoc(doc_name)); }
        public static List<Component> setComp(Docs doc)
        {
            Log.set("setComp(" + doc.name + ")");
            List<int> docCompPars = Lib.GetPars(doc.LoadDescription);
            //-- заполнение массива комплектующих Comps из прайс-листа металлопроката
            List<Component> Comps = new List<Component>();
            for (int i = doc.i0; i <= doc.il; i++)
            {
                try
                {
                    string descr = doc.Body.Strng(i, docCompPars[0]);
                    double lng = 0;
                    //-- разбор параметров LoadDescription

                    List<int> strPars = Lib.GetPars(descr);
                    string docDescr = doc.LoadDescription;
                    int parShft = 0;
                    while (docDescr.Contains('/'))
                    {
                        string[] s = doc.LoadDescription.Split('/');
                        List<int> c = Lib.GetPars(s[0]);
                        int pCol = c[c.Count() - 1];    // колонка - последний параметр до '/'
                        List<int> p = Lib.GetPars(s[1]);
                        lng = strPars[p[0] - 1];    // длина заготовки = параметр в str; индекс - первое число после '/'
                        docDescr = docDescr.Replace("/", "");
                        parShft++;
                    }
                    if (lng == 0)
                        lng = doc.Body.Int(i, docCompPars[1]) / 1000;    // для lng указана колонка в LoadDescription   
                    double? price = doc.Body.Double(i, docCompPars[2] + parShft);
                    double wgt = 0.0;   //!!! времянка -- пока вес будем брать только из Tekla
                    string mat = "";    //!!! времянка -- материал нужно извлекать из description или описания - еще не написано!
                    Comps.Add(new Component(descr, mat, lng, wgt, price));
                }
                catch { Msg.F("Err in setComp", doc.name); }
            }
            Log.exit();
            return Comps; 
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

        #region ------ test Component -----
#if DEBUG
        public static void testComponent()
        {
            Log.set("testComponent");
            //-- test environment preparation: set csFPs and doc - price list "ГК Монолит"
            Docs doc = Docs.getDoc("ГК Монолит");
            List<FP> csFPs = new List<FP>();
            Rule.Rule rule = new Rule.Rule(15);
            csFPs = rule.Parser(FP.type.CompSet, doc.LoadDescription);
            TST.Eq(csFPs.Count, 3);

            Component comp = new Component(doc, 8 + 3, csFPs, rule.ruleFPs);
            TST.Eq(comp.fps[0].txs[0], "b");
            TST.Eq(comp.fps[0].txs.Count, 1);
            TST.Eq(comp.fps[0].pars[0].ToString(), "20");
            TST.Eq(comp.fps[0].pars.Count, 1);
            Log.exit();
        }
#endif //#if DEBUG
        #endregion ------ test Component ------
    } // end class Component
} // end namespace Component
