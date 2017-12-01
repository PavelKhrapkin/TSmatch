/*=================================
* Handler Unit Test 1.12.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Comp = TSmatch.Component.Component;
using CS = TSmatch.CompSet.CompSet;
using DP = TSmatch.DPar.DPar;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using ElmGr = TSmatch.Group.Group;
using FileOp = match.FileOp.FileOp;
using MH = TSmatch.Handler.Handler;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Mtch;
using SR = TSmatch.SaveReport.SavedReport;
using Supl = TSmatch.Suppliers.Supplier;
using TS = TSmatch.Tekla.Tekla;

namespace TSmatch.Handler.Tests
{
    [TestClass()]
    public class UT_Handler
    {
        Boot boot = new Boot();
        Mod mod = new Mod();
        UT_TSmatch._UT_MsgService U = new UT_TSmatch._UT_MsgService();

        [TestMethod()]
        public void UT_Hndl()
        {
            //-- Assign: подготавливаем все необходимое для Hndl-
            //.. mod.elements и mod.elmGroups, инициируем Rules с загрузкой прайс-листов
            boot.Init();
            var sr = new _SR();
            mod = sr.SetModel(boot);
            mod.elements = sr.Raw(mod);
            List<Elm> elmCopy = new List<Elm>();
            foreach (Elm elm in mod.elements) elmCopy.Add(elm);
            for (int i = 0; i < elmCopy.Count; i++) Assert.AreEqual(elmCopy[i], mod.elements[i]);
            int cnt = mod.elements.Count;
            string MD5 = mod.getMD5(mod.elements);
            Assert.IsTrue(cnt > 0);
            string cMD5 = mod.getMD5(elmCopy);
            Assert.AreEqual(cMD5, MD5);
            if (mod.Rules == null || mod.Rules.Count == 0)
            {
                sr._GetSavedRules(mod);
            }
            var mh = new MH();
            Mtch mtsh = new Mtch(mod);

            mh.Hndl(ref mod);

            // проверка, что elements не испортились
            foreach (var gr in mod.elmGroups) cnt -= gr.guids.Count();
            Assert.AreEqual(0, cnt);
            Assert.AreEqual(mod.elements.Count, elmCopy.Count);
            for (int i = 0; i < elmCopy.Count; i++) Assert.AreEqual(elmCopy[i], mod.elements[i]);
            string newMD5 = mod.getMD5(mod.elements);
            string copyMD5 = mod.getMD5(elmCopy);
            Assert.AreEqual(mod.getMD5(mod.elements), MD5);

            // проверка наличия compDescription, sCS, sSupl и totalPrice в группах
            foreach (var gr in mod.elmGroups)
            {
                if (gr.totalPrice == 0) continue;
                Assert.IsTrue(gr.compDescription.Length > 0);
                Assert.IsTrue(gr.SupplierName.Length > 0);
                Assert.IsTrue(gr.CompSetName.Length > 0);
            }

            //Hndl performance test -- 180 sec for 100 cycles ОНХП модель 1124 элемента
            //                      -- 20,4 sec 1 cycle модель "Навес над трибунами" 7128 э-тов
            int nLoops = 1;
            DateTime t0 = DateTime.Now;
            for (int i = 0; i < nLoops; i++)
            {
                mh.Hndl(ref mod);
            }
            TimeSpan ts = DateTime.Now - t0;
            var secHndl = ts.TotalSeconds / nLoops;
            Assert.IsTrue(secHndl > 0.0);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Pricing()
        {
            boot.Init();
            mod = mod.sr.SetModel(boot);
            double priceExcel = mod.total_price;    // price from Excel

            // test 0: по одной или более групп match найден.
            mod.mh.Pricing(ref mod);
            Assert.IsTrue(mod.matches.Count > 0);
            if (mod.name == "Chasovnya+lepestok")
            {
                bool c235found = false;
                foreach (var r in mod.Rules)
                {
                    if (!r.text.Contains("235")) continue;
                    c235found = true;
                    break;
                }
                Assert.IsTrue(c235found);
            }

            // test 1: посчитана общая цена проекта
            double totalPrice = 0;
            foreach(var match in mod.matches)
            {
                totalPrice += match.group.totalPrice;
            }
            Assert.IsTrue(totalPrice > 1000);
            priceExcel = Math.Round(priceExcel / 1000000, 1);
            totalPrice = Math.Round(totalPrice / 1000000, 1);
            Assert.AreEqual(totalPrice, priceExcel);

            if (mod.name == "ONPZ-RD-ONHP-3314-1075_1.001-CI_3D_Tekla")
            {
                Assert.AreEqual(21, mod.matches.Count);
                // в Excel 8117835,38862033 rub
                double x = Math.Round(8117835.38862033 / 1000000, 1) ;
                Assert.AreEqual(8.1 , x);
                double p = Math.Round(mod.total_price / 1000000, 1);
                Assert.AreEqual(6.1, p);
            }

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_getGrps()
        {
            var mh = new MH();
            var sr = new SR();
            if (boot.isTeklaActive) mod.dir = TS.GetTeklaDir(TS.ModelDir.model);
            else mod.dir = boot.ModelDir;

            mod.elements = sr.Raw(mod);
            string md5 = mod.getMD5(mod.elements);
            Assert.AreEqual(32, md5.Length);

            var grp = mh.getGrps(mod.elements);
            Assert.IsTrue(grp.Count > 0);
            string pricing_md5 = mod.get_pricingMD5(grp);
            Assert.AreEqual(32, pricing_md5.Length);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_PriceGr_Msg()
        {
            // Assign
            boot.Init();
            Rule.Rule rule = new Rule.Rule();
            rule.sSupl = "СтальХолдинг";
            rule.sCS = "Полоса";
            rule.text = "М: C245=C255 ; Профиль: Полоса горячекатаная = PL = — *x*;";
            ElmGr gr = new ElmGr();
            gr.SupplierName = rule.sSupl;
            gr.guids = new List<string>() { "guid1", "guid2" };

            // test 1: Msg.F("Rules not initialyzed") English
            string s = sub_PriceGr(mod, gr, "en");
            Assert.AreEqual("Rules in Model were not initialyzed", s);

            // test 2: Msg.F("Rules not initialyzed") Russian
            s = sub_PriceGr(mod, gr, "ru");
            Assert.AreEqual("Не инциированы правила модели",  s);

            // test 3: Rules initialyzed, works with CompSet and Components, Rule, MsgF Wrong LoadDescriptor
            gr.Prf = "I20"; gr.prf = "i20";
            rule.text = "Профиль: Балка =I*";
            string comp_txt = "Балка 20";
            rule.ruleDP = new DPar.DPar(rule.text);
            rule.synonyms = rule.RuleSynParse(rule.text);
            //           var syns = rule.synonyms[Section.Section.SType.Profile].ToList();
            List<Comp> comps = new List<Comp>()
            {
                new Comp(new DP("Prf:I10; Price:23456")),
                new Comp(new DP("Prf:I20; Price:34567"))
            };
            Supl supl = new Supl("СтальХолдинг", init: false);
            string LoadDescriptor = "M:1; опис:3; профиль:2; цена: 4; Ед: руб/т";
            CS cs = new CS("Балка", supl, LoadDescriptor, comps);
            Comp comp = new Comp();
            comp.compDP = new DP("Prf: " + comp_txt);
            mod.Rules.Add(rule);
            rule.CompSet = cs;
            s = sub_PriceGr(mod, gr, "en", _prefix:"Msg.W: ");
            Assert.AreEqual("CompSet_wrong_LoadDescriptor", s);

            FileOp.AppQuit();
        }

        private string sub_PriceGr(Mod mod, ElmGr gr, string sLang="en", string sev = "F", string _prefix="")
        {
            U.SetLanguage(sLang);
            string result = "", prefix = _prefix;
            if(string.IsNullOrEmpty(prefix)) prefix = "Msg." + sev + ": [Handler.PriceGr]: ";
            try { mod.mh.PriceGr(mod, gr); }
            catch (Exception e)
            {
                if (e.Message.IndexOf(prefix) == 0)
                    result = e.Message.Substring(prefix.Length);
            }
            return result;
        }


        [TestMethod()]
        // сравниваем результат PriceGr с тем, что записано в TSmatchINFO.xlsx/Report
        // группа за группой
        public void UT_PriceGr_Native()
        {
            boot.Init();
            mod = mod.sr.SetModel(boot);
            mod.sr.GetSavedRules(mod, init: true);
            var Rules = mod.Rules.ToList();

            // специально для первой же незаметчиваемой группы --30
            var nomatch = mod.mh.PriceGr(mod, mod.elmGroups[12]);


            //Act
            foreach (var gr in mod.elmGroups)
            {
                double priceExel = gr.totalPrice;
   //             int ind = Rules.FindIndex(x => x.sSupl == gr.SupplierName && x.sCS == gr.CompSetName);
                var mtch = mod.mh.PriceGr(mod, gr);
                Assert.AreEqual(Round(priceExel), Round(mtch.group.totalPrice));
                if (mtch.ok.ToString() != "Match") continue;
                Assert.AreEqual(gr.SupplierName, mtch.rule.sSupl);
                Assert.AreEqual(gr.CompSetName, mtch.rule.sCS);

                Assert.AreEqual(gr.totalPrice, mtch.group.totalPrice);
                Assert.AreEqual(gr.SupplierName, mtch.group.SupplierName);
                Assert.AreEqual(gr.CompSetName, mtch.group.CompSetName);
                Assert.AreEqual(gr.mat, mtch.group.mat);
                Assert.AreEqual(gr, mtch.group);
            }

            FileOp.AppQuit();
        }

        private double Round(double v) { return Math.Round(v, 2); }
#if old //24/5 move to UT_ModelHandle
        [TestMethod]
        public void UT_Model_getGroup()
        {
            var Im = new IMIT();
            Mod model = Im.IM_Model();
            Dictionary<string, Elm> elements = Im.IM_Elements();
            string id1 = "MyId1", id2 = "MyId2";
            string mat = elements[id1].mat;
            string prf = elements[id1].prf;
            List<string> guids = new List<string> { id1, id2 };

            ElmAttSet.Group gr = new ElmAttSet.Group(elements, mat, prf, guids);

            Assert.AreEqual(gr.totalPrice, elements[id1].price + elements[id2].price);

            model.setElements(elements.Values.ToList());
            model.getGroups();
            Assert.AreEqual(model.elmGroups.Count, 3);
            Assert.AreEqual(model.elmGroups[0].mat, mat);
            Assert.AreEqual(model.elmGroups[0].prf, prf);
            Assert.AreEqual(model.elmGroups[0].totalPrice, 0);
            Assert.AreEqual(model.elmGroups[0].guids.Count, 2);
            Assert.AreEqual(model.elmGroups[1].guids.Count, 1);
            var gds0 = model.elmGroups[0].guids;
            Assert.AreEqual(gds0[0], id1);
            Assert.AreEqual(gds0[1], id2);
            Assert.AreEqual(model.elmGroups[1].totalPrice, 0);

            var grB = Im.IM_Group("B20");
            Assert.AreEqual(grB.mat, "b20");
            Assert.AreEqual(grB.prf, "1900x1600");
            Assert.AreEqual(grB.guids.Count, 2);
        }
#endif // 24/5 moveto UT_ModelHandle
        }
    class _SR : SR
    {
        internal Mod _GetSavedRules(Mod model)
        {
            return GetSavedRules(model, init: true);
        }
    } // end interface class _SR for access to SavedReport method
}