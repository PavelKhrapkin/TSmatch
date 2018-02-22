/* ---------------------------------------------------------------------------
 * Documents - basic class working with Documents, their TOC and Conents
 * 
 *  16.2.2018 Pavel Khrapkin
 * 
 * NB: Document is NOT Serializable, it could not be written in XML or get MD5
 * 
 * 2018.1.12 UT_DocumentTests: UT_Init, UT_DocIntStr, UT_Document   OK
 *                  UT_GetDoc, UT_IsDocExists
 * --- History: -----
 *  16.02.2018 - transfer to PriceMatch
 * 
 * --- toc & Methods: --------------
 * toc - Table Of Content class. The document contains collection of all Docs
 * Init(boot, HPath)    - Initiate TOC from bootstrap
 * Init(name, ...)      - initiate Document name with many optional parameters
 * GetDoc([name])   - return Document name, or TOC, when name is null or empty
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace PriceMatch
{
    public class Document
    {
        private Boot boot;
        private Dictionary<string, string> HPath;
        private toc T;
        public const string TOC = "TOC";

        public string name;
        public string path;
        public string FileName;
        public string SheetN;
        public DateTime Date;
        public DateTime Created;
        public int i0;
        public int il;
        public string LoadDescription;
        public List<string> RuleText = new List<string>();
        public Mtr Body;
        public bool IsOpen;

        public Document() { }

        #region ----- init region -----
        public void Init(Boot boot, Dictionary<string, string> HPath)
        {
            if (T == null)
            {
                T = new toc();
                T.tocInit(HPath);
            }
            this.boot = boot;
            this.HPath = HPath;
        }

        public bool Init(string Name, string Path = "", string fileName = "",
            string SheetName = "", string _date = "", string created = "",
            dynamic FirstRow = null, dynamic LastRow = null,
            string LoadDescription = "", params string[] RuleText)
        {
            if (IsDocExist(Name)) T.DelDoc(Name);
            Document doc = new Document();
            doc.Init(boot, HPath);
            doc.name = Name;
            doc.path = Path.Contains("#") ? T.GetPath(Path) : Path;
            doc.FileName = fileName;
            if (!boot.fo.IsFileExist(doc.path, fileName)) return false;
            doc.SheetN = SheetName == null ? Name : SheetName;
            // Excel file not open yet - Sheet list invisibe untile file open
            //           if (!boot.fo.IsSheetExist(doc.SheetN)) return false;
            doc.Date = time(_date);
            doc.Created = time(created);
            doc.i0 = intRow(FirstRow);
            doc.il = intRow(LastRow);
            if (!string.IsNullOrEmpty(LoadDescription))
                doc.LoadDescription = LoadDescription;
            if (RuleText.Length > 0)
                foreach (string rt in RuleText) doc.RuleText.Add(rt);
            T.AddTOC(doc);
            return IsDocExist(Name);
        }

        private int intRow(dynamic row)
        {
            if (row == null) return -1;
            if (row.GetType() == typeof(int)) return row;
            if (row.GetType() != typeof(string))
                throw new Exception("[Doc.toc] bad int parameter");
            int i = -1;
            int.TryParse(row, out i);
            return i;
        }

        public double Dec(int row, int col)
        { return (double)Convert.ToDecimal(Str(row, col)); }
        public int Int(int row, int col)
        { return Convert.ToInt32(Str(row, col)); }
        public string Str(int row, int col)
        {
            var v = Body.x(row, col);
            if (v == null) return null;
            return v.ToString();
        }

        private DateTime time(string dt)
        {
            DateTime result = DateTime.MinValue;
            DateTime.TryParse(dt, out result);
            if (dt == "Now") result = DateTime.Now;
            return result;
        }
        #endregion ----- init region -----

        public Document GetDoc(string name = null)
        {
            var docs = T.GetDocs();
            Document doc = null;
            if (string.IsNullOrEmpty(name)) name = TOC;
            try { doc = docs[name]; }
            catch { return null; }
            if (!doc.IsOpen)
            {
                var fo = new FileOp();
                fo.FileOpen(doc.path, doc.FileName);
                doc.Body = fo.GetSheetValue(doc.SheetN);
                if (doc.Body == null) return null;
                if (doc.il == -1 || doc.il < doc.i0) doc.il = doc.Body.iEOL();
                doc.IsOpen = true;
            }
            return doc;
        }

        public bool IsDocExist(string name)
        {
            var docs = T.GetDocs();
            return docs.ContainsKey(name);
        }

        public void AllDocClose()
        {
            var docs = T.GetDocs();
            foreach (var doc in docs)
            {
                doc.Value.Close();
            }
        }
        public void Close()
        {
            boot.fo.FileOpen(path, FileName);
            boot.fo.AppQuit();
        }

        public static implicit operator Document(object[] obj)
        {
            Document doc = new Document();
            doc.name = (string)obj[4];
            return doc;
        }
    }

    #region ----- TOC region -----
    class toc : Document
    {
        protected Dictionary<string, Document> documents
            = new Dictionary<string, Document>();
        protected Dictionary<string, string> HPath
            = new Dictionary<string, string>();

        public void tocInit(Dictionary<string, string> HPath)
        {
            const string p = "#TOC", m = "#Model";
            const string f = "TSmatch.xlsx", fm = "TSmatchINFO.xlsx";

            this.HPath = HPath;

            AddTOC(TOC, p, f);

            AddTOC(Declaration.sINFO, m, fm, 2);
            AddTOC(Declaration.sReport, m, fm, 2);
            AddTOC(Declaration.sRules, m, fm, 2);

            documents[TOC].IsOpen = true;
            documents[TOC].Body = new Mtr(documents.Values.ToList());
        }

        public void AddTOC(string name, string path, string file, int i0 = 0,
            string SheetN = "", string LoadDescription = "")
        {
            if (documents.ContainsKey(name)) return; //потокобезопасность UT
            Document doc = new Document();
            doc.name = name;
            doc.path = path.Contains("#") ? GetPath(path) : path;
            doc.FileName = file;
            doc.SheetN = string.IsNullOrEmpty(SheetN) ? name : SheetN;
            doc.LoadDescription = LoadDescription;
            doc.i0 = i0;
            doc.il = -1;
            documents.Add(doc.name, doc);
        }
        public void AddTOC(Document doc)
        {
            if (documents.ContainsKey(doc.name)) documents.Remove(doc.name);

            documents.Add(doc.name, doc);
        }

        public Dictionary<string, Document> DelDoc(string Name)
        {
            documents.Remove(Name);
            return documents;
        }

        public Dictionary<string, Document> GetDocs() { return documents; }
        public string GetPath(string Hpath)
        {
            try { return HPath[Hpath]; }
            catch { throw new Exception("HPath " + Hpath + " doesn't exist"); }
        }
    }
    #endregion ----- TOC region -----
} // end namespace