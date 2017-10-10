using TSmatch.Handler;
/*=================================
* Handler Unit Test 16.8.2017
*=================================
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

using FileOp = match.FileOp.FileOp;
using Boot = TSmatch.Bootstrap.Bootstrap;
using Msg = TSmatch.Message.Message;
using Mod = TSmatch.Model.Model;
using TS = TSmatch.Tekla.Tekla;
using MH = TSmatch.Handler.Handler;
using SR = TSmatch.SaveReport.SavedReport;
using Elm = TSmatch.ElmAttSet.ElmAttSet;
using ElmGr = TSmatch.Group.Group;
using Mtch = TSmatch.Matcher.Mtch;
using Comp = TSmatch.Component.Component;
using CS = TSmatch.CompSet.CompSet;
using DP = TSmatch.DPar.DPar;
using Supl = TSmatch.Suppliers.Supplier;

namespace TSmatch.Handler.Tests
{
    [TestClass()]
    public class UT_Handler
    {
        public Mod model;

        [TestMethod()]
        public void UT_Hndl()
        {
            var boot = new Boot();
            var sr = new _SR();
            model = sr.SetModel(boot);

            model.elements = sr.Raw(model);
            List<Elm> elmCopy = new List<Elm>();
            foreach (Elm elm in model.elements) elmCopy.Add(elm);
            for (int i = 0; i < elmCopy.Count; i++) Assert.AreEqual(elmCopy[i], model.elements[i]);
            int cnt = model.elements.Count;
            string MD5 = model.getMD5(model.elements);
            Assert.IsTrue(cnt > 0);
            string cMD5 = model.getMD5(elmCopy);
            Assert.AreEqual(cMD5, MD5);
            if (model.Rules == null || model.Rules.Count == 0)
            {
                sr._GetSavedRules(model);
            }
            var mh = new MH();
            Mtch mtsh = new Mtch(model);

            mh.Hndl(ref model);

            // проверка, что elements не испортились
            foreach (var gr in model.elmGroups) cnt -= gr.guids.Count();
            Assert.AreEqual(0, cnt);
            Assert.AreEqual(model.elements.Count, elmCopy.Count);
            for (int i = 0; i < elmCopy.Count; i++) Assert.AreEqual(elmCopy[i], model.elements[i]);
            string newMD5 = model.getMD5(model.elements);
            string copyMD5 = model.getMD5(elmCopy);
            Assert.AreEqual(model.getMD5(model.elements), MD5);

            // проверка наличия compDescription, sCS? sSupl и totalPrice в группах
            foreach (var gr in model.elmGroups)
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
                mh.Hndl(ref model);
            }
            TimeSpan ts = DateTime.Now - t0;
            var secHndl = ts.TotalSeconds / nLoops;
            Assert.IsTrue(secHndl > 0.0);

            // 

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_Pricing()
        {
            var boot = new Boot();
            Mod model = new Mod();
            model = model.sr.SetModel(boot);

            model.mh.Pricing(ref model);
            Assert.IsTrue(model.matches.Count > 0);
            if (model.name == "Chasovnya+lepestok")
            {
                bool c235found = false;
                foreach (var r in model.Rules)
                {
                    if (!r.text.Contains("235")) continue;
                    c235found = true;
                    break;
                }
                Assert.IsTrue(c235found);
            }

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_getGrps()
        {
            var boot = new Boot();
            var model = new Mod();
            var mh = new MH();
            var sr = new SR();
            if (boot.isTeklaActive) model.dir = TS.GetTeklaDir(TS.ModelDir.model);
            else model.dir = boot.ModelDir;

            model.elements = sr.Raw(model);
            string md5 = model.getMD5(model.elements);
            Assert.AreEqual(32, md5.Length);

            var grp = mh.getGrps(model.elements);
            Assert.IsTrue(grp.Count > 0);
            string pricing_md5 = model.get_pricingMD5(grp);
            Assert.AreEqual(32, pricing_md5.Length);

            FileOp.AppQuit();
        }

        [TestMethod()]
        public void UT_PriceGr()
       {
            // Assign
            Mod mod = new Mod();
            Rule.Rule rule = new Rule.Rule();
            rule.sSupl = "СтальХолдинг";
            rule.sCS = "Полоса";
            rule.text = "М: C245=C255 ; Профиль: Полоса горячекатаная = PL = — *x*;";
            ElmGr gr = new ElmGr();
            Msg.Dialog = false;
            gr.SupplierName = rule.sSupl;
            gr.guids = new List<string>() { "guid1", "guid2" };
            var U = new UT_TSmatch._UT_MsgService();

            // test 1: Msg.F("Rules not initialyzed") English
            U.SetCulture("en");
            try { mod.mh.PriceGr(mod, gr); } catch { }
            Assert.AreEqual("[Handler.PriceGr]: Rules in Model were not initialyzed", U.GetMsg());

            // test 2: Msg.F("Rules not initialyzed") Russian
            U.SetCulture("ru");
            try { mod.mh.PriceGr(mod, gr); } catch { }
            Assert.AreEqual("[Handler.PriceGr]: Не инциированы правила модели", U.GetMsg());

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
            U.SetCulture("en"); // раскомментировать, чтобы увидеть внешний вид
            Msg.Dialog = true;  // сообщения об ошибке по русски и по английски
            rule.CompSet = cs;
            var match = mod.mh.PriceGr(mod, gr);
            Assert.IsTrue(true);
        }
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