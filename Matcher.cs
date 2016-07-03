/*------------------------------------------------------------------------------------------
 * Matcher -- contains important class Rule, which descride how to find Suppliers Components
 *            suit to the Model Elements. The result - set of foind matches - put in class OK
 *
 * 21.6.2016 П.Храпкин, А.Пасс, А.Бобцов
 *
 *--- History ---
 * 18.1.2016 заложено П.Храпкин, А.Пасс, А.Бобцов
 * 22.1.2016  П.Храпкин, А.Бобцов
 * 16.2.2016 ревизия
 * 27.2.2016 PKh перенес фрагменты в модуль Components
 *  6.3.2016 Rule Constructor correction; Start check if Tekla is active
 * 15.3.2016 Use Model Rule list -- UseRules
 * 26.3.2016 CompSet reference in the Rule class instead of Document
 *  3.4.2016 adoption to the the Component, CompSet, and Supplier classes
 * 11.4.2016 Remove CompSet and Supplier not gives any match from Model in UseAllRules
 * 21.6.2016 adapt to the Groups, Mgroups etc in ElmAttSet
 * -----------------------------------------------------------------------------------------
 *      КОНСТРУКТОРЫ Правил - загружают Правила из листа Правил в TSmatch или из журнала моделей
 * Rule(дата, тип, текст Правила, документ-база сортаментов)    - простая инициализация из TSmatch
 * Rule(Docs doc, int n)    - инициализируем Правило из строки n листа "Matching_Rules" в TSmatch
 * Rule(int n)              - по умолчанию документа, загружаем Правило из листа в TSmatch
 * ---------  Rules - коллекция Правил -----------
 *      структура OK - содержит данные о найденных заготовках, соответствующих элементам модели   
 * OK(int _gr, string _str, Docs _doc, int _nComp, double _w, double? _p)   - конструктор
 * okToObj(OK ok, ref object[] obj, int fr) - заполняем элементы массива obj, начиная с номера fr
 * clearObj(ref object[] obj, int fr)       - обнуляем элементы массива obj, начиная с номера fr
 * ---------  OKs - коллекция найденных соответствий модели и заготовок -----------
 *      МЕТОДЫ:
 * Start()      - инициирует начальный запуск всех модулей сисемы TSmatch
 * UseRules()   - применяем Правила к группам компонент модели. Вызывает
 * SearchInComp() - ищет подходящие комплектующие в соответствие с Правилами    
 *    --- Вспомогательные меторды - подпрограммы ---
 * RuleParsre(r.text) - синтаксический разбор Правила r.text
 * attParse(str, reg, lst, ref n) - разбор раздела Правила в строке str
 * GetPars(str) -public- разбирает строку- часть компонента или Правила, выделяя int параметры.
 * isRuleApplicable(r.text, gr.mat, gr.prf) - определяет, применимо ли Правило?
 * isOKexist(n) - возвращает true, если для группы n уже найдено соответствие в OKs
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;

using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using TS = TSmatch.Tekla.Tekla;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using ElmGr = TSmatch.ElmAttSet.Group;
using Cmp = TSmatch.Components.Component;
using CmpSet = TSmatch.Components.CompSet;
using Supl = TSmatch.Suppliers.Supplier;
using System;

namespace TSmatch.Matcher
{
    public class Matcher
    {
        static List<Rule> Rules = new List<Rule>();

        public struct Rule // структура, описывающая правило работы Matcher'а
        {
            public readonly string name;        //название правила
            public readonly string type;        //тип правила
            public readonly string text;        //текст правила
            public readonly CmpSet CompSet;     //список компонентов, с которыми работает правило
            public readonly Supl Supplier;      //Поставщик

            public Rule(Docs doc, int i) : this()
            {
                this.name = (string)doc.Body[i, Decl.RULE_NAME];
                this.type = (string)doc.Body[i, Decl.RULE_TYPE];
                this.text = Lib.ToLat((string)doc.Body[i, Decl.RULE_RULE]);
                string csName = (string)doc.Body[i, Decl.RULE_COMPSETNAME];
                string suplName = (string)doc.Body[i, Decl.RULE_SUPPLIERNAME];
                this.Supplier = Supl.getSupplier(suplName);
                this.CompSet = CmpSet.getCompSet( csName, Supplier.name );
            }
            // параметр doc не указан
            public Rule(int n) : this(Docs.getDoc(Decl.RULES), n) {}
        }
        public struct OK    // структура даннsх, описывающая найденное соответствие..
        {                   //..Правил, Прайс-листа комплектующих, и строки - Группы <mat,prf>
            public int gr_nstr; // номер строки Группы, для которой найдено соответствие
            string strComp;     // текст подходящей строки в Comp
            public Docs doc;           // Документ, в котором нашли подходящие комплектующие
            int nComp;          // номер строки в Документе комплектующих, по которому ОК
            double weignt;      // общий вес для найденного типа комплектующих
            double? price;      // общая стоимость найденного типа комплектующих; null - не найдено
            /// <summary>
            /// OK(n_gr, strComp, doc, nComp, Weight, Price)
            /// </summary>
            /// <param name="_gr"></param>
            /// <param name="_str"></param>
            /// <param name="_doc"></param>
            /// <param name="_nComp"></param>
            /// <param name="_w"></param>
            /// <param name="_p"></param>
            public OK(int _gr, string _str, Docs _doc, int _nComp, double _w, double? _p)
            {
                this.gr_nstr = _gr; this.strComp = _str;
                this.doc = _doc;    this.nComp = _nComp;
                this.weignt = _w;   this.price = _p;
            }
            public void okToObj(OK ok, ref object[] obj, int fr)
            {
                obj[fr++] = ok.strComp;
                obj[fr++] = ok.doc.name; obj[fr++] = ok.nComp;
                obj[fr++] = ok.weignt;   obj[fr] = ok.price;
            }
            public static object[] clearObj(ref object[] obj, int fr)
            {
                obj[fr++] = "";
                obj[fr++] = ""; obj[fr++] = "";
                obj[fr++] = ""; obj[fr] = "";
                return obj;
            }
        }
        static List<OK> OKs = new List<OK>();  // список найденных соответствий

        /// <summary>
        /// Start - initiate Rules - text lines, which describe match process
        /// </summary>
        /// <history> Jan-2016
        /// 19.2.16 - переписано обращение к Documents.Start с инициализацией массивов FileDir
        /// 30.3.16 - Start other modules removed to Bootstrap
        /// </history>
        public static void Start()
        {
            Log.set("Matcher.Start");
            Docs rule = Docs.getDoc(Decl.RULES);   // инициируем список Правил Matcher'a
            for (int i = 4; i<=rule.Body.iEOL(); i++)
                { if (rule.Body[i, Decl.RULE_NAME] != null) Rules.Add(new Rule(rule, i)); }
            Log.exit();
        }
        /// <summary>
        /// UserRules(mod) - Apply Model mod Rules to create TSmatchINFO.xlsx/Report
        /// </summary>
        /// <param name="mod">Model to de handled</param>
        /// <history>10.3.2016
        /// 15.3.2016 - get Rule list (Rules) from the Model mod
        ///  3.4.2016 - adoption to the updated CompSet class
        /// </history>
        public static void UseRules(Mod mod)
        {
            Log.set("UseRules(" + mod.name + ")");
            Rules = mod.Rules;
            Docs Report = Docs.getDoc(Decl.REPORT); // output result in ModelINFO Document
            int nstr = 0;                           // nstr - string number in Groupr
            foreach(var gr in ElmGr.Groups)
            {
                bool found = false;                 // true, when matching Component found

                object[] obj = new object[Report.Body.iEOC()];
                obj[0] = nstr; obj[1] = gr.mat; obj[2] = gr.prf;
//!! 21/6/2016                obj[3] = gr.lng; obj[4] = gr.wgt; obj[5] = gr.vol;

                foreach (var r in Rules)
                {
                    RuleParser(r.text);     // сейчас Правило r многократно разбирается каждый раз для каждой Группы..
                                            // Возможно,списки Rule.List стоит перенести в класс Rule и обработать заранее
                    if (SearchInComp(r.CompSet, nstr, gr.mat, gr.prf))
                    {
                        found = true;
                        foreach (var ok in OKs)
                            if( ok.gr_nstr == nstr) ok.okToObj(ok, ref obj, 6);
                    }
                    //////else
                    //////{
                    //////    mod.CompSets.Remove(r.CompSet);
                    //////    mod.Rules.Remove(r);
                    //////}
                }
                //////if (found) Report.Body.AddRow(obj);
                //////else Report.Body.AddRow(OK.clearObj(ref obj, 6));
                if (!found) OK.clearObj(ref obj, 6);
                Report.wrDoc(1, obj);
                nstr++;
            }
            foreach (var ok in OKs)
            {
                Docs doc = ok.doc;  //подставить
                CmpSet cs = mod.CompSets.Find(x => x.doc == doc);
                if (cs == null) mod.CompSets.Remove(cs);
            }
            foreach (var sup in mod.Suppliers)
            {
                CmpSet cs = mod.CompSets.Find(x => x.Supplier == sup);
                if (cs == null) mod.Suppliers.Remove(sup);
            }
            Log.exit();
        }
        /// <summary>
        /// isRuleApplicable(mat, prf) - return true when parsed Rule is applicable to mat and prf values
        /// </summary>
        /// <param name="mat">Material</param>
        /// <param name="prf">Profile</param>
        /// <returns>true, if Rule after Parcing mentioned mat and prf</returns>
        /// <history>9.4.2016</history>
        static bool isRuleApplicable(string mat, string prf)
        { return Lib.IContains(RuleMatList, mat) && Lib.IContains(RulePrfList, prf); }
        /// <summary>
        /// GetPars(str) разбирает строку раздела компонента или Правила, выделяя числовые параметры.
        ///         Названия материалов, профилей и другие нечисловые подстроки игнорируются.
        /// </summary>
        /// <param name="str">входная строка раздела компонента</param>
        /// <returns>List<int>возвращаемый список найденых параметров</int></returns>
        public static List<int> GetPars(string str)
        {
            const string VAL = @"\d+";
            List<int> pars = new List<int>();
            string[] pvals = Regex.Split(str, Decl.ATT_DELIM);
            foreach (var v in pvals)
            {
                if (string.IsNullOrEmpty(v)) continue;
                if (Regex.IsMatch(v, VAL))
                    pars.Add(int.Parse(Regex.Match(v, VAL).Value));
            }
            return pars;
        }
        /// <описание>
        /// RuleParser разбирает текстовую строку - правило, выделяя и возвращая в списках 
        ///             MatList  - части Правила, относящиеся к Материалу компонента
        ///             PrfList  - части, относящиеся к Профилю
        ///             OthList - остальные разделы Правила.
        /// Разделы начинаются признаком раздела, например, "Материал:" и отделяются ';'
        /// Признак раздела распознается по первой букве 'M' и завершается ':'. Поэтому
        ///             "Профиль:" = "П:" = "Прф:" = "п:" = "Prof:"
        /// Заглавные и строчные буквы эквивалентны, национальные символы транслитерируются.
        /// Разделы Правила можно менять местами и пропускать; тогда они работают "по умолчанию".
        /// '=' означает эквивалентность. Допустимы выражения "C255=Ст3=сталь3".
        /// ',' позволяет перечислять элементы и подразделы Правила.
        /// ' ' пробелы, табы и знаки конца строки игнорируются, однако они могут служить признаком
        ///     перечисления, так же, как пробелы. Таким образом, названия материалов или профилей
        ///     можно просто перечислить - это эквивалентно '='
        /// Параметры   - последовательность символов вида "$p1" или просто "p1" или "Параметр235".
        ///               Параметры начинаются со знака '$' или 'p'  и кончаются цифрой. 
        ///               Их значения подставляются из значений и атрибутов компонента в модели.
        ///               Номер параметра в правиле неважен- он заменяется в TSmatch на номер
        ///               по порядку следования параметров в правиле автоматически.
        ///         Возможно, стоит всегда параметры выделять по знаку $ в качестве первой буквы.
        /// #параметры - величины в Правиле, которые распознаются "по значению" в тексте,
        ///              например, номера колонок с весом или ценой
        /// *параметры - Wild параметр комплектующих; соответствующий элемент модели может иметь
        ///              любое хначение, например, ширина листа, из которого нарезают полосы
        /// Redunduncy% параметр - коэффициент запаса в процентах. Перед десятичным чистом -
        ///              коэффициентом запаса - могут быть ключевые слова, возможно, сокращенные
        ///              (избыток, запас, отходы, Redundency, Excess, Waste) по русски или 
        ///              по английски заглавными или строчными буквами.
        /// </описание>
        /// <history> декабрь 2015 - январь 2016
        /// 19.2.16 - #параметры
        /// 29.2.16 - *параметры
        static List<string> RuleMatList = new List<string>();
        static List<string> RulePrfList = new List<string>();
        static List<string> RuleOthList = new List<string>();
        static List<int> RuleValPars = new List<int>();
        static double RuleRedundencyPerCent = 0.0;
        static List<int> RuleWildPars = new List<int>();
        static int RuleMatNpars = 0, RulePrfNpars = 0, RuleOthNpars = 0;
        static void RuleParser(string rule)
        {
            Log.set("RuleParser(\"" + rule + "\")");
            const string rM = "(m|м).*:", rP = "(п|p).*:",           // Материал и Профиль
                rVal = @"#\d+", rWild = @"\*.*?\d+",                 // #- и *- параметры 
                rDec = @"(\d+)|(\d.\d*)|(\d,\d)",                    // десятичные числа с точкой или запятой
                rRedundancy = @"(изб|зaп|oтx|red|was|excess).*\d+%"; //Redundency|Waste|Excess|Отходы|Запас
            Regex regM = new Regex(rM, RegexOptions.IgnoreCase);
            Regex regP = new Regex(rP, RegexOptions.IgnoreCase);
            RuleMatList.Clear(); RulePrfList.Clear(); RuleOthList.Clear();
            //--- get #value parameters from the rule
            RuleValPars.Clear();
            while (Regex.IsMatch(rule, rVal, RegexOptions.IgnoreCase))
            {
                Match val = Regex.Match(rule, rVal);
                int x = int.Parse(val.Value.Replace("#", ""));
                RuleValPars.Add(x);
                rule = rule.Replace(val.Value, "");
            }
            //--- get List of * parameters from the rule
            RuleWildPars.Clear();
            while (Regex.IsMatch(rule, rWild, RegexOptions.IgnoreCase))
            {
                Match starPar = Regex.Match(rule, rWild);
                List<int> p = GetPars(starPar.Value);
                RuleWildPars.Add(p[0] - 1);
                rule = rule.Replace(starPar.Value, "");
            }
            //--- Redundency % handling
            while (Regex.IsMatch(rule, rRedundancy, RegexOptions.IgnoreCase))
            {
                Match excess = Regex.Match(rule, rRedundancy, RegexOptions.IgnoreCase);
                Match perCent = Regex.Match(excess.ToString(), rDec);
                if (!double.TryParse(perCent.ToString(), out RuleRedundencyPerCent))
                    RuleRedundencyPerCent = 0.0;
                rule = rule.Replace(excess.Value, "");
            }
            //--- Rule parsing by Sections. Разбор Правила по разделам
            string[] tmp = rule.Split(';');
            foreach (var s in tmp)
            {
                string x = s;
                while (x.Length > 0)
                {
                    if (Regex.IsMatch(x, rM, RegexOptions.IgnoreCase))
                        x = attParse(x, regM, RuleMatList, ref RuleMatNpars);
                    if (Regex.IsMatch(x, rP, RegexOptions.IgnoreCase))
                        x = attParse(x, regP, RulePrfList, ref RulePrfNpars);
                    if (x != "")
                        x = attParse(x, null, RuleOthList, ref RuleOthNpars);
                }
            }

            Log.exit();
        }
        /// <summary>
        /// attParse(str, reg, lst, ref n) - разбор раздела Правила в строке str
        /// </summary>
        /// <param name="str">разбираемый раздел Правила</param>
        /// <param name="reg">шаблон - заголовок раздела</param>
        /// <param name="lst">разбираемые параметры раздела</param>
        /// <param name="n">счетчик параметров</param>
        /// <returns>возвращает строку str после разбора - она должна быть пустой</returns>
        /// <history> 12.2.2016 PKh <\history>
        static string attParse(string str, Regex reg, List<string> lst, ref int n)
        {
            if (reg != null) str = reg.Replace(str, "");
            string[] parametrs = Regex.Split(str, Decl.ATT_DELIM);
            foreach (var par in parametrs)
            {
                if (string.IsNullOrEmpty(par)) continue;
                bool parPars = false;
                if (Regex.IsMatch(par, Decl.ATT_PARAM)) { n++; parPars = true; }
                if (!string.IsNullOrWhiteSpace(par)
                    && !Regex.IsMatch(par, Decl.ATT_DELIM)
                    && !parPars) lst.Add(par.ToUpper());
                str = str.Replace(par, "");
            }
            if (str != "") Log.FATAL("строка \"" + str + "\" разобрана не полностью");
            return str;
        }
        /// <summary>
        /// isOKexist(n) - возвращает true, если для группы n уже найдено соответствие в OKs
        /// </summary>
        static bool isOKexist(int n)
        {
            bool result = false;
            foreach (var ok in OKs)
            {
                if (ok.gr_nstr == n)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// SearchInComp(cs, nstr, mat, prf) - search Components in match with mat and prf in price-list 
        /// </summary>
        /// <param name="cs">set of Components - price list of supplies - to search in</param>
        /// <param name="nstr">string number in Groups</param>
        /// <param name="mat">Material of nstr line in Groups</param>
        /// <param name="prf">Profile of nstr line in Groups</param>
        /// <returns></returns>
        /// <history> дек 2015 - фев 2016 - preliminary releses
        /// 1.3.16 - handle *parameters
        /// 3.4.16 - adoption to CompSet and Componenmt classes
        /// </history>
        public static bool SearchInComp(CmpSet cs, int nstr, string mat, string prf)
        {
            Log.set("SearchInComp");
            bool found = false;
            mat = mat.ToUpper(); prf = prf.ToUpper();
            if (isRuleApplicable(mat, prf))
            { 
                List<int> matPars = GetPars(mat);
                List<int> prfPars = GetPars(prf);
                //-- если в Правиле есть * - используем заготовку с любым значением соотв.параметра
                for (int nComp = cs.doc.i0, iComp = 0; nComp <= cs.doc.il; nComp++, iComp++)
                {                                                       // все имеющиеся в элементе..
                    string s = cs.Components[iComp].description;        //..модели параметры prf должны ..
                    if (string.IsNullOrWhiteSpace(s)) continue;         //..совпадать с параметрами Comp..
                    if (!Lib.IContains(RuleMatList, mat)) continue;     //..если совпадают -> found = true
                    if (!Lib.IContains(RulePrfList, prf)) continue;
                    List<int> CompPars = GetPars(s.ToUpper());
                    for (int i = 0, iW = 0; i < prfPars.Count; i++)        
                    {                                                   // check wild parameters 
                        if (iW < RuleWildPars.Count)
                        {
                            if (i == RuleWildPars[iW]) found = true;
                        }
                        if (prfPars[i] != CompPars[i]) break;       
                        if (i == prfPars.Count - 1) found = true;   
                    }
                    if (found)
                    {
                        if (isOKexist(nstr)) break;     // если уже есть соответствие в OKs - NOP
                        // обработаем *параметры
/* //!! 21/6/2016 в отладку с ElmAttSet Group
                        double lng = Mod.Groups[nstr].lng / 1000; //приводим lng к [м]
                        double v = Mod.Groups[nstr].vol;        //объем в [м3]
                        double w = Mod.Groups[nstr].wgt;        //вес в [т]

                        if (w == 0)
                        {
                            w = v * 7.85 * 1000;   // уд.вес стали в кг
                        }
                        w *= 1 + RuleRedundencyPerCent / 100;     // учтем запас в % из Правила
                        double? p = cs.Components[iComp].price * w / 1000;
                        OKs.Add(new OK(nstr, s, cs.doc, nComp, w, p));
*/  //!! 21/6/2016 в отладку с ElmAttSet Group
                        break;
                    }
                } //end foreach Comp
            } // end if isRuleApplicable
            Log.exit();
            return found;
        } // end SearchInComp
    } // end class Matcher
} // end namespace Matcher
