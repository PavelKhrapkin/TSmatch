/*--------------------------------------------------------------------------------------------
 * ModelWrFile : Model -- Write ModelINFO, Report and other documents in file TSmatchINFO.xlsx
 *
 *  18.08.2017 Pavel Khrapkin
 *
 *--- Unit Tests --- 
 * 2017.08.9 UT_ModelWrFile: UT_sInt, UT_iDbl, UT_sDat OK
 *--- History ---
 * 13.07.2017 taken from Model code
 * 19.07.2017 Lib.timeStr(date) instead of DateTime format- it is strange behavior of Excel fix
 *  9.08.2017 Docs.getDoc(reset:true); use sInt, sDbl, sDat
 * 18.08.2017 Write to Report gr.compDescription string, delete isMatches method
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
using Mod = TSmatch.Model.Model;
using Msg = TSmatch.Message.Message;

namespace TSmatch.Model.WrModelInfo
{
    public class ModelWrFile
    {
        public static readonly ILog log = LogManager.GetLogger("ModelWrFile");

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
                        doc.wrDocForm(s.Name, s.Url, s.City, s.Index, s.Street, s.Telephone);
                    }
                    break;
                case WrMod.Rules:       // перечень Правил, используемых для обработки модели
                    if (mod.Rules.Count == 0) Msg.F("Can't write TSmatchINFO.xlsx/Rules");
                    doc.wrDocSetForm("FORM_RuleLine");
                    foreach (var rule in mod.Rules)
                    {
                        doc.wrDocForm(sDat(rule.date), rule.sSupl, rule.sCS, rule.text);
                    }
                    break;
                case WrMod.Report:      // отчет по сопоставлению групп <материал, профиль> c прайс-листами поставщиков
                    doc.wrDocSetForm("FORM_Report", AutoFit: true);
                    int n = 1;
                    foreach (var gr in mod.elmGroups)
                    {
                        string compDescription = "", suplName = "", csName = "";
                        if(!string.IsNullOrEmpty(gr.SupplierName))
                        {
                            suplName = gr.SupplierName;
                            csName = gr.CompSetName;
                            compDescription = gr.compDescription;
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