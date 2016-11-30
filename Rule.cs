/*----------------------------------------------------------------------------------------------
 * Rule.cs -- Rule, which describes how to find Components used in Model in Supplier's price-list
 *
 * 17.10.2016 П.Храпкин
 *
 *--- History ---
 * 17.10.2016 code file created from module Matcher
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
 * Start()      - инициирует начальный запуск всех модулей системы TSmatch
 *    --- Вспомогательные методы - подпрограммы ---
 * RuleParsre(r.text) - синтаксический разбор Правила r.text
 * attParse(str, reg, lst, ref n) - разбор раздела Правила в строке str
 * GetPars(str) -public- разбирает строку- часть компонента или Правила, выделяя int параметры.
 * isRuleApplicable(r.text, gr.mat, gr.prf) - определяет, применимо ли Правило?
 * isOKexist(n) - возвращает true, если для группы n уже найдено соответствие в OKs
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;

using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;
using CmpSet = TSmatch.CompSet.CompSet;
using Supl = TSmatch.Suppliers.Supplier;
using System;

namespace TSmatch.Rule
{
    public class Rule : IEquatable<Rule>	// структура, описывающая правило работы Matcher'а
    {
        public static readonly ILog log = LogManager.GetLogger("Rule");

        private int _id { get; set; }
		public readonly string name;        //название правила
		public readonly string type;        //тип правила
		public readonly string text;        //текст правила
        //---- references to other classes
		public readonly CmpSet CompSet;     //список компонентов, с которыми работает правило
		public readonly Supl Supplier;      //Поставщик
        //---- fileds of the Rule filled by Parser method
        public List<string> RuleMatList = new List<string>();
        public List<string> RuleMatPar  = new List<string>();
        public List<string> RulePrfList = new List<string>();
        public List<string> RulePrfPar  = new List<string>();
        public List<string> RuleCstList = new List<string>();
        public List<string> RuleCstPar  = new List<string>();
        public readonly List<int> RuleValPars = new List<int>();
        public readonly List<int> RuleWildPars = new List<int>();
        public double RuleRedundencyPerCent = 0.0;  //коэффициент избыточности, требуемый запас по данному материалу/профилю/Правилу

        public Rule(Docs doc, int i)
        {
            log.Info("Constructor Rule(doc, i=" + i + ")");
            name = (string)doc.Body[i, Decl.RULE_NAME];
            type = (string)doc.Body[i, Decl.RULE_TYPE];
            text = Lib.ToLat((string)doc.Body[i, Decl.RULE_RULE]);
            string csName = (string)doc.Body[i, Decl.RULE_COMPSETNAME];
            string suplName = (string)doc.Body[i, Decl.RULE_SUPPLIERNAME];
            Supplier = Supl.getSupplier(suplName);
            CompSet = CmpSet.getCompSet( csName, Supplier );
            RuleParser(text);
        }
        // параметр doc не указан, по умолчанию извлекаем Правила из TSmatch.xlsx/Rules
        public Rule(int n) : this(Docs.getDoc(Decl.RULES), n) {}

        public bool Equals(Rule other)
        {
            if (other == null) return false;
            return (this._id == other._id);
        }
        ////////////////////public override int GetHashCode()
        ////////////////////{
        ////////////////////    return _id;
        ////////////////////}
        ////////////////////public List<Rule> AddUnq(Rule r)
        ////////////////////{
        ////////////////////    if(Lib.IContains(this, this._id))


        ////////////////////    this.Contains(r);
        ////////////////////    if (this.Contains(r)) return this;
        ////////////////////    r._hashCode = this.GetHashCode();
        ////////////////////    this.AddUnq(r);
        ////////////////////    return this;

        ////////////////////}
        /// <summary>
        /// isRuleApplicable(mat, prf) - return true when parsed Rule is applicable to mat and prf values
        /// </summary>
        /// <param name="mat">Material in Model</param>
        /// <param name="prf">Profile in Model</param>
        /// <returns>true, if Rule after Parcing mentioned mat and prf</returns>
        /// <history>9.4.2016</history>
        internal bool isRuleApplicable(string mat, string prf)
        {
            return Lib.IContains(RuleMatList, mat) && Lib.IContains(RulePrfList, prf);
        }

        ///<summary>
        /// private RuleParser(string) - Parse string text of Rule to fill Rule parameters.
        ///                              Regular Expression methodisa are in use.
        ///</summary>
        /// <описание>
        /// -------------------------- СИНТАКСИС ПРАВИЛ -----------------------
        /// RuleParser разбирает текстовую строку - правило, выделяя и возвращая в списках или полях,
        ///             входящих в состав класса Rule значения и списки, полученные из текста Правила.
        ///     ...... Части Правила, иначе - Разделы или Секции ......
        /// Разделы начинаются признаком раздела, например, "Материал:" и разделяются между собой знаком ';'.
        /// Заголовок раздела распознается по первым буквам 'M' (Материал), "Пр" ("Pr" для английского текста),
        /// 'C' (Cost или Стоимость) или 'Ц' (Цена) и завершается ':'. Поэтому
        ///             "Профиль:" = "П:" = "Прф:" = "п:" = "Prof:"
        /// Заглавные и строчные буквы эквивалентны, национальные символы переводятся в эквивалентные по начертанию
        /// знаки латинского алфавита, чтобы избежать путаницы между латинским Х и русским Х.
        /// Разделы Правила можно менять местами и пропускать; тогда они работают "по умолчанию".
        /// В теле раздела могут быть синонимы - альтернативные обозначения Материала или Профиля. Их синтаксис:
        ///             '=' означает эквивалентность. Допустимы выражения "C255=Ст3=сталь3".
        ///             ',' позволяет перечислять элементы и подразделы Правила.
        ///             ' ' пробелы, табы и знаки конца строки игнорируются, однако они могут служить
        ///                 признаком перечисления, так же, как пробелы. Таким образом, названия материалов
        ///                 или профилей можно просто перечислить - это эквивалентно '='
        /// В результате работы метода RuleParser списки альтернативных наименований для Материала, Профили и Прочего
        ///             RuleMatList - список допустимых альтернативных наименований материалов, с которыми работает Правило
        ///             RulePrfList - список альтернативных наименований Профиля
        ///             RuleCstList - список альтернативных наименований других разделов Правила
        ///     ...... Параметры ......
        /// Текст Раздела Правила может иметь т.н. Параметры - символические обозначения каких-либо значений.
        /// Например,"$p1" или просто "p1" или "Параметр235". Параметр начинается со знака'$' или 'p' и кончается цифрой,
        /// причем знак '$' можно опускать. Значение Параметра подставляется из атрибутов модели в САПР в порядке следования
        /// Параметров в Правиле. 
        ///
        ///     #параметры - величины в Правиле, которые соответствуют "значению" в тексте атрибута или прайс-листа,
        ///               например, номер колонки с весом или с ценой
        ///     *параметры - Wild параметр комплектующих; соответствующий элемент модели может иметь
        ///               любое значение, например, ширина листа, из которого нарезают полосы
        ///     Redunduncy% параметр - коэффициент запаса в процентах. Перед десятичным числом -
        ///               коэффициентом запаса - могут быть ключевые слова, возможно, сокращенные
        ///               (избыток, запас, отходы, Redundency, Excess, Waste) по русски или 
        ///               по английски заглавными или строчными буквами.
        /// </описание>
        /// <history> декабрь 2015 - январь 2016
        /// 19.2.16 - #параметры
        /// 29.2.16 - *параметры
        /// 17.10.16 - перенос в Rule class
        /// </history>
        /// <ToDo>23.11 реализовать разбор секции Cost</ToDo>
        enum Section { Material, Profile, Cost};
        private void RuleParser(string rule)
        {
            Log.set("RuleParser(\"" + rule + "\")");
            rule = Lib.ToLat(rule).ToUpper();

            string[] strs = rule.Split(';');
            foreach (var str in strs)
            {
                if (str.Length == 0) break;
                SectionParse(Section.Material, str, ref RuleMatList, ref RuleMatPar);
                SectionParse(Section.Profile, str, ref RulePrfList, ref RulePrfPar);
                SectionParse(Section.Cost, str, ref RuleCstList, ref RuleCstPar);
            }
            RuleTrace();
            Log.exit();
        }

        private void RuleTrace()
        {
            log.Info("----------- Parse Rule (\"" + text + "\") ---------------");
            string matSynonyms = "", matParams = "", prfSymonyms = "", prfParams = "";
            foreach (string syn in RuleMatList) matSynonyms += " " + syn;
            foreach (string par in RuleMatPar) matParams += " " + par;
            log.Info("Material");
            log.Info("\tSynonims:  " + matSynonyms);
            log.Info("\tParametrs: " + matParams);
            foreach (string syn in RulePrfList) prfSymonyms += " " + syn;
            foreach (string par in RulePrfPar) prfParams += " " + par;
            log.Info("Profile");
            log.Info("\tSynonims:  " + prfSymonyms);
            log.Info("\tParametrs: " + prfParams);
        }

        /// <summary>
        /// SectionParse(string, rSection) - parse Section - part of the Rule text. Result filled in Rule field
        /// Lists Synonims and Parameters. Regular Expressions used to parse input string.
        /// /// </summary>
        /// <param name="str">Part of the Rule string to be parsed</param>
        /// <param name="rSection">Regular Expression template of the Section header</param>
        /// <param name="synonym">List<string>of allowed alternatives - Material or Profile names</param>
        /// <param name="params">List<string>of Section parameters</param>
        /// <returns>List<string>List </returns>
        /// <history> 12.2.2016 PKh
        /// 19.10.2016 re-done from previous static attParse
        /// 23.10.2016 bug fix, Section header is parsing now in this module
        /// 23.11.2016 Cost Section parse for the Volume dependent calcalation
        /// <\history>
        private void SectionParse(Section sectn, string str, ref List<string>synonym, ref List<string> parameter)
        {
            const string rMat = "m.*:";         //for "Profile" abbreviation must be at least "Pr", or "Пр"
            const string rPrf = "(пp|pr).*:";   //.. to avoid mixed parse of russian 'р' in "Материал"
            const string rCst = "(c|ц).*:";     //Cost (or Цена) Section

            switch (sectn)
            {
                case Section.Material:
                    if (!IsMtch(str, rMat)) return;
                    log.Info("Section Material str=" + str);
                    str = RemMtch(str, rMat);
                    break;
                case Section.Profile:
                    if (!IsMtch(str, rPrf)) return;
                    log.Info("Section Profile str=" + str);
                    str = RemMtch(str, rPrf);
                    break;
                case Section.Cost:
                    if (!IsMtch(str, rCst)) return;
                    log.Info("Section Cost str=" + str);
                    str = RemMtch(str, rCst);
                    //this.RuleCstList = synonyms;
                    //this.RuleCstPar = parameters;
                    break;
                default: Msg.F("Rule.SectionParse-- wrong Section arg");
                    break;
            }
            parameter = Parameter(ref str);
            synonym = Synonym(ref str);
            if (str != "") Log.FATAL("строка \"" + str + "\" разобрана не полностью");
        }
        /// <summary>
        /// IsMtch(string str, string reg) - return TRUE, if str contains regular expression reg.
        /// </summary>
        /// <param name="str">string to check</param>
        /// <param name="reg">regular expression</param>
        /// <returns>true is str is in match with reg, ignoring letter case and with ToLat(str)</returns>
        private bool IsMtch(string str, string reg)
        {
            //str = Lib.ToLat(str).ToLower();
            //reg = Lib.ToLat(reg).ToLower();
            return Regex.IsMatch(str, reg, RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// RemMtch(str, reg) - remove substring matching regular expression reg from str
        /// </summary>
        /// <param name="str">string to be modified</param>
        /// <param name="reg">regular expression to seek in str</param>
        /// <returns>modified str</returns>
        private string RemMtch(string str, string reg)
        {
            ////str = Lib.ToLat(str).ToLower();
            ////reg = Lib.ToLat(reg).ToLower();
            Regex regHdr = new Regex(reg, RegexOptions.IgnoreCase);
            return regHdr.Replace(str, "");  // remove Section header
        }

        /// <summary>
        /// Parse Section for the sub-strings - synonims, and return them as List<string> separated by Delimenters
        /// </summary>
        /// <param name="str">string to be parsed</param>
        /// <returns>List of the synonyms</returns>
        /// <ToDo>
        /// 17.11 перенести определение ATT_DELIM сюда. Для этого надо убедиться Shft/F12, что ATT_DELIM используется только тут
        /// </ToDo>
        private List<string> Synonym(ref string str)
        {
            const string DELIMETR = @"(${must}|,|=| |\t|\n|\*|x)";
            List<string> result = new List<string>();
            string[] subStr = Regex.Split(str, DELIMETR);
            foreach (var s in subStr)
            {
                if (string.IsNullOrEmpty(s)) continue;
                if (!string.IsNullOrWhiteSpace(s) && !Regex.IsMatch(s, DELIMETR)) result.Add(s);
                str = str.Replace(s, "");
            }
            return result;
        }
        private List<string> Parameter(ref string str)
        {
            const string PARAM = @"(?<param>(\$|p|р|п|P|Р|П)\w*\d)"; //параметры в Правилах
            const string PAR_DELIM = @"(x|\*|#)";
            List<string> result = new List<string>();
            if (str.Length == 0) return result;
            string[] parametrs = Regex.Split(str, PARAM);
            foreach (var par in parametrs)
            {
                if(    string.IsNullOrEmpty(par)
                    || string.IsNullOrWhiteSpace(par)
                    || !IsMtch(par, PARAM)   ) continue;
                result.Add(par.ToUpper());
                str = str.Replace(par, "");
            }
            while (IsMtch(str, PAR_DELIM)) str = RemMtch(str, PAR_DELIM);
            return result;
        }
        /* 18.11.2016 --- попробую убрать весь разбор параметров в SectionParse
                    const string rM = "(m|м).*:", rP = "(п|p).*:",           // Материал и Профиль
                        rVal = @"#\d+", rWild = @"\*.*?\d+",                 // #- и *- параметры 
                        rDec = @"(\d+)|(\d.\d*)|(\d,\d)",                    // десятичные числа с точкой или запятой
                        rRedundancy = @"(изб|зaп|oтx|red|was|excess).*\d+%"; //Redundency|Waste|Excess|Отходы|Запас
                    Regex regM = new Regex(rM, RegexOptions.IgnoreCase);
                    Regex regP = new Regex(rP, RegexOptions.IgnoreCase);

                    string strMat = Lib.ToLat("Материал:");
                    string rMM = "m.*:";
                    string rPP = "(п|p).*:";
                    string rPR = "п.*:";
                    bool b = Regex.IsMatch(strMat, rMM, RegexOptions.IgnoreCase);
                    bool h = Regex.IsMatch(strMat, rPR, RegexOptions.IgnoreCase);

                    //--- get #value parameters from the rule, put them in RuleValPars List
                    while (Regex.IsMatch(rule, rVal, RegexOptions.IgnoreCase))
                    {
                        Match val = Regex.Match(rule, rVal);
                        int x = int.Parse(val.Value.Replace("#", ""));
                        RuleValPars.Add(x);
                        rule = rule.Replace(val.Value, "");
                    }
                    //--- get List of * parameters from the rule, put them into RuleWildPars List
                    while (Regex.IsMatch(rule, rWild, RegexOptions.IgnoreCase))
                    {
                        Match starPar = Regex.Match(rule, rWild);
                        List<int> p = Lib.GetPars(starPar.Value);
                        RuleWildPars.Add(p[0] - 1);
                        rule = rule.Replace(starPar.Value, "");
                    }
                    //--- Redundency % handling, put them into RuleRedandencyPerCent List
                    while (Regex.IsMatch(rule, rRedundancy, RegexOptions.IgnoreCase))
                    {
                        Match excess = Regex.Match(rule, rRedundancy, RegexOptions.IgnoreCase);
                        Match perCent = Regex.Match(excess.ToString(), rDec);
                        if (!double.TryParse(perCent.ToString(), out RuleRedundencyPerCent))
                            RuleRedundencyPerCent = 0.0;
                        rule = rule.Replace(excess.Value, "");
                    }
                    //--- Rule parsing by Sections (Material/Profile/Other). Разбор Правила по разделам
        */ //18.11.2016
           /* 18.11.2016
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
           */ ///////////////// 18.11.2016
              /* !!!!!!!!!!!!!!!!!!!!!!!!!!! 17.10.2016
                      /// <summary>
                      /// Start - initiate Rules - text lines, which describe match process
                      /// </summary>
                      /// <ToDo>
                      /// 1.10.2016
                      /// Вообще говоря, метод Start не нужен, как и общий список Rules, поскольку Правила в Модели считываются
                      /// непосредственно из TSmatch.xlsx/Rules. В дальнейшем надо:
                      /// -!- в классе Rules добавть поля RuleMatList, RulePrfList и пр со стр.270-276 и здесь парсировать Правила
                      /// </ToDo>
                      /// <history> Jan-2016
                      /// 19.2.16 - переписано обращение к Documents.Start с инициализацией массивов FileDir
                      /// 30.3.16 - Start other modules removed to Bootstrap
                      /// </history>
                      public static void Start()
                      {
                          Log.set("Matcher.Start");
                          List<Rule> Rules = new List<Rule>();
                          Docs rule = Docs.getDoc(Decl.RULES);   // инициируем список Правил Matcher'a
                          for (int i = 4; i<=rule.Body.iEOL(); i++)
                              { if (rule.Body[i, Decl.RULE_NAME] != null) Rules.Add(new Rule(rule, i)); }
                          Log.exit();
                      }

                      /// <summary>
                      /// UserRules(mod) - Apply Model mod Rules to create TSmatchINFO.xlsx/Report
                      /// </summary>
                      /// <param name="mod">Model to de handled</param>
                      /// <ToDo>
                      /// 1.10.2016
                      /// -!- убрать все обращения к Report и obj, вместо этого заполнять struct OK
                      /// </ToDo>
                      /// <history>10.3.2016
                      /// 15.3.2016 - get Rule list (Rules) from the Model mod
                      ///  3.4.2016 - adoption to the updated CompSet class
                      /// 30.9.2016 - Groups taken from mod
                      ///  1.10.2016 - вместо печати TSmatchINFO/Report заполняем struct OK в mod.elmGroups
                      /// </history>
                      public static void UseRules(Mod mod)
                      {
                          Log.set("UseRules(" + mod.name + ")");
                          int nstr = 0;                           // nstr - string number in Groupr
                          foreach(var gr in mod.elmGroups)
                          {
                              foreach (var r in mod.Rules)
                              {
                                  RuleParser(r.text);     // сейчас Правило r многократно разбирается каждый раз для каждой Группы..
                                                          // Возможно,списки Rule.List стоит перенести в класс Rule и обработать заранее
                                  SearchInComp(r.CompSet, nstr++, gr.mat, gr.prf);
                              }
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
               !!!!!!!!!!!!!!!!!!!! 17.10.2016 */
    } // end class Rule
} // end namespace Rule
