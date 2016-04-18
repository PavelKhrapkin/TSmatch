/*-------------------------------------------------------------------------------------------------------
 * Document -- works with all Documents in the system basing on TOC - Table Of Content
 * 
 *  17.4.2016  Pavel Khrapkin, Alex Pass, Alex Bobtsov
 *
 *--------- JOURNAL ----------------  
 * 2013-2015 заложена система управления документами на основе TOC и Штампов
 * 22.1.16 - из статического конструктора начало работы Document перенесено в Start
 * 17.2.16 - в Documents добавлены - строки границы таблицы I0 и IN и их обработка
 * 19.2.16 - getDoc, getDocDir распознавание шаблонов в doc.FileDirectory
 *  5.3.16 - null if Document not found or exist
 *  8.3.16 - ErrorMessage system use; setDocTemplate
 * 12.3.16 - module comments in English; minor corrects in getDoc; multilanguage Forms support
 * 16.3.16 - Document Format support in Documents class data structures, Start, etc. Created class Form
 * 20.3.16 - use EOL() method
 * 26.3.16 - Reset("Now")
 * 27.3.16 - Close with saveDoc
 * 30.3.16 - get #templates from Bootstrap in Start() and getDoc(name)
 *  1.4.16 - saveDoc(..) overlay
 *  4.4.16 - wrDoc with account of previous output form last_name allow fast multy-string output
 * 17.4.16 - tocStart extracted from Start for initial TOC open
 * -------------------------------------------
 *      METHODS:
 * Start()              - Load from directory TOCdir of TOC all known Document attributes, prepare everithing
 * setDocTemplate(dirTemplate, val) - set #dirTtemplate value as val in list of #templates  
 * getDoc(name[,fatal]) - return Document doc named in TOC name or create it. Flag fatal is to try to open only
 * Reset()              - "Reset" of the Document. All contents of hes Excel Sheet erased, write Header form
 * wrDoc(str, object[]) - write data from obj to the Excel file in format of Form and Form_F
?* loadDoc(name, wb)    - загружает Документ name или его обновления из файла wb, запускает Handler Документа
 * saveDoc(doc [,BodySave, string MD5]) - сохраняет Документ в Excel файл, если он изменялся
 * Close([save_flag])   - Close document, save it when saveflag=true, default - false;
?* isDocChanged(name)   - проверяет, что Документ name открыт
?* recognizeDoc(wb)     - распознает первый лист файла wb по таблице Штампов
 * 
 * internal sub-class Stamp chech the Stamps of Document - "signature" strings in the defined rows and columns 
 * Stamp(Range rng)     - constructor - set Stamp exemple from Excel Range rng
 * Check(doc.Body.Mtr)  - return true after checking Document Body if it contans right Stamps
 * 
 * internal sub-class Form - handle document Forms
 * Init(int toc_line)   - save all Form names for Document from string in toc_line in the list
 */
using System;
using System.Data;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

using FileOp = match.FileOp.FileOp;
using Decl = TSmatch.Declaration.Declaration;
using Mtr = match.Matrix.Matr;  // класс для работы с внутренними структурами данных
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using Templ = TSmatch.Startup.Bootstrap.Template;
//using Boot = TSmatch.Startup.Bootstrap;

namespace TSmatch.Document
{
    public class Document
    {
        private static Dictionary<string, Document> Documents = new Dictionary<string, Document>();   //коллекция Документов

        public string name;
        private bool isOpen = false;
        public bool isChanged = false;
        public bool isNewDoc = false;   // признак вновь создаваемого Документа
        public string type;
        private int EOLinTOC;
        public int i0, il;              // номера началной и конечной строк основной таблицы Документа
        public Excel.Workbook Wb;
        public Excel.Worksheet Sheet;
        private string FileName;
        private string FileDirectory;
        private string SheetN;
        public string MadeStep;
        private DateTime MadeTime;
        private string chkSum;          //контрольная сумма документа MD5 в виде стоки из 32 знаков
        public string Supplier = "";    //имя организации - поставщика сортамента 
        private List<int> ResLines;     //число строк в пятке -- возможны альтернативные значения
        private Stamp stamp;            //каждый документ ссылается на цепочку сигнатур или Штамп
        private DateTime creationDate;  // дата создания Документа
        private string Loader;
        public string LoadDescription;  // строка - описание структуры документа
        private string LastUpdateFromFile;
        public Mtr ptrn;
        public Mtr Body;
        public DataTable dt;
        public Mtr Summary;
        public List<Form> forms = new List<Form>(); // Document's Format descriptions
        public Dictionary<string, Dictionary<string, string>> docDic = new Dictionary<string, Dictionary<string, string>>();

//        private const int TOC_DIRDBS_COL = 10;  //в первой строке в колонке TOC_DIRDBS_COL записан путь к dirDBs
//        private const int TOC_LINE = 4;         //строка номер TOL_LINE таблицы ТОС отностися к самому этому документу.
        public Document(string name)    // конструктор создает пустой Документ с именем name
        { this.name = name; }
        public Document(Document d)     // конструктор - перегрузка для копирования Документа
        {
            name = d.name;
            Body = d.Body;
            type = d.type;
            i0 = d.i0; il = d.il;
            isOpen = d.isOpen; isChanged = d.isChanged; isNewDoc = d.isNewDoc;
            Wb = d.Wb; Sheet = d.Sheet; SheetN = d.SheetN;
            FileName = d.FileName; FileDirectory = d.FileDirectory;
            MadeStep = d.MadeStep; MadeTime = d.MadeTime;
            chkSum = d.chkSum;
            stamp = d.stamp;
            creationDate = d.creationDate;
            forms = d.forms;
        }

        /// <summary>
        /// Start(TOCdir) - prepare further works with the Documents; setup data from TOC
        /// </summary>
        /// <param name="FileDir">Directory, Path to TSmatch.xlsx</param>
        /// <journal> 22.1.2016
        /// 12.3.2016 - multilanguage Heders support
        /// 14.3.2016 - Form class support
        /// 19.3.2016 - use EOL method
        /// 30.3.2016 - Start(TOCdir) and getDoc with #template interaction with Bootstrap
        /// 17.4.2016 - tocStart extracted from Start for initial TOC open
        /// </journal>
        public static void Start(string TOCdir)
        {
            Log.set("Document.Start(" + TOCdir + ")");
            Document toc = tocStart(TOCdir);
            Mtr mtr = toc.Body;
            for (int i = toc.i0; i <= toc.il; i++)
            {
                string docName = mtr.Strng(i, Decl.DOC_NAME);
                if (docName != "")
                {
                    Document doc = new Document(docName);
                    // mtr относится только к TOCmatch, а не ко всем Документам,
                    //.. то есть реально загружаем ТОС, а остальные Документы- потом
                    if (doc.name == Decl.DOC_TOC) doc = toc;
                    Documents.Add(docName, doc);
                    doc.MadeTime = Lib.getDateTime(mtr[i, Decl.DOC_TIME]);
                    doc.ResLines = Lib.ToIntList(mtr.Strng(i, Decl.DOC_TYPE), '/');
                    doc.MadeStep = mtr.Strng(i, Decl.DOC_MADESTEP);
                    doc.FileName = mtr.Strng(i, Decl.DOC_FILE);
                    doc.FileDirectory = mtr.Strng(i, Decl.DOC_DIR);
                    doc.type = mtr.Strng(i, Decl.DOC_TYPE);
                    doc.isNewDoc = (doc.type == Decl.DOC_TYPE_N);
                    doc.SheetN = mtr.Strng(i, Decl.DOC_SHEET);
                    if (doc.type == Decl.TSMATCH_TYPE) doc.Sheet = toc.Wb.Worksheets[doc.SheetN];
                    doc.creationDate = Lib.getDateTime(mtr[i, Decl.DOC_CREATED]);
                    doc.Supplier = mtr.Strng(i, Decl.DOC_SUPPLIER);
                    doc.LoadDescription = mtr.Strng(i, Decl.DOC_STRUCTURE_DESCRIPTION);
                    doc.forms = Form.Init(i);
                    doc.Loader = mtr.Strng(i, Decl.DOC_LOADER);
                    doc.EOL(i);
                    int j;
                    for (j = i + 1; j <= mtr.iEOL() && mtr.Strng(j, Decl.DOC_NAME) == ""; j++) ;
                    doc.stamp = new Stamp(i, j - 1);
                } //if docName !=""
            } // for по строкам TOC
            Log.exit();
        } // end Start
        public static Document tocStart(string TOCdir)
        {
            Log.set("tocStart");
            Document toc = new Document(Decl.DOC_TOC);
            toc.Wb = FileOp.fileOpen(TOCdir, Decl.F_MATCH);
            toc.Sheet = toc.Wb.Worksheets[Decl.DOC_TOC];
            toc.Body = FileOp.getSheetValue(toc.Sheet);
            toc.type = Decl.TSMATCH_TYPE;
            toc.EOL(Decl.TOC_I0);
            Form.setWb(toc.Wb, toc.Body);
            toc.isOpen = true;
            Log.exit();
            return toc;
        }
        /// <summary>
        /// getDoc(name) - get Document name - when nor read yet - from the file. If necessary - Create new Sheet
        /// </summary>
        /// <param name="name">Document name</param>
        /// <param name="fatal">FATAL if this flag = true; else - return null if Document doesnt exists</param>
        /// <returns>Document or null</returns>
        /// <returns>Document</returns>
        /// <journal> 25.12.2013 отлажено
        /// 25.12.2013 - чтение из файла, формирование Range Body
        /// 28.12.13 - теперь doc.Sheet и doc.Wb храним в структуре Документа
        /// 5.4.14  - инициализируем docDic, то есть подготавливаем набор данных для Fetch
        /// 22.12.15 - getDoc для нового документа - в Штампе он помечен N
        /// 6.1.16 - NOP если FiliDirectory содержит # - каталог Документа еще будет разворачиваться позже
        ///  5.3.16 - null if Document not found or exist
        /// 30.3.16 - get #template Path from Bootstrap.Template; try-catch rewritten
        ///  5.4.16 - bug fix - SheetReset for "N" Document
        /// </journal>
        public static Document getDoc(string name, bool fatal = true)
        {
            Log.set("getDoc(" + name + ")");
            Document doc = null;
            string err = "Err getDoc: ", ex= "";
            try { doc = Documents[name]; }
            catch (Exception e) { err += "doc not in TOC"; ex = e.Message; doc = null; }
            if (doc != null && !doc.isOpen)
            {
                if (doc.FileDirectory.Contains("#"))    // #Template substitute with Path in Bootsrap.Templates
                    doc.FileDirectory = Templ.Templates.Find(x => x.template == doc.FileDirectory).templ_val;
                //-------- Load Document from the file or create it ------------
                bool create = !string.IsNullOrEmpty(doc.type) && doc.type[0] == 'N' ? true : false;
                doc.Wb = FileOp.fileOpen(doc.FileDirectory, doc.FileName, create);
                try
                {
                    if (doc.type == Decl.DOC_TYPE_N) FileOp.SheetReset(doc.Wb, doc.SheetN);
                    doc.Sheet = doc.Wb.Worksheets[doc.SheetN];
                }
                catch (Exception e) { err += "no SheetN"; ex = doc.SheetN; doc = null; }
                if(create && doc != null) doc.Reset();
                else if( doc != null) doc.Body = FileOp.getSheetValue(doc.Sheet);
            } // end if(!doc.isOpen)
            if(doc == null && fatal) Msg.F(err, ex, name);
            if(doc != null) doc.isOpen = true;
            Log.exit();
            return doc;
        }
        /// <summary>
        /// Reset() - "Reset" of the Document. All contents of hes Excel Sheet erased, write Header form
        /// Reset("Now") - write DataTime.Now string in Cell [1,1]
        /// </summary>
        /// <journal>9.1.2014
        /// 17.1.16 - полностью переписано с записью Шапки
        /// 16.3.16 - header name get from doc.forms[0]
        /// 26.3.16 - Reset("Now")
        /// 27.3.16 - bug fixed. Issue was: Reset named as a doc.name instead SheetN
        /// </journal>
        public void Reset(string str = "")
        {
            Log.set("Reset(" + this.name + ")");
            Document toc = getDoc(Decl.DOC_TOC);
            this.Sheet = FileOp.SheetReset(this.Wb, this.SheetN);
            Excel.Range rng = FileOp.setRange(this.Sheet);
            if (this.forms.Count == 0) Msg.F("Document.Reset no HDR form", this.name);
            string myHDR_name = this.forms[0].name;
            FileOp.CopyRng(toc.Wb, myHDR_name, rng);
            this.Body = FileOp.getSheetValue(this.Sheet);
            if (str == "Now")
            {
                Body[1, 1] = Lib.timeStr();
                FileOp.setRange(this.Sheet);
                FileOp.saveRngValue(Body);
            }
            Log.exit();
        }
        /// <summary>
        /// wrDoc(str, obect[] obj) -- write data from array of objects to the Document in Excel
        /// </summary>
        /// <param name="str">form name; "str_F" format is also accounted</param>
        /// <param name="obg">data array to be written</param>
        /// <journal>16.3.2016
        /// 26.3.2016 - output HDR_ form with time.Now in [1,1]
        ///  3.4.2016 - multiple line output support with last_name
        /// 13.4.2016 - Internal error message, when form not found
        /// </journal>
        public void wrDoc(string str, params object[] obj)
        {
            Log.set("wrDoc(" + str + ", obj[])");
            int i0 = Body.iEOL() + 1;
            Form frm = forms.Find(x => x.name == str);
            if (frm == null) Msg.F("Document.wrDoc no form", str, this.name);
            if (frm.name == Form.last_name)
            {
                Body.AddRow(obj);
            }
            else
            {
                saveDoc();
                Excel.Range rng = FileOp.setRange(Sheet, i0);
                Document toc = getDoc(Decl.DOC_TOC);
                FileOp.CopyRng(toc.Wb, str, rng);
                Body = FileOp.getSheetValue(Sheet);
                FileOp.saveRngValue(Body);
                int i = 0;
                foreach (var v in obj)
                {
                    int r = frm.row[i] + i0 - 1;
                    int c = frm.col[i++];
                    Body[r, c] = v;
                }
                FileOp.saveRngValue(Body);
                Form.last_name = frm.name;
            }
            Log.exit();
        }
        public void wrDoc(int iForm, params object[] obj) { wrDoc(forms[iForm].name, obj); }
        //public void wrDoc(string form_name, params object[] objs)
        //{
        //    wrDoc(form_name, objs);
        //}
        ////////public void wrDoc(int iForm, object[] lst) // List<T> lst)
        ////////{
        ////////    Form frm = forms.Find(x => x.name == forms[iForm].name);
        ////////    string format_name = frm.name + "_F";
        ////////    if (!FileOp.isNamedRangeExist(Wb, format_name)) Msg.F("ERR __!!__.NOFORM_F", frm.name);
        ////////    object[] obj = new object[lst.Count];
        ////////    //!!-- fill obj[]
        ////////    wrDoc(frm.name, obj);
        ////////}
        ////public void wrDoc(string str)
        ////{
        ////    object[] t = { Lib.timeStr() };
        ////    wrDoc(str, t);
        ////    Body[1, 1] = t[0];
        ////}
        public void wrDoc(List<int> rows, List<int>cols, List<int> rFr, List<int> cFr)
        {
            Log.set("wrDoc(List rows, List cols, List rFr, List cFr)");
            Mtr tmpBody = FileOp.getSheetValue(this.Sheet);
            throw new NotImplementedException();
            Log.exit();
        }
        /// <summary>
        /// EOLstr(str) - return parsed string str into Int32, or return EOLinTOC, when str is EOL
        /// </summary>
        /// <returns>Int32 or EOLinTOC</returns>
        /// <journal>20.3.2016</journal>
        int EOLstr(string str)
        {
            Log.set("Document.Int(" + str + ")");
            int x = 0;
            if (!Int32.TryParse(str, out x))
            {
                if (this.type == "TSmatch") x = this.Sheet.UsedRange.Rows.Count;
                else if (str == "EOL") x = this.EOLinTOC;
            }
            Log.exit();
            return x;
        }
        /// <summary>
        /// EOL(int tocRow) - setup this Document int numbers EndEOLinTOC, i0, and il - main table borders
        /// </summary>
        /// <param name="tocRow">line number of this Document in TOC</param>
        /// <journal>19.3.2016</journal>
        void EOL(int tocRow)
        {
            Document toc = (name == Decl.DOC_TOC) ? this : getDoc(Decl.DOC_TOC);
            EOLinTOC = EOLstr(toc.Body.Strng(tocRow, Decl.DOC_EOL));
            this.i0 = EOLstr(toc.Body.Strng(tocRow, Decl.DOC_I0));
            this.il = EOLstr(toc.Body.Strng(tocRow, Decl.DOC_IL));
        }
        private void splitBodySummary()
        {      
            int fullEOL = Body.iEOL();
            int _resLns = 0;
            switch (ResLines.Count)
            {
                case 0: break;
                case 1: _resLns = ResLines[0]; break;
                default: _resLns = (this.MadeStep == "Loaded") ? ResLines[0] : ResLines[1]; break;
            }
            int iEOL = (_resLns == 0) ? fullEOL : fullEOL - _resLns;
            int iEOC = Body.iEOC();

            Body = FileOp.getRngValue(Sheet, 1, 1, iEOL, iEOC);
            dt = Body.DaTab();
            if (_resLns > 0) Summary = FileOp.getRngValue(Sheet, iEOL + 1, 1, fullEOL, iEOC);
        }
        /// <summary>
        /// loadDoc(name, wb)   - загрузка содержимого Документа name из файла wb
        /// </summary>
        /// <param name="name"></param>
        /// <param name="wb"></param>
        /// <returns>Document   - при необходимости читает name из файла в match и сливает его с данными в wb</returns>
        /// <journal> Не дописано
        /// 15.12.2013 - взаимодействие с getDoc(name)
        /// 7.1.13 - выделяем в Документе Body и пятку посредством splitBodySummary
        /// </journal>
        public static Document loadDoc(string name, Excel.Workbook wb)
        {
            Log.set("loadDoc(" + name + ", " + wb.Name + ")");
            Document doc = getDoc(name);
            doc.LastUpdateFromFile = wb.Name;
            string oldRepName = "Old_" + doc.SheetN;
            try
            {
                /*                wb.Worksheets[1].Name = "TMP";
                                wb.Worksheets[1].Move(doc.Sheet);
                                 // если Old_ уже есть, но еще не обработан - уничтожаем прежний "частичный" отчет
                                if (FileOp.sheetExists(doc.Wb, oldRepName))
                                {
                                    FileOp.DisplayAlert(false);
                                    doc.Wb.Worksheets[doc.name].Delete();
                                    FileOp.DisplayAlert(true);
                                } else doc.Sheet.Name = "Old_" + doc.SheetN;
                                doc.Wb.Worksheets["TMP"].Name = doc.SheetN;
                */
            }
            catch
            {
                Log.FATAL("Не удалось перенести лист [1] из входного файла "
                    + doc.LastUpdateFromFile + " в Документ " + name);
            }
            ////           doc.Sheet = doc.Wb.Worksheets[name];
            doc.splitBodySummary();
            doc.FetchInit();

            ////// если есть --> запускаем Handler
            ////if (doc.Loader != null) Proc.Reset(doc.Loader);
            ////// если нужно --> делаем Merge name с oldRepName
            ////if (FileOp.sheetExists(doc.Wb, oldRepName))
            ////{
            ////    // еще не написано!!
            ////    // NB: в таблице Процессов есть Шаг MergeReps
            ////}
            Log.exit();
            return doc;
        }
        /// <summary>
        /// saveDoc(doc [,BodySave, string MD5]) - сохраняет Документ в Excel файл, если он изменялся
        /// </summary>
        /// <param name="name">имя документа</param>
        /// <param name="BodySave>true - doc.Body нужно сохранить, false - уже в Excel</param>
        /// <param name="MD5">MD5 документа. Если BodySave = false - обязательно</param>
        /// <journal>10.1.2016
        /// 18.1.16 - аккуратная обработка BodySave=false и MD5
        /// 20.1.16 - fix bug: not write EOLinTOC for TSmatch type Documents
        /// 1.04.16 - overlay saveDoc(..)
        /// </journal>
        public static void saveDoc(Document doc, bool BodySave = true, string MD5 = "",int EOL = 0)
        {
            Log.set("saveDoc(\"" + doc.name + "\")");
            try
            {
                Document toc = Documents[Decl.DOC_TOC];
                if (doc.type == Decl.DOC_TYPE_N) doc.isChanged = true; 
                if (doc.isChanged)
                {
                    int EOLinTOC = EOL;
                    if (BodySave)
                    {
                        FileOp.setRange(doc.Sheet);
                        FileOp.saveRngValue(doc.Body);
                        doc.chkSum = doc.Body.ComputeMD5();
                        doc.EOLinTOC = doc.Body.iEOL();
                        FileOp.fileSave(doc.Wb);
                        doc.isChanged = false;
                    }
                    else
                    {
                        if (MD5.Length < 20 || EOL == 0) Msg.F("ERR_05.8_saveDoc_NOMD5");
                        else { doc.chkSum = MD5; doc.EOLinTOC = EOLinTOC; }
                    }
                    Mtr tmp = FileOp.getSheetValue(toc.Sheet);
                    for (int n = toc.i0; n <= toc.il; n++)
                    {   // находим и меняем строку документа doc TOC
                        if ((string)toc.Body[n, Decl.DOC_NAME] != doc.name) continue;
                        tmp[1, 1] = Lib.timeStr();
                        tmp[n, Decl.DOC_TIME] = Lib.timeStr();
                        tmp[n, Decl.DOC_MD5] = doc.chkSum;
                        if (doc.type == "N") tmp[n, Decl.DOC_CREATED] = Lib.timeStr();
                        if (doc.type != Decl.TSMATCH_TYPE) tmp[n, Decl.DOC_EOL] = doc.EOLinTOC;
                        FileOp.setRange(toc.Sheet);
                        FileOp.saveRngValue(tmp, AutoFit: false);  //======= save TОC in TSmatch.xlsx
                        break;
                    }
                }  
            } catch (Exception e) { Log.FATAL("Ошибка \"" + e.Message + "\" сохранения файла \"" + doc.name + "\""); }
            Log.exit();
        }
        public void saveDoc(bool BodySave = true, string MD5 = "", int EOL = 0)
        { saveDoc(this, BodySave, MD5, EOL); }
        /// <summary>
        /// Close([save_flag])   - Close document, save it when saveflag=true, default - false;
        /// </summary>
        /// <param name="save_flag">saveDoc if true. This case all rest parameters as in saveDoc</param>
        public void Close(bool save_flag = false, bool BodySave = true, string MD5 = "", int EOL = 0)
        {
            Log.set("Close(" + name + ")");
            if (save_flag) saveDoc(this, BodySave, MD5, EOL);
            this.isOpen = false;
            FileOp.DisplayAlert(false);
            try { this.Wb.Close(); }
            catch {  }
            FileOp.DisplayAlert(true);
            Log.exit();
        }
        private static void colCpy(Mtr mtr, int rwMtr, Excel.Range rng, int rwRng)
        {
            int cols = mtr.iEOC();
//!!! 2.1.16            for (int col = 1; col <= cols; col++) rng.Cells[rwRng, col] = mtr.get(rwMtr, col);
        }
/*        /// <summary>
        /// recognizeDoc(wb)        - распознавание Документа в Листе[1] wb
        /// </summary>
        /// <param name="wb"></param>
        /// <returns>имя распознанного документа или null, если Документ не распознан</returns>
        /// <journal> 14.12.2013
        /// 16.12.13 (ПХ) переписано распознавание с учетом if( is_wbSF(wb) )
        /// 18.01.14 (ПХ) с использование Matrix
        /// </journal>
        public static string recognizeDoc(Excel.Workbook wb)
        {
            Log.set("recognizeDoc(wb)");
            Mtr wbMtr = FileOp.getSheetValue(wb.Worksheets[1]);
            // вначале проверим где у wb Штамп - в теле или в пятке? Штамп в пятке бывает только у SF
            // отделим от wbMtr область пятки SF -- переложим SFresLines строк wbMtr в wdSFsummary
            int iEOL = wbMtr.iEOL();
            int iEOC = wbMtr.iEOC();
            object[,] tmp = new object [Decl.SFresLines + 1, iEOC + 1];
            for (int rw = 1; rw <= Decl.SFresLines; rw++)
                for (int col = 1; col <= iEOC; col++)
                   tmp[rw, col] = wbMtr.get(iEOL - Decl.SFresLines + rw - 1, col);
            Mtr wbSFsummary = new Mtr(tmp);

                    Mtr rng = (Documents["SFDC"].stamp.Check(wbSFsummary))? wbSFsummary: wbMtr;

            try 
            {
                foreach (var doc in Documents)  // ищем подходящий документ по Штампам
                    if (doc.Value.stamp.Check(rng)) return doc.Value.name;
                return null;                    // если ничего не нашли -> вовращаем null
            }
            finally { Log.exit(); }                  
        }
*/ //2.1.16
        /// <summary>
        /// инициирует Fetch-структуру Документа для Запроса fetch_rqst.
        /// Если fetch_rqst не указан - для всех Запросов Документа.
        /// </summary>
        /// <param name="fetch_rqst"></param>
        /// <journal>11.1.2014 PKh
        /// 15.1.2014 - дописан FetchInit() - просмотр всех Fetch Документа</journal>
        public void FetchInit()
        {
            Log.set("FetchInit");
            try
            {
                for (int col = 1; col <= ptrn.iEOC(); col++)
                {
                    string ftch = ptrn.Strng(Decl.PTRN_FETCH, col);
                    string[] ar = ftch.Split('/');
                    if (ar.Length <= 2) continue;
                    Document doc = getDoc(ar[0]);
                    doc.FetchInit(ftch);
                }
            }
            catch { Log.FATAL("ошибка FetchInit() для Документа \"" + name + "\""); }
            finally { Log.exit(); }
        }
        /// <param name="fetch_rqst"></param>
        /// <example>FetchInit("SFacc/2:3")</example>
        public void FetchInit(string fetch_rqst)
        {
            Log.set("FetchInit(fetch_rqst)");
            try
            {
                if (string.IsNullOrEmpty(fetch_rqst)) { FetchInit(); return; }
                string[] ar_rqst = fetch_rqst.Split('/');
                if (!Documents.ContainsKey(ar_rqst[0])) Log.FATAL("нет такого Документа");
                string strFetch = ar_rqst[0] + "/" + ar_rqst[1];
                if (docDic.ContainsKey(strFetch)) return; // уже инициирован -> return
                Document doc = getDoc(ar_rqst[0]);
                string[] cols = ar_rqst[1].Split(':');
                int key = Lib.ToInt(cols[0]);
                int val = Lib.ToInt(cols[1]);
                Dictionary<string, string> keyDic = new Dictionary<string, string>();
                docDic.Add(strFetch, keyDic);
                DateTime t0 = DateTime.Now;
                for (int i = 1; i <= doc.Body.iEOL(); i++)
                {

                    string s1 = doc.Body.Strng(i, key);
                    if (s1 != "")try { keyDic.Add(s1, doc.Body.Strng(i, val)); }
                        catch
                        {
                            Log.Warning("Запрос \"" + fetch_rqst + " Строка " + i
                                + " неуникальное значение \"" + s1 + "\" в ключевом поле запроса!");
                        }
                }
                DateTime t1 = DateTime.Now;
                new Log("-> "+(t1-t0));
            }
            catch { Log.FATAL("ошибка запроса \"" + fetch_rqst + "\" для Документа \"" + name + "\""); }
            finally { Log.exit(); }
        }
        /// <summary>
        /// Fetch(fetch_rqst, x) -- извлекает значение по строкам х и ftch_rqst
        /// </summary>
        /// <example>Fetch("SFacc/2:3/0", "ООО «ОРБИТА СПб»") </example>
        /// <example>Fetch("SF/2:3/0", "ООО «ОРБИТА СПб»") </example>
        /// <param name="fetch_rqst"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        /// <journal>5.4.2014</journal>
        public string Fetch(string fetch_rqst, string x)
        {
            Log.set("Fetch");
            string result = null;
            try
            {
                string[] ar_rqst = fetch_rqst.Split('/');
                Document doc = getDoc(ar_rqst[0]);
                Dictionary<string, string> Dic = doc.docDic[ar_rqst[0] + "/" + ar_rqst[1]];
                result = Dic[x];
            }
            catch { Log.FATAL("ошибка Fetch( \"" + fetch_rqst + "\", \"" + x + "\")" ); }
            finally { Log.exit(); }
            return result;
        }
        /// <summary>
        /// Класс Stamp, описывающий все штампы документа
        /// </summary>
        /// <journal> дек 2013
        /// 12.1.2014 - работа с матрицей в памяти, а не с Range в Excel
        /// </journal>
        private class Stamp
        {
            public List<OneStamp> stamps = new List<OneStamp>();
            /// <summary>
            /// конструируем цепочку Штампов по строкам TOC от i0 до i1
            /// </summary>
            /// <param name="i0"></param>
            /// <param name="i1"></param>
            /// <journal>
            /// 18.1.2014 (ПХ) в класс Штамп и в конструктор добавлен _parentDoc - Документ Штампа
            /// </journal>
            public Stamp(int i0, int i1)
            {
                Document doc_toc = getDoc(Decl.DOC_TOC);
                if (doc_toc.Body.Strng(i0, Decl.DOC_STMPTYPE) != Decl.DOC_TYPE_N)
                {
                    for (int i = i0; i <= i1; i++) stamps.Add(new OneStamp(doc_toc, i));
                }
            }
            /// <summary>
            /// Check(Документ) - проверка, что штамп в Документе соответствует цепочке Штампов в TOCmatch
            /// 
            /// Штамп.Check(Mtr) - проверяем, что данные в Mtr содержат сигнатуры Штампа на нужных местах
            /// </summary>
            /// <param name="doc">проверяемый Документ</param>
            /// <returns>true, если результат проверки положительный, иначе false</returns>
            /// <journal> 12.12.13
            /// 16.12.13 (ПХ) перенес в класс Stamp и переписал
            /// 13.1.2014 - переписано
            /// 18.1.14 (ПХ) - переписано еще раз: проверяем mtr
            /// </journal>
            public bool Check(Mtr mtr)
            {             
                if (mtr == null) return false;
                foreach (OneStamp st in stamps) if (!st.Check(mtr)) return false;
                return true;
            }
            /// <summary>
            /// Trace(Stamp)    - вывод в Log-файл данных по Штампам Документа
            /// </summary>
            /// <param name="st"></param>
            /// <journal> 26.12.13 -- не дописано -- нужно rnd не только doc.Body, но для SF doc.Summary
            /// 18.1.14 (ПХ) отладка с Matrix
            /// 12.12.15 - для TSmatch не нужно обрабатывать документы типа SFDC
            /// </journal>
            public string Trace(Document doc)
            {
                Mtr rng = doc.Body;
                string msg = (string)((rng == doc.Summary) ? "Пятка" : "Body");
                msg += "Документ не соответствует Штампам";
                foreach (OneStamp st in doc.stamp.stamps)
                    traceSub(st.Check(rng) ? "OK" : "!!", st);
                return msg;
            }
            static void traceSub(string msg, OneStamp st)
            {
                new Log("\t=" + msg + "=> " + st.get("type") + " " + st.get("sig") + " " + st.get());
            }
        }
        /// <summary>
        /// Класс, описывающий штамп документа (с вариантами позиций, заданными в одной стрке TOCmatch)
        /// </summary>
        public class OneStamp
        {
            private string signature;   // проверяемый текст Штампа - сигнатура
            private string typeStamp;   // '=' - точное соответствие сигнатуры; 'I' - "текст включает.."
            private List<int[]> stampPosition = new List<int[]>();   // альтернативные позиции сигнатур Штампов
            /// <summary>
            /// Конструктор OneStanp(doc_toc, int rowNumber)
            /// </summary>
            /// <param name="doc_toc">  таблица TOCmatch</param>
            /// <param name="rowNumber">одна строка штампа (т.е. сигнатура и позиции)</param>
            /// <example>
            /// примеры: {[1, "1, 6"]} --> [1,1] или [1,6]
            ///  .. {["4,1", "2,3"]} --> [4,2]/[4,3]/[1,2]/[1,3]
            /// </example>
            /// <journal> 12.12.2013 (AP)
            /// 16.12.13 (ПХ) добавлен параметр isSF - добавляется в структуру Штампа
            /// 12.1.14 - работаем с TOCmatch с памяти -- Matrix
            /// </journal>
            public OneStamp(Document doc, int rowNumber)
            {
                signature = doc.Body.Strng(rowNumber, Decl.DOC_STMPTXT);
                typeStamp = doc.Body.Strng(rowNumber, Decl.DOC_STMPTYPE);
  
                List<int> rw  = intListFrCell(doc, rowNumber, Decl.DOC_STMPROW);
                List<int> col = intListFrCell(doc, rowNumber, Decl.DOC_STMPCOL);
                // декартово произведение множеств rw и col
                rw.ForEach(r => col.ForEach(c => stampPosition.Add(new int[] { r, c })));
            }
            /// <summary>
            /// используется для внешнего доступа к private полям Штампа, в т.ч. для Log и Trace
            /// </summary>
            /// <param name="str">что извлекаем: "signature" или "type" или "position"</param>
            /// <returns>string</returns>
            /// <journal> 18.1.2014 (ПХ)</journal>
            public string get(string str = "position")
            {
                string v;
                switch (str.ToLower()[0])
                {
                    case 's': v = signature; break;
                    case 't': v = typeStamp; break;
                    default:
                        {
                            v = "{";
                            foreach (int[] pos in stampPosition) v += "[" + pos[0] + "," + pos[1] + "]";
                            v += "}"; break;
                        }
                }
                return v;
            }
            private List<int> intListFrCell(Document doc, int row, int col)
            {
                return Lib.ToIntList(doc.Body.Strng(row, col), ',');
            }
            /// <summary>
            /// Stamp.Check(mtr) - проверка mtr на соответствие Штампу в stmp. ОК->true, несовпадение->false
            /// </summary>
            /// <param name="mtr"></param>
            /// <returns>bool: true если проверка Штампа дает совпадение сигнатуры</returns>
            /// <journal> 12.12.2013
            /// 25.12.13 (ПХ) ToString вместо Value2 для проверяемой ячейки
            /// 13.1.14 - работа с матрицами
            /// 18.1.14 - (ПХ) рефакторинг. Теперь сверяем strToCheck в mtr и SigInStamp в Штампе
            /// </journal>
            public bool Check(Mtr mtr)
            {
                string sigInStamp = signature.ToLower();
                foreach (var pos in stampPosition) {
                    string strToCheck = mtr.Strng(pos[0], pos[1]).ToLower();
                    if (typeStamp == "=") {
                        if (strToCheck == sigInStamp) return true;
                    } else {
                        if (strToCheck.Contains(sigInStamp)) return true;
                    }
                }
                return false;
            }
        }   // end class OneStamp
        /// <summary>
        /// Form - Document's Format description - Document sub-class. Forms get from TSmatch.xlsx/Forms
        /// </summary>
        /// <journal>16.3.2016</journal>
        public class Form
        {
            private static Excel.Workbook Wb;
            private static Mtr tocMtr;
            internal static string last_name;

