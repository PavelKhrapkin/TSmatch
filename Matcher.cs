/*------------------------------------------------------------------------------------------
 * Matcher -- находит соответствия входным компонентам в документах дазы данных
 *            в соответствии с правилами Match Rules
 *
 * 2.3.2016 П.Храпкин, А.Пасс, А.Бобцов
 *
 *--- журнал ---
 * 18.1.2016 заложено П.Храпкин, А.Пасс, А.Бобцов
 *  22.1.2016  П.Храпкин, А.Бобцов
 *  16.2.2016 ревизия
 *  27.2.2016 PKh перенес фрагменты в модуль Components
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

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;    //низкоуровневый ввод-вывод Windows используется только для Log в Start

using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Decl = TSmatch.Declaration.Declaration;
using TS = TSmatch.Tekla.Tekla;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
using Cmp = TSmatch.Component.Component;
using TSmatch.Document;

using Mtr = match.Matrix.Matr;

namespace TSmatch.Matcher
{
    public class Matcher
    {
        static List<Rule> Rules = new List<Rule>();

        public struct Rule // структура, описывающая правило работы Matcher'а
        {
            public readonly string name;    //название правила
            public readonly string type;    //тип правила
            public readonly string text;    //текст правила
            public readonly string doc;     //входной документ - список компонентов, с которыми работает правило

            public Rule(Docs doc, int i) : this()
            {
                this.name = (string)doc.Body[i, Decl.RULE_NAME];
                this.type = (string)doc.Body[i, Decl.RULE_TYPE];
                this.text = Lib.ToLat((string)doc.Body[i, Decl.RULE_RULE]);
                this.doc = (string)doc.Body[i, Decl.RULE_DOCS];
            }
            // параметр doc не указан
            public Rule(int n) : this(Docs.getDoc(Decl.MATCHING_RULES), n) {}
        }
        public struct OK   // структура даннх, описывающая найденное соответствие..
        {                   //..Правил, Прайс-листа комплектующих, и строки - Группы <mat,prf>
            public int gr_nstr; // номер строки Группы, для которой найдено соответствие
            string strComp;     // текст подходящей строки в Comp
            Docs doc;           // Документ, в котором нашли подходящие комплектующие
            int nComp;          // номер строки в Документе комплектующихб по которому ОК
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
        /// Start - инициализация модулей Models, Reule, Documents
        /// </summary>
        /// <journal> Jan-2016
        /// 19.2.16 - переписано обращение к Documents.Start с инициализацией массивов FileDir
        /// </journal>
        public static void Start()
        {
            Log.set("Matcher.Start");
            string[] FileDirTemplates = {"#TOC", "#Model", "#Components"};
            string TOCdir =TS.GetTeklaDir();
            string ModelDir = TS.GetTeklaDir((int)TS.ModelDir.model);
            string ComponentsDir = TOCdir + @"\База комплектующих";
            string[] FileDirValues = {TOCdir, ModelDir, ComponentsDir};
            Docs.Start(FileDirTemplates, FileDirValues);    // инициируем Документы из TSmatch.xlsx
            Mod.Start();                                    // инициируем список Моделей, известных TSmatch
            Docs rule = Docs.getDoc(Decl.MATCHING_RULES);   // инициируем список Правил Matcher'a
            for (int i = 4; i<=rule.Body.iEOL(); i++)
                { if (rule.Body[i, Decl.RULE_NAME] != null) Rules.Add(new Rule(rule, i)); }
            Log.exit();
        }
        public static void UseRules()
        {
            Log.set("UseRules");
            Docs Report = Docs.getDoc(Decl.SH3_REPORT);
            int nstr = 0;
            foreach(var gr in Mod.Groups)
            {
                bool found = false;
                object[] obj = new object[Report.Body.iEOC()];
                obj[0] = nstr; obj[1] = gr.mat; obj[2] = gr.prf;
                obj[3] = gr.lng; obj[4] = gr.wgt; obj[5] = gr.vol;
                foreach (var r in Rules)
                {
//                    new Log(@"разбираем Правило " + r.name + "(" + r.text + ")");
                    RuleParser(r.text);
                    if (isRuleApplicable(r.text, gr.mat, gr.prf))
                    {   //--- определим по элементу Groups, применять ли Правило?
                        if (SearchInComp(Docs.getDoc(r.doc), nstr, gr.mat, gr.prf))
                        {
                            found = true;
                            foreach (var ok in OKs)
                                if( ok.gr_nstr == nstr) ok.okToObj(ok, ref obj, 6);
                        }
                    }
                }
                if (found) Report.Body.AddRow(obj);
                else Report.Body.AddRow(OK.clearObj(ref obj, 6));              
                nstr++;
            }
            Report.isChanged = true;
            Docs.saveDoc(Report);
            Log.exit();
        }
        /// <summary>
        /// isRuleApplicable(rule, mat, prf) - возвращает true, если Правило rule применимо
        ///                                    к Материалу mat и Профилю prf
        /// </summary>
        /// <param name="rule">текст Правила</param>
        /// <param name="mat">Материал</param>
        /// <param name="prf">Профиль</param>
        /// <returns>true, если Правило применимо</returns>
        static bool isRuleApplicable(string rule, string mat, string prf)
        {
            return Lib.IContains(RuleMatList, mat) && Lib.IContains(RulePrfList, prf);
        }
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
        /// <journal> декабрь 2015 - январь 2016
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
                rVal = @"#\d+", rWild = @"\*.*?\d+",                  // #- и *- параметры 
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
        /// <journal> 12.2.2016 PKh <\journal>
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
        /// SearchInComp(doc, nstr, mat, prf) - поиск комплектующих mat и prf в doc 
        /// </summary>
        /// <param name="doc">файл компонентов - быза поставщика</param>
        /// <param name="nstr">номер строки в Groups</param>
        /// <param name="mat">Материал из строки nstr в Groups</param>
        /// <param name="prf">Профиль из строки nstr в Groups</param>
        /// <returns></returns>
        /// <journal> дек 2015 - фев 2016 - пререлизы
        /// 1.3.15 - обработка двух *параметров
        /// </journal>
        public static bool SearchInComp(Docs doc, int nstr, string mat, string prf)
        {
            Log.set("SearchInComp");
            bool found = false;
            mat = mat.ToUpper(); prf = prf.ToUpper();
            List<int> matPars = GetPars(mat);
            List<int> prfPars = GetPars(prf);
            Cmp.getComp(doc);
            //-- если в Правиле есть * - используем заготовку с любым значением соотв.параметра
            for (int nComp = doc.i0, iComp = 0; nComp <= doc.il; nComp++, iComp++)
            {
                string s = Cmp.Comps[iComp].description;
                if (string.IsNullOrWhiteSpace(s)) continue;
                if (!Lib.IContains(RuleMatList, mat)) continue;
                if (!Lib.IContains(RulePrfList, prf)) continue;
                List<int> CompPars = GetPars(s.ToUpper());
                for (int i = 0, iW = 0; i < prfPars.Count; i++)          // все имеющиеся в элементе..
                {                                               //..модели параметры prf должны ..
                    if (iW < RuleWildPars.Count)
                    {
                        if (i == RuleWildPars[iW])
                        {
                            // * - Wild !!
                            found = true;
                            break;

                            if (i == prfPars.Count - iW - 1) found = true;
                            iW++;
                        }
                    }
                    if (prfPars[i] != CompPars[i]) break;       //..совпадать с параметрами Comp..
                    if (i == prfPars.Count - 1) found = true;   //..если совпадают -> found = true
                }
                if (found)
                {
                    if (isOKexist(nstr)) break;     // если уже есть соответствие в OKs - NOP
                    // обработаем *параметры
                    double lng = Mod.Groups[nstr].lng / 1000; //приводим lng к [м]
                    double v = Mod.Groups[nstr].vol;        //объем в [м3]
                    double w = Mod.Groups[nstr].wgt;        //вес в [т]

                    if (w == 0)
                    {
                        w = v * 7.85 * 1000;   // уд.вес стали в кг
                    }
                    w *= 1 + RuleRedundencyPerCent / 100;     // учтем запас в % из Правила
                    double? p = Cmp.Comps[iComp].price * w / 1000;
                    OKs.Add(new OK(nstr, s, doc, nComp, w, p));
                    break;
                }
            } //end foreach Comp
            Log.exit();
            return found;
        }
    } // end class Matcher
} // end namespace Matcher
