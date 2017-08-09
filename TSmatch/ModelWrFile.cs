﻿/*--------------------------------------------------------------------------------------------
 * ModelWrFile : Model -- Write ModelINFO, Report and other documents in file TSmatchINFO.xlsx
 *
 *  9.08.2017 Pavel Khrapkin
 *
 *--- History ---
 *  13.07.2017 taken from Model code
 *  19.07.2017 Lib.timeStr(date) instead of DateTime format- it is strange behavior of Excel fix
 *   9.08.2017 Docs.getDoc(reset:true)
 *--- Unit Tests --- 
 * 2017.08.9 UT_ModelWrFile: UT_sInt, UT_iDbl, UT_sDat OK
 * -------------------------------------------------------------------------------------------
 * Methods:
 * wrModel(mode, model) - write model in ModelINFO, Report, and other tabs of TSmatchINFO.xlsx
 */
using System;

using log4net;
using Decl = TSmatch.Declaration.Declaration;
using Log = match.Lib.Log;
using Lib = match.Lib.MatchLib;
using Docs = TSmatch.Document.Document;
using Mtch = TSmatch.Matcher.Mtch;
using SType = TSmatch.Section.Section.SType;
using Mod = TSmatch.Model.Model;

namespace TSmatch.Model.WrModelInfo
{
    public class ModelWrFile
    {
        public static readonly ILog log = LogManager.GetLogger("ModelWrFile");

        /// <summary>
        /// wrModel(doc_name) - write formatted data from mod to Excel file
        /// </summary>
        /// <param name="doc_name">document to be written name</param>
        /// <history>16.3.2016
        /// 18.3.2016 - write in Excel list of Rules in FORM_RULE
        /// 26.3.2016 - use rule.CompSet.name reference instead of doc.name
        ///  1.4.2016 - re-written
        /// 21.8.2016 - case constants defined here from Decl, changed TSmatchINFO Document list, restructured
        /// 10.4.2017 - enum WrMod
        ///  9.8.2017 - use sInt, sDbl, sDat
        /// </history>
        public enum WrMod { ModelINFO, Materials, Suppliers, Rules, Report }
        public void wrModel(WrMod mode, Mod mod)
        {
            string doc_name = mode.ToString();
            Log.set("Model.wrModel(" + doc_name + ")");
            DateTime t0 = DateTime.Now;
            Docs doc = Docs.getDoc(doc_name, create_if_notexist: true, reset: true);
            doc.Reset();
            switch (mode)
            {
                case WrMod.ModelINFO:   // общая информация о модели: имя, директория, MD5 и др
                    doc.wrDocSetForm("HDR_ModelINFO", 1, AutoFit: true);
                    string adr = mod.adrCity;
                    if (mod.adrStreet != string.Empty) adr += ", " + mod.adrStreet;
                    doc.wrDocForm(mod.name, adr, mod.dir, mod.phase
                        , sDat(mod.date), mod.MD5
                        , sInt(mod.elements.Count), sDat(mod.pricingDate), mod.pricingMD5);
                    break;
                case WrMod.Materials:   // сводка по материалам, их типам (бетон, сталь и др)
                    doc.wrDocSetForm("FORM_Materials", 3, AutoFit: true);
                    foreach (var mGr in mod.elmMgroups)
                    {
                        doc.wrDocForm(mGr.mat, mGr.totalVolume, mGr.totalWeight, mGr.totalPrice);
                    }
                    break;
                case WrMod.Suppliers:   // сводка по поставщикам проекта (контакты, URL прайс-листа, закупки)
                    doc.wrDocSetForm("FORM_ModSupplierLine", 4, AutoFit: true);
                    foreach (var s in mod.Suppliers)
                    {
                        doc.wrDocForm(s.name, s.Url, s.City, s.index, s.street, s.telephone);
                    }
                    break;
                case WrMod.Rules:       // перечень Правил, используемых для обработки модели
                    doc.wrDocSetForm("FORM_RuleLine");
                    foreach (var rule in mod.Rules)
                    {
                        doc.wrDocForm(sDat(rule.date), rule.Supplier.name, rule.CompSet.name, rule.text);
                    }
                    break;
                case WrMod.Report:      // отчет по сопоставлению групп <материал, профиль> c прайс-листами поставщиков
                    bool noMatchFlag = true;
                    foreach(var gr in mod.elmGroups)
                    {
                        if (gr.match == null || gr.match.ok != Mtch.OK.Match) continue;
                        noMatchFlag = false;
                        break;
                    }
                    if (noMatchFlag)
                    {
                        if (mod.mh == null) mod.mh = new Handler.Handler();
                        mod.mh.Pricing(ref mod);
                    }               
                    doc.wrDocSetForm("FORM_Report", AutoFit: true);
                    int n = 1;
                    foreach (var gr in mod.elmGroups)
                    {
                        string compDescription = "", suplName = "", csName = "";
                        if (gr.match != null && gr.match.ok == Mtch.OK.Match)
                        {
                            compDescription = gr.match.component.Str(SType.Description);
                            suplName = gr.match.rule.Supplier.name;
                            csName = gr.match.rule.CompSet.name;
                        }
                        doc.wrDocForm(n++, gr.Mat, gr.Prf
                            , sDbl00(gr.totalLength), sDbl(gr.totalWeight), sDbl(gr.totalVolume)
                            , compDescription, suplName, csName
                            , sDbl(gr.totalWeight), sDbl(gr.totalPrice));
                    }
                    doc.isChanged = true;
                    doc.saveDoc();
                    //--- string - Summary
                    double sumWgh = 0, sumPrice = 0;
                    int iGr = doc.i0;
                    foreach (var gr in mod.elmGroups)
                    {
                        double? w = doc.Body.Double(iGr, Decl.REPORT_SUPL_WGT);
                        double? p = doc.Body.Double(iGr++, Decl.REPORT_SUPL_PRICE);
                        sumWgh += (w == null) ? 0 : (double)w;
                        sumPrice += (p == null) ? 0 : (double)p;
                    }
                    doc.wrDocSetForm("FORM_Report_Sum", AutoFit: true);
                    doc.wrDocForm(sumWgh, sumPrice);
                    break;
            }
            doc.isChanged = true;
            doc.saveDoc();
            log.Info("Время записи в файл \"" + doc_name + "\"\t t= " + (DateTime.Now - t0).ToString() + " сек");
            Log.exit();
        }

        public string sDbl(double v) { return string.Format("{0 :N2}", v); }
        public string sInt(int i)    { return string.Format("{0 :N0}", i); }
        public string sDbl00(double v) { return string.Format("{0 :N0}", v); }
        public string sDat(DateTime d) { return Lib.timeStr(d, "d.MM.yyyy H:mm"); }
    } // end class
} // end namespace