/*----------------------------------------------------------------------------------------------
 * Rule.cs -- Rule, which describes how to find Components used in Model in Supplier's price-list
 *
 * 24.5.2017 П.Храпкин
 *
 *--- History ---
 * 17.10.2016 code file created from module Matcher
 *  2.05.2017 FingerPrint references removed
 * 19.05.2017 public DateTime Rule.Date
 * 24.05.2017 get
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
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;

using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;
using Comp = TSmatch.Component.Component;
using CmpSet = TSmatch.CompSet.CompSet;
using Supl = TSmatch.Suppliers.Supplier;
using DP = TSmatch.DPar.DPar;
using Sec = TSmatch.Section.Section;
using SType = TSmatch.Section.Section.SType;
using TSmatch.Section;

namespace TSmatch.Rule
{
    public class Rule : IEquatable<Rule>	// структура, описывающая правило работы Matcher'а
    {
        public static readonly ILog log = LogManager.GetLogger("Rule");

        public DateTime date;               //дата и время записи Правила
        private int _id { get; set; }
        public readonly string name;        //название правила
        public readonly string type;        //тип правила
        public readonly string text;        //текст правила

        //---- references to other classes - price-list conteiners
        public readonly CmpSet CompSet;     //список компонентов, с которыми работает правило
        public readonly Supl Supplier;      //Поставщик
        public readonly DP ruleDP;          //identifiers of Materials, Profile, and others
        public Dictionary<SType, List<string>> synonyms = new Dictionary<SType, List<string>>();

        public double RuleRedundencyPerCent = 0.0;  //коэффициент избыточности, требуемый запас по данному материалу/профилю/Правилу
        private string sSupl;
        private string sCS;
        private string sR;

        public Rule(Docs doc, int i)
        {
//24/5            name = (string)doc.Body[i, Decl.RULE_NAME];
//24/5            type = (string)doc.Body[i, Decl.RULE_TYPE];
            text = Lib.ToLat((string)doc.Body[i, Decl.RULE_RULETEXT]);
            synonyms = RuleSynParse(text);
            ruleDP = new DP(text);  // template for identification
            string csName = (string)doc.Body[i, Decl.RULE_COMPSETNAME];
            string suplName = (string)doc.Body[i, Decl.RULE_SUPPLIERNAME];
            Supplier = new Suppliers.Supplier(suplName);
            CompSet = new CmpSet(csName, Supplier);
        }
        // параметр doc не указан, по умолчанию извлекаем Правила из TSmatch.xlsx/Rules
        public Rule(int n) : this(Docs.getDoc(Decl.TSMATCHINFO_RULES), n) { }
#if DEBUG
        // 27/3/2017 пока - for unit test purpases only
        public Rule(string str, CmpSet cs)
        {
            text = str;
            synonyms = RuleSynParse(str);
            ruleDP = new DP(str);
            CompSet = cs;
        }

        public Rule(DateTime _date, string sSupl, string sCS, string sR)
        {
            date = _date;
            text = sR;
            synonyms = RuleSynParse(text);
            ruleDP = new DP(text);
            Supplier = new Suppliers.Supplier(sSupl);
            CompSet = new CmpSet(sCS, Supplier);
        }

        private Dictionary<SType, List<string>> RuleSynParse(string str)
        {
            var Syns = new Dictionary<SType, List<string>>();
            string[] sections = str.Split(';');
            foreach (var s in sections)
            {
                List<string> synLst = new List<string>();
                Section.Section sect = new Section.Section(s);
                string[] strs = sect.body.Split('=');
                foreach (string tx in strs)
                {
                    int indxPar = tx.IndexOf('*');
                    string stx;
                    if (indxPar < 0) stx = tx;
                    else stx = tx.Substring(0, indxPar);
                    synLst.Add(stx);
                }
                if (synLst.Count > 1) Syns.Add(sect.type, synLst);
            }
            return Syns;
        }
#endif

        public bool Equals(Rule other)
        {
            if (other == null) return false;
            return (this._id == other._id);
        }

        internal static List<string> getList(string name)
        {
            List<string> result = new List<string>(); 
            Docs doc = Docs.getDoc(Decl.TSMATCHINFO_RULES);
            for(int i = doc.i0; i <= doc.il; i++)
            {
                string suplName = doc.Body.Strng(i, Decl.RULE_SUPPLIERNAME);
                if (suplName != name) continue;
                result.Add(suplName);
            }
            return result;
        }
#if OLD
        /// <summary>
        /// isRuleApplicable(mat, prf) - return true when parsed Rule is applicable to mat and prf values
        /// </summary>
        /// <param name="mat">Material in Model</param>
        /// <param name="prf">Profile in Model</param>
        /// <returns>true, if Rule after Parcing mentioned mat and prf</returns>
        /// <history>9.4.2016
        /// 29.12.2016 use FingerPrint
        /// </history>
        /// <TODO>29.12.16 implement with FP</TODO>
        internal bool isRuleApplicable(string mat, string prf)
        {
            bool ok= false;
//29.12.16            ok = Lib.IContains(RuleMatList, mat) && Lib.IContains(RulePrfList, prf);
            return ok;
        }

        ///<summary>        /// private RuleParser(string) - Parse string text of Rule to fill Rule parameters.
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
        /// <history> декабрь 2015 - январь 2016 предистория
        /// 19.2.16 - #параметры
        /// 29.2.16 - *параметры
        /// 17.10.16 - перенос в Rule class
        /// 28.12.16 - введены Параметры вида {..} правила. Остальные типы правил пока не разбираем
        /// 25.03.17 - переписал с использованием Section 
        /// </history>
        public Dictionary<SType, FP> Parser(FP.type _type, string text)
        {
            Log.set("Rule.Parser(" + _type + ", " + text);
            Dictionary<SType, FP> result = new Dictionary<SType, FP>();
            string[] sections = Lib.ToLat(text).ToLower().Split(';');
            foreach (string sec in sections)
            {
                Sec txSec = new Sec(sec);
                if (txSec.type == SType.NOT_DEFINED) continue;
                FP fp = new FP(_type, txSec.body);
                result.Add(txSec.type, fp);
            }
            Log.exit();
            return result;
        }

        private void RuleLogInfo()
        {
            log.Info("----- Parse Rule (\"" + text + "\") -----");
//            this.ruleFPs(x => x.);
            //if (matFP != null) log.Info("MatFP:" + matFP.strINFO());
            //if (prfFP != null) log.Info("PrfFP:" + prfFP.strINFO());
        }

        ///////////////////////////// <summary>
        ///////////////////////////// SectionParse(string) - parse Section - part of the Rule text. Result filled in Rule field
        ///////////////////////////// Lists Synonims and Parameters. Regular Expressions used to parse input string.
        ///////////////////////////// </summary>
        ///////////////////////////// <param name="str">Part of the Rule string to be parsed</param>
        ///////////////////////////// <history> 12.2.2016 PKh
        ///////////////////////////// 19.10.2016 re-done from previous static attParse
        ///////////////////////////// 23.10.2016 bug fix, Section header is parsing now in this module
        ///////////////////////////// 23.11.2016 Cost Section parse for the Volume dependent calcalation
        /////////////////////////////  2.12.2016 RecognizeSection
        ///////////////////////////// <\history>
        //////////////////////////private void SectionParse(string str)
        //////////////////////////{
        //////////////////////////    Section section = RecognyseSection(ref str);

        //////////////////////////    switch (section)
        //////////////////////////    {
        //////////////////////////        case Section.Material:  matFP = new FP(FP.type.Rule, str);
        //////////////////////////            break;
        //////////////////////////        case Section.Profile:   prfFP = new FP(FP.type.Rule, str);
        //////////////////////////            break;
        /////// 2.1.17 ///////////        //case Section.Cost:      Parser(ref str, RuleCstList, RuleCstPar);
        //////////////////////////            //RuleCstPar = Parameter(ref str);
        //////////////////////////            //RuleCstList = Synonym(ref str);
        //////////////////////////            //break;
        //////////////////////////        ////default: Msg.F("Rule.SectionParse-- wrong Section recognition", section);
        //////////////////////////        ////    break;
        //////////////////////////    }
        //////////////////////////    ////////if (str != "") Log.FATAL("строка \"" + str + "\" разобрана не полностью");
        //////////////////////////}





        private void Parser(ref string str, List<string> synonym, Dictionary<string, string> parametr)
        {
            //--выделение параметров из строки str, как регулярных выражений PARAM
            const string PARAM = @"(?<param>(\$|p|р|п|P|Р|П)\w*\d)"; //параметры в Правилах
//            const string PAR_DELIM = @"(x|#)";
            if (str.Length == 0) return;
//12/12            string[] parametrs = Regex.Split(str, PARAM);
            foreach (var par in Regex.Split(str, PARAM))
            {
                if (string.IsNullOrEmpty(par)
                    || string.IsNullOrWhiteSpace(par)
                    || !IsMtch(par, PARAM)) continue;
                parametr.Add(par.ToUpper(), "");
            }
            //-- оформление списка синонимов по параметрам в Dictionary<str,str>parametr
            const string DELIMETR = @"(${must}|,|=| |\t|\n|\*|x)";
            string[] subStr = Regex.Split(str, DELIMETR);
            foreach (var s in subStr)
            {
                if (string.IsNullOrEmpty(s)) continue;
                if (!string.IsNullOrWhiteSpace(s) && !Regex.IsMatch(s, DELIMETR))
                {
                    string tmp = s;
                    foreach (var par in parametr)
                    {
                        if (!tmp.Contains(par.Key)) continue;
                        int fr = tmp.IndexOf(par.Key);
                        tmp = tmp.Remove(fr, par.Key.Length);
                    }
                    synonym.Add(tmp);
                }
                str = str.Replace(s, "");
            }
        }

        /// <summary>
        /// Parse Section for the sub-strings - synonims, and return them as List<string> separated by Delimenters
        /// </summary>
        /// <param name="str">string to be parsed</param>
        /// <returns>List of the synonyms</returns>
        /// <ToDo>
        /// 17.11 перенести определение ATT_DELIM сюда. Для этого надо убедиться Shft/F12, что ATT_DELIM используется только тут
        /// 9.12.16 в список синонимов помещать текст без $*1. Для этого Parametr() надо вызывать отсюда!
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
        private Dictionary<string, string> Parameter(ref string str)
        {
            const string PARAM = @"(?<param>(\$|p|р|п|P|Р|П)\w*\d)"; //параметры в Правилах
            const string PAR_DELIM = @"(x|#)";
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (str.Length == 0) return results;
            string[] parametrs = Regex.Split(str, PARAM);
            foreach (var par in parametrs)
            {
                if (string.IsNullOrEmpty(par)
                    || string.IsNullOrWhiteSpace(par)
                    || !IsMtch(par, PARAM)) continue;
                results.Add(par.ToUpper(), "");
            }
            return results;
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
#endif //OLD
    } // end class Rule
} // end namespace Rule
