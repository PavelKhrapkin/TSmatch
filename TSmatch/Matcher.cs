/*----------------------------------------------------------------------------------------------
 * Matcher -- module fulfill matching Group of the Elements with CompSet in accrding with Rule
 *
 * 4.6.2017 Pavel Khrapkin
 *
 *--- History ---
 * 2016 previous editions P.Khrapkin, A.Pass, A.Bobtsov
 *  5.12.2016 revision of module Matcher: class Mtch instead of OK
 *  3.03.2017 enum ok {Match, NoMatch, NoSection}
 *  7.03.2017 use Section module
 *  4.06.2017 fair and handle Exception in Component when cannot parse Rule or Group
 *  
 *  <ToDo> 2017.03.2 Matcher revision:
 *  - check Shft/F12 all referenced to Mtch
 *  2017.06.04 move part of methods from Components to Matcher
 *  
 * ---------  Mtch - class - the result of matching
 * 
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
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
//----- My modules and classes -----------
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Decl = TSmatch.Declaration.Declaration;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
// 30.11.2016 using Cmp = TSmatch.CompSet.Component.Component;
using CS = TSmatch.CompSet.CompSet;
using Comp = TSmatch.Component.Component;
using Rule = TSmatch.Rule.Rule;
using TSmatch.ElmAttSet;
using TSmatch.Rule;
using Msg = TSmatch.Message.Message;
using Sec = TSmatch.Section.Section;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using SType = TSmatch.Section.Section.SType;
using TSmatch.DPar;

namespace TSmatch.Matcher
{
    public class Mtch
    {
        public static readonly ILog log = LogManager.GetLogger("Matcher");

        public enum OK { Match, NoMatch, NoSection, NoGroup }

        public readonly OK ok = OK.NoGroup;
        public ElmAttSet.Group group;           //reference to the Group<material, profile> being matched
        public Component.Component component;   //used Component in Match the Rule and Group
        public Rule.Rule rule;                  //the rule, which manage the matching

        private static Mod model;

        public Mtch(Mod mod) { model = mod; }
        /// <summary>
        /// Mtch(gr, _rule) - check if Group gr is in match with rule
        ///    if Mtch.ok.Match - return Mtch.Component chousen from CompSet.Component
        ///    else ok.NoMatch
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="_rule"></param>
        public Mtch(ElmAttSet.Group gr, Rule.Rule _rule)
        {
            //28/6            gr.Elements  
            if (gr == null || gr.guids.Count < 1) return;
            ok = OK.NoMatch;
            group = gr;
            foreach (var comp in _rule.CompSet.Components)
            {
                bool found = false;
                try { found = comp.isMatch(gr, _rule); }
                catch { }
                if (!found) continue;
                //-- Component is found - fill Price for all Guids elemets
#if CHECK_MD5
                if (!OK_MD5()) Msg.AskFOK("corrupted MD5"); 
#endif
                ok = OK.Match;
                string priceStr;
                try { priceStr = comp.Str(SType.Price); }
                catch { Msg.F("Match: Bad Price descriptor", _rule.sSupl, _rule.sCS); }
#if CHECK_MD5
                if (!OK_MD5()) Msg.AskFOK("corrupted MD5");
#endif
                component = comp;
                gr.match = this;    //27/3!!
                rule = _rule;
#if CHECK_MD5
                if (!OK_MD5()) Msg.AskFOK("corrupted MD5");
#endif
                gr.totalPrice = getPrice(gr, rule.CompSet.csDP, comp.Str(SType.Price));
#if CHECK_MD5
                if (!OK_MD5()) Msg.AskFOK("corrupted MD5");
#endif
            }
        }
#if CHECK_MD5
        public bool OK_MD5()
        {
            string newMD5 = model.getMD5(model.elements);
            return model.MD5 == newMD5;
        }
#endif
        private double getPrice(ElmAttSet.Group group, DPar.DPar csDP, string priceStr)
        {
            double price = Lib.ToDouble(priceStr);
            foreach (var sec in csDP.dpar)
            {
                if (!sec.Key.ToString().Contains("UNIT_")) continue;
                switch (sec.Key)
                {
                    case SType.UNIT_Weight: // kg -> tonn
                        if (group.totalWeight == 0) return group.totalVolume * 7850;
                        return group.totalWeight / 1000 * price;
                    case SType.UNIT_Vol:    // mm3 -> m3
                        return group.totalVolume / 1000 / 1000 / 1000 * price;
                    case SType.UNIT_Length:
                        return group.totalLength * price;
                    case SType.UNIT_Qty:
                        return price;
                }
            }
            return 0;
        }
#if OLD //4/8/2017
        private double getPrice(Elm elm, DPar.DPar csDP, string priceStr)
        {
            double price = Lib.ToDouble(priceStr);
            foreach (var sec in csDP.dpar)
            {
                if (!sec.Key.ToString().Contains("UNIT_")) continue;
                switch (sec.Key)
                {
                    case SType.UNIT_Weight: // kg -> tonn
                        if (elm.weight == 0) return elm.volume * 7850;
                        return elm.weight / 1000 * price;
                    case SType.UNIT_Vol:    // mm3 -> m3
                        return elm.volume / 1000 / 1000 / 1000 * price;
                    case SType.UNIT_Length:
                        return elm.length * price;
                    case SType.UNIT_Qty:
                        return price;
                }
            }
            return 0;
        }

        /// <summary>
        /// проверка, соответствует ли строка str набору синонимов и параметров
        /// !допустимы str без параметров; при этом pars == null
        /// !ограничение: str должен начинаться с непустого фрагмента текста, иначе Fatal Error
        /// </summary>
        /// <param name="str"></param>
        /// <param name="txs"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        /// <description> --- ОПИСАНИЕ ИДЕИ --------------------------
        /// Правило - это шаблон вида "tx1<par1>tx2<par2>"
        /// Правило уже разбрано в методе Rule.RuleParse, то есть на входе
        /// - str строка для обработки, она берется из прайс-листа или модели в САПР,
        /// - List<string> txts - список фрагментов текста, он 
        /// - Dictionary<string,strins>pars -словарь параметров с еще не заполненными значениями,
        ///      то есть pars = { {{par.name},""},..,..}. isStrMatch заполняет pars.Values
        /// Разбор Правила и заполнение значений параметров в isStrMatch делается с применением
        /// технологии Regular Expression. Здесь использован код из Reverse String Format
        /// http://stackoverflow.com/questions/5346158/parse-string-using-format-template
        /// </description>
        //////////        internal bool isSectionMatch(string str, List<string> txs, ref Dictionary<string, string> pars)
        //////////        {
        //////////            bool ok = false;
        //////////            if (string.IsNullOrWhiteSpace(str)) goto Exit;
        //////////            string reg = "";
        //////////            foreach (var s in txs) reg += s + "*";
        //////////            ok = Regex.IsMatch(str, reg);
        //////////            if (!ok || pars == null) goto Exit;
        //////////            //-- fill pars.Values in Dictionary
        //////////            string pattern = "^" + Regex.Replace(reg, @"\*", "(.*?)") + "$";
        //////////            Regex r = new Regex(pattern);
        //////////            Match m = r.Match(str);
        //////////            string[] parNam = new string[pars.Keys.Count];
        //////////            pars.Keys.CopyTo(parNam, 0);
        ////////////            string[] parNam = pars.Keys.ToArray();
        //////////            if (parNam.Length != m.Groups.Count - 1) Msg.F("Err: inconsystant Rule text/parameters");
        //////////            pars.Clear();
        //////////            for (int i = 1; i < m.Groups.Count; i++)
        //////////            {
        //////////                string name = parNam[i - 1];
        //////////                string val = m.Groups[i].Value;
        //////////                val = val.Trim();
        //////////                pars.Add(name, val);
        //////////            }
        //////////Exit:       return ok;
        //////////        }
        /// <summary>
        /// isRuleMatch(group, rule) - check if rule could be applied with group of elements gr
        /// </summary>
        /// <desctiption>
        /// для TRUE нужно, чтобы Правило содержало Синонимы, допустимые для этой Группы.
        /// пустой список Синонимов означает "любое значение".
        /// если у элемента списка Синонимов есть параметры, их сопоставляют с данными Группы
        /// с помощью Регулярных выражений
        /// </desctiption>
        /// <param name="gr"></param>
        /// <param name="rule"></param>
        /// <returns>true if could be in match</returns>
        //////////////private bool isRuleMatch(ElmAttSet.Group gr, Rule.Rule rule)
        //////////////{
        //////////////    bool result = false;
        //////////////    throw new NotImplementedException();
        //////////////    return result;
        //////////////}

  //#region ------ test Matcher -----

        internal static void testMtch()
        {
            Log.set("testMtch");
            Mtch mtch = new Mtch();
            // 28/5            mtch.test_getSectionText();
            // 28/5            mtch.test_isSectionMatch();
            //////////////////mtch.test_Mtch_1();
            //// 28/3 ////////mtch.test_Mtch_2();
            //////////////////mtch.test_Mtch_3();
            Log.exit();
        }

        //////////////        private void test_getSectionText()
        //////////////        {
        //////////////            Log.set("test_getSectionTest(Section.Material, text");
        //28/5/////////////// 7/3/2017 /////////////////            TST.Eq(getSectionText(FP.Section.Material, "Профиль: L 20 x 5; M: C245; Price: 2690"), "c245");
        //////////////            Log.exit();
        //////////////        }

        //////////////private void test_isSectionMatch()
        //////////////{
        //////////////    Log.set("isSectionMatch(Section.Material, C245, rule.text)");

        //////////////    /////// 7/3/2017 ////            bool ok = isSectionMatch(FP.Section.Material, "C245", "Профиль: L * x * ст*; длина: * = * м; M: ст *;");

        //////////////    Log.exit();
        //////////////}

        private void test_Mtch_1()
        {
            Log.set(" test_Mtch_1: Rule 4 и Group<C255, L20x4>");
            Rule.Rule rule = new Rule.Rule(4);
            ElmAttSet.ElmAttSet el = new ElmAttSet.ElmAttSet(
                "ID56A7442F-0000-0D70-3134-353338303236",
                "C245", "Steel", "Уголок20X4", 0, 0, 0, 1000);
            Dictionary<string, ElmAttSet.ElmAttSet> els = new Dictionary<string, ElmAttSet.ElmAttSet>();
            els.Add(el.guid, el);
            List<string> guids = new List<string>(); guids.Add(el.guid);
            ElmAttSet.Group gr = new ElmAttSet.Group(els, "C245", "Уголок20X4", guids);
            Mtch match = new Mtch(gr, rule);
            //6/4/17            TST.Eq(match.ok == OK.Match, true);
            Log.exit();
        }
        void test_Mtch_2()
        {
            Log.set(" test_Mtch_2: Rule 15 и Group < B12,5 , 1900x1600 > ");
            rule = new Rule.Rule(15);
            ElmAttSet.ElmAttSet elB = new ElmAttSet.ElmAttSet(
                "ID56A7442F-0000-0D7B-3134-353338303236",
                "B12,5", "Concrete", "1900x1600", 0, 0, 0, 1000);
            Dictionary<string, ElmAttSet.ElmAttSet> els = new Dictionary<string, ElmAttSet.ElmAttSet>();
            els.Add(elB.guid, elB);
            List<string> guids = new List<string>(); guids.Add(elB.guid);
            var model = new Model.Model();
            model.setElements(els);
            model.getGroups();
            var gr = model.elmGroups[0];
            //6/4/17            TST.Eq(gr.guids.Count, 1);
            var match = new Mtch(gr, rule);
            //6/4/17           TST.Eq(match.ok == OK.Match, true);
            var cmp = match.component;
            //31/3            TST.Eq(cmp.fps[SType.Material].pars[0].par.ToString(), "b12,5");
            Log.exit();
        }
        void test_Mtch_3()
        {
            Log.set(" test_Mtch_3: Rule 5 и Group < C235, Pl30 >");
            rule = new Rule.Rule(5);
            ElmAttSet.ElmAttSet elm = new ElmAttSet.ElmAttSet(
                "ID56A7442F-0000-0D74-3134-353338303236",
                "C235", "Steel", "Pl30", 0, 0, 0, 1001);
            Dictionary<string, ElmAttSet.ElmAttSet> els
                = new Dictionary<string, ElmAttSet.ElmAttSet>();
            els.Add(elm.guid, elm);
            List<string> guids = new List<string>(); guids.Add(elm.guid);
            var model = new Model.Model();
            model.setElements(els);
            model.getGroups();
            var gr = model.elmGroups[0];
            //6/4/17           TST.Eq(gr.guids.Count, 1);
            //6/4/17           TST.Eq(gr.mat, "c235");
            //6/4/17TST.Eq(gr.prf, "pl30");
            var doc = Docs.getDoc("Полоса СтальхолдингM");
            var csDP = new Dictionary<SType, string>();
            //31/3            csFPs = rule.Parser(FP.type.CompSet, doc.LoadDescription);
            // 2/4            Comp comp1 = new Comp(doc, 2, csDP);
            // 2/4            Comp comp2 = new Comp(doc, 12, csDP);
            // 2/4            List<Comp> comps = new List<Comp> { comp1, comp2 };
            // 2/4            CS cs = new CS("test_CS", null, rule, doc.LoadDescription, comps);
            // 2/4            TST.Eq(cs.csDP.Count, 4);

            //////////////////////////TST.Eq(comp1.isMatch(gr, rule), false);
            //////////////////////////TST.Eq(comp2.isMatch(gr, rule), true);
            Log.exit();
        }
        ////////////////////            return;     //13/3 - заглушен остаток теста

        ////////////////////            //-- test environment preparation: set ElmAttSet.Group and Rule
        ////////////////////            el = new ElmAttSet.ElmAttSet("MyGuid", "B30", "Concrete", "", 0, 0, 0, 1000);
        ////////////////////            els = new Dictionary<string, ElmAttSet.ElmAttSet>();
        ////////////////////            els.Add(el.guid, el);
        ////////////////////            gr = new ElmAttSet.Group(els, "B30", null, guids);
        ////////////////////            rule = new Rule.Rule(15);

        ////////////////////            //-- empty gr: guids.Count = 0 || gr == null
        ////////////////////            TST.Eq(guids.Count, 0);

        ////////////////////            TST.Eq(match.ok == OK.NoGroup, true);
        // 28/3 ////////////            gr = null;
        ////////////////////            match = new Mtch(gr, rule);
        ////////////////////            TST.Eq(match.ok == OK.NoGroup, true);

        ////////////////////            //-- normal Mtch with 1 element with mat = "B30"
        ////////////////////            guids.Add(el.guid);
        ////////////////////            gr = new ElmAttSet.Group(els, "B30", null, guids);
        ////////////////////            match = new Mtch(gr, rule);
        ////////////////////            TST.Eq(match.ok == OK.Match, true);

        ////////////////////            //-- Mtch "C235" and "B30" -- should be OK.NoMatch
        //////////////////////            guids.Clear();
        ////////////////////            gr = new ElmAttSet.Group(els, "C235", null, guids);
        ////////////////////            match = new Mtch(gr, rule);
        ////////////////////            TST.Eq(match.ok == OK.NoMatch, true);
        //////////////////////            TST.Eq(match.price, 0.0);
        ////////////////////            Log.exit();
        ////////////////////        }

        //#endregion ------ test Matcher ------
        /* 5.12.2016 ревизия
        public struct OK    // структура данных, описывающая найденное соответствие..
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
              this.doc = _doc; this.nComp = _nComp;
              this.weignt = _w; this.price = _p;
          }
          public void okToObj(OK ok, ref object[] obj, int fr)
          {
              obj[fr++] = ok.strComp;
              obj[fr++] = ok.doc.name; obj[fr++] = ok.nComp;
              obj[fr++] = ok.weignt; obj[fr] = ok.price;
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
          List<TSmatch.Rule.Rule> Rules = new List<TSmatch.Rule.Rule>();
          Docs rule = Docs.getDoc(Decl.RULES);   // инициируем список Правил Matcher'a
          for (int i = 4; i <= rule.Body.iEOL(); i++)
          { if (rule.Body[i, Decl.RULE_NAME] != null) Rules.Add(new TSmatch.Rule.Rule(rule, i)); }
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
        /// 17.10.2016 - parse Model Rules
        /// </history>
        public static void UseRules(Mod mod)
        {
          Log.set("UseRules(" + mod.name + ")");
          int nstr = 0;                           // nstr - string number in Groupr
          foreach(var gr in mod.elmGroups)
          {
              foreach (var r in mod.Rules)
              {
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
        ///////////////////// <summary>
        ///////////////////// GetPars(str) разбирает строку раздела компонента или Правила, выделяя числовые параметры.
        /////////////////////         Названия материалов, профилей и другие нечисловые подстроки игнорируются.
        ///////////////////// </summary>
        ///////////////////// <param name="str">входная строка раздела компонента</param>
        ///////////////////// <returns>List<int>возвращаемый список найденых параметров</int></returns>
        //////////////////public static List<int> GetPars(string str)
        //////////////////{
        //////////////////    const string VAL = @"\d+";
        //////////////////    List<int> pars = new List<int>();
        //////////////////    string[] pvals = Regex.Split(str, Decl.ATT_DELIM);
        //////////////////    foreach (var v in pvals)
        //////////////////    {
        //////////////////        if (string.IsNullOrEmpty(v)) continue;
        //////////////////        if (Regex.IsMatch(v, VAL))
        //////////////////            pars.Add(int.Parse(Regex.Mtch(v, VAL).Value));
        //////////////////    }
        //////////////////    return pars;
        //////////////////}
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
              System.Text.RegularExpressions.Mtch val = Regex.Mtch(rule, rVal);
              int x = int.Parse(val.Value.Replace("#", ""));
              RuleValPars.Add(x);
              rule = rule.Replace(val.Value, "");
          }
          //--- get List of * parameters from the rule
          RuleWildPars.Clear();
          while (Regex.IsMatch(rule, rWild, RegexOptions.IgnoreCase))
          {
              System.Text.RegularExpressions.Mtch starPar = Regex.Mtch(rule, rWild);
              List<int> p = Lib.GetPars(starPar.Value);
              RuleWildPars.Add(p[0] - 1);
              rule = rule.Replace(starPar.Value, "");
          }
          //--- Redundency % handling
          while (Regex.IsMatch(rule, rRedundancy, RegexOptions.IgnoreCase))
          {
              System.Text.RegularExpressions.Mtch excess = Regex.Mtch(rule, rRedundancy, RegexOptions.IgnoreCase);
              System.Text.RegularExpressions.Mtch perCent = Regex.Mtch(excess.ToString(), rDec);
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
              List<int> matPars = Lib.GetPars(mat);
              List<int> prfPars = Lib.GetPars(prf);
              //-- если в Правиле есть * - используем заготовку с любым значением соотв.параметра
              for (int nComp = cs.doc.i0, iComp = 0; nComp <= cs.doc.il; nComp++, iComp++)
              {                                                       // все имеющиеся в элементе..
                  string s = cs.Components[iComp].description;        //..модели параметры prf должны ..
                  if (string.IsNullOrWhiteSpace(s)) continue;         //..совпадать с параметрами Comp..
                  if (!Lib.IContains(RuleMatList, mat)) continue;     //..если совпадают -> found = true
                  if (!Lib.IContains(RulePrfList, prf)) continue;
                  List<int> CompPars = Lib.GetPars(s.ToUpper());
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
        ////!! 21/6/2016 в отладку с ElmAttSet Group
        //                        double lng = Mod.Groups[nstr].lng / 1000; //приводим lng к [м]
        //                        double v = Mod.Groups[nstr].vol;        //объем в [м3]
        //                        double w = Mod.Groups[nstr].wgt;        //вес в [т]

        //                        if (w == 0)
        //                        {
        //                            w = v * 7.85 * 1000;   // уд.вес стали в кг
        //                        }
        //                        w *= 1 + RuleRedundencyPerCent / 100;     // учтем запас в % из Правила
        //                        double? p = cs.Components[iComp].price * w / 1000;
        //                        OKs.Add(new OK(nstr, s, cs.doc, nComp, w, p));
        //  //!! 21/6/2016 в отладку с ElmAttSet Group
                      break;
                  }
              } //end foreach Comp
          } // end if isRuleApplicable
          Log.exit();
          return found;
        } // end SearchInComp
        2016.12.05 revision */
#endif //#if OLD 8/5/2017
    } // end class Matcher
} // end namespace Matcher