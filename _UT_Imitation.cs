using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lib = match.Lib.MatchLib;
using Comp = TSmatch.Component.Component;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using Rule = TSmatch.Rule.Rule;
using Mod = TSmatch.Model.Model;
using SType = TSmatch.Section.Section.SType;
using DP = TSmatch.DPar.DPar;
using CS = TSmatch.CompSet.CompSet;

namespace TSmatch.Unit_Tests.Imitation
{
    public class _UT_Imitation
    {
        /// <summary>
        /// Имитация коллекции элементов из САПР - Dictionary<guid, ElmAttSet> Elements
        /// </summary>
        internal Dictionary<string, Elm> IM_Elements()
        {
            Dictionary<string, Elm> elements = new Dictionary<string, Elm>();
            string mat = "c245", matType = "Steel"
                , matB = "B20", matBtype = "Concrete";
            string prf0 = "1900x1600", prf1 = "l20x5", prf2 = "l25x8";
            string id1 = "MyId1", id2 = "MyId2", id3 = "MyId3", id4 = "MyId4", id5 = "MyId5";
            elements.Clear();
            Elm   e1 = new Elm(id1, mat, matType, prf1)
                , e2 = new Elm(id2, mat, matType, prf1)
                , e3 = new Elm(id3, mat, matType, prf2)
                , e4 = new Elm(id4, matB, matBtype, prf0)
                , e5 = new Elm(id5, matB, matBtype, prf0);
            elements.Add(e1.guid, e1);
            elements.Add(e2.guid, e2);
            elements.Add(e3.guid, e3);
            elements.Add(e4.guid, e4);
            elements.Add(e5.guid, e5);
            Assert.AreEqual(elements.Count, 5);
            return elements;
        }

        /// <summary>
        /// Имитация группировки из ElmAttSet.Group
        /// </summary>
        /// <param name="i">номер группы</param>
        internal ElmAttSet.Group IM_Group(string mat = "", string prf = "")
        {
            var model = IM_Model();
            var elements = IM_Elements();
            mat = Lib.ToLat(mat).ToLower().Replace(" ", "");
            prf = Lib.ToLat(prf).ToLower().Replace(" ", "");
            ElmAttSet.Group result = null;
            model.setElements(elements.Values.ToList());
            model.getGroups();
            foreach(var gr in model.elmGroups)
            {
                if (mat != "" && gr.mat != mat) continue;
                if (prf != "" && gr.prf != prf) continue;
                result = gr;
            }
            if (result == null) Assert.Fail();
            return result;
        }
        internal ElmAttSet.Group IM_Group(string str)
        {
            var model = IM_Model();
            var elements = IM_Elements();
            model.setElements(elements.Values.ToList());
            model.getGroups();
            string text = Lib.ToLat(str).ToLower();
            foreach(var gr in model.elmGroups)
            {
                if (gr.mat.Contains(text)) return gr;
            }
            return null;
        }

        /// <summary>
        /// Имитация модели из модуля Models
        /// </summary>
        internal Mod IM_Model()
        {
            return new Mod("UT_Mod", "MyDir", "noIFC", "my_made"
                , "my_phase", "myMD5");
        }

        /// <summary>
        /// Имитация Rule
        /// </summary>
        internal Rule.Rule IM_Rule(string str = "M:C245; Prf: Уголок=L*")
        {
            CS cs = IM_CompSet();
            Rule.Rule r = new Rule.Rule(str, cs);
            return r;
        }

        /// <summary>
        /// Имитация CompSet
        /// </summary>
        internal CS IM_CompSet(string str = "M:1;Prf:2;Des:3;Price:4")
        {
            var c1 = Im_Comp("C245", "Уголок 18x3", price: "832");
            var c2 = Im_Comp("C245", "Уголок 20x5", price: "1010");
            var c3 = Im_Comp("C245", "Уголок 25x8", price: "1234.56");
            var c4 = Im_Comp("C245", "Уголок 35x12", price: "2541.23");
            List<Comp> comps = new List<Comp> { c1, c2, c3, c4 };
            CS cs = new CS("Пример уголков", null, str, comps: comps);
            return cs;
        }

        public Comp Im_Comp(string mat, string descr, string price)
        {
            string str = "";
            str += "M:" + mat + ";";
            str += "Descr:" + descr + ";";
            //Profile = копия из Description только для отладки!!
            str += "Profile:" + descr + ";";
            str += "Price:" + price + ";";
            DP dp = new DP(str);
            Comp compDP = new Comp(dp, null);
            return compDP;
        }

        /// <summary>
        /// имитация списка синонимов - только для отладки
        /// </summary>
        /// <param name="i"></param>
        internal void IM_setRuleSynonyms(ref Rule.Rule rule, int i = -1)
        {
 //24/3           if (i < 0) { rule.synonyms = null; return; }
            var anglePrf = new List<string> { "l", "Уголок равнопол." };
            var synonyms = new Dictionary<SType, List<string>>();
            rule.synonyms = synonyms;
            List<string> syns = new List<string>();
            foreach (string syn in anglePrf)
            {
                syns.Add(Lib.ToLat(syn).ToLower().Replace(" ", ""));
            }
            rule.synonyms.Add(SType.Profile, syns);
        }
    }
} //end namespace UnitTests.Imitation