            public string name;
            public List<int> row = new List<int>();
            public List<int> col = new List<int>();

            public Form(string _name, List<int> _row, List<int> _col)  // constructor Form
            {
                name = _name; row = _row; col = _col;
            }
            /// <summary>
            /// List<Form> Init(int toc_line) - initiate all Forms for Document in toc_line.
            /// </summary>
            /// <param name="toc_line">line number in TOC</param>
            /// <returns>form list</returns>
            public static List<Form> Init(int toc_line)
            {
                Log.set("Init(" + toc_line + ")");
                bool language = Msg.getLanguage() == Decl.ENGLISH;
                //------------------------------------------------------------------------------
                language = true; //en-US for Englisg Debug. Remove or comment this line later  !
                //------------------------------------------------------------------------------
                List<Form> Forms = new List<Form>();
                for (int col = Decl.DOC_FORMS, i = 0; i < 10; i++)
                {
                    string s = tocMtr.Strng(toc_line, col++);
                    if (string.IsNullOrEmpty(s)) continue;                 
                    if (language) s = "EN_" + s;
                    if (FileOp.isNamedRangeExist(Wb, s))
                    {
                        List<int> _r = new List<int>();
                        List<int> _c = new List<int>();
                        string sf = s + "_F";
                        if (FileOp.isNamedRangeExist(Wb, sf))
                        {
                            Mtr format = new Mtr(Wb.Names.Item(sf).RefersToRange.Value);
                            for (int c = 1; c <= format.iEOC(); c++)
                                for (int r = 1; r <= format.iEOL(); r++)
                                {
                                    string f = format.Strng(r, c);
                                    if ( f.Contains("{") & f.Contains("}")) { _r.Add(r); _c.Add(c); }
                                }
                        }
                        Forms.Add(new Form(s, _r, _c));
                    }                 
                }
                Log.exit();
                return Forms;
            }
            public static void setWb(Excel.Workbook _Wb, Mtr _tocMtr)
            {
                Wb = _Wb; tocMtr = _tocMtr;
            }
        } // end class Form
        /// <summary>
        /// BodyForm - class BodyForm used for output some (not all!) fields of Body into Excel
        /// </summary>
        /// <journal>20.3.2016</journal>
        ////public class BodyForm
        ////{
        ////    public Form form;
        ////    public List<int> rowsBody = new List<int>();
        ////    public List<int> colsBody = new List<int>();

        ////    public BodyForm(string name, List<int> r, List<int> c, List<int> rB, List<int> cB)
        ////    {
        ////        form = new Form(name, r, c);
        ////        rowsBody = rB; colsBody = cB;
        ////    }

        ////    public BodyForm BodyRowForm(string _name, int _r, int[] _cols)
        ////    {
        ////        Log.set("BodyRowForm");
        ////        BodyForm result = null;
        ////        List<int> rs = new List<int>();
        ////        List<int> cs = new List<int>();
        ////        List<int> rB = new List<int>();
        ////        List<int> cB = new List<int>();
        ////        foreach (int col in _cols)
        ////        {
        ////            result.rows
        ////        }
        ////        result.Add(new BodyForm(_name, rs, cs, rB, cB));
        ////        Log.exit();
        ////        return result;
        ////    }
        ////} // end class BodyForm
    } // end class Document
} // end namespace