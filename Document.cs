/*-------------------------------------------------------------------------------------------------------
 * Document -- works with all Documents in the system basing on TOC - Table Of Content
 * 
 *  14.3.2016  Pavel Khrapkin, Alex Pass, Alex Bobtsov
 *
 *--------- JOURNAL ----------------  
 * 2013-2015 заложена система управления документами на основе TOC и Штампов
 * 22.1.16 - из статического конструктора начало работы Document перенесено в Start
 * 17.2.16 - в Documents добавлены - строки границы таблицы I0 и IN и их обработка
 * 19.2.16 - getDoc, getDocDir распознавание шаблонов в doc.FileDirectory
 *  5.3.16 - null if Document not found or exist
 *  8.3.16 - ErrorMessage system use; setDocTemplate
 * 12.3.16 - module comments in English; minor corrects in getDoc; multilanguage Forms support
 * 14.3.16 - Document Format support in Documents class data structures, Start, etc. Created class Form
 * -------------------------------------------
 *      METHODS:
 * Start(FileDir)       - Load from directory FileDir of TOC all known Document attributes, prepare everithing
 * setDocTemplate(dirTemplate, val) - set #dirTtemplate value as val in list of #templates  
 * getDoc(name[,fatal]) - return Document doc named in TOC name or create it. Flag fatal is to try to open only
?* NewSheet(name)       - создает новый Лист для документа name и копирует в него шапку
?* loadDoc(name, wb)    - загружает Документ name или его обновления из файла wb, запускает Handler Документа
?* isDocChanged(name)   - проверяет, что Документ name открыт
?* recognizeDoc(wb)     - распознает первый лист файла wb по таблице Штампов
 * 
 * internal sub-class Stamp chech the Stamps of Document - "signature" strings in the defined rows and columns 
 * Stamp(Range rng)     - constructor - set Stamp exemple from Excel Range rng
 * Check(doc.Body.Mtr)  - return true after checking Document Body if it contans right Stamps
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

namespace TSmatch.Document
{
    public class Document
    {
        private static Dictionary<string, Document> Documents = new Dictionary<string, Document>();   //коллекция Документов

        private static string[] FileDirTemplate;   // пары #шаблоны - значения каталогов..
        private static string[] FileDirValue;      //..распознаваемых в getDoc

        public string name;
        private bool isOpen = false;
        public bool isChanged = false;
        public bool isNewDoc = false;   // признак вновь создаваемого Документа
        public string type;
        public int i0, il;              // номера началной и конечной строк оснвной таблицы Документа
        public Excel.Workbook Wb;
        public Excel.Worksheet Sheet;
        private string FileName;
        private string FileDirectory;
        private string SheetN;
        public string MadeStep;
        private DateTime MadeTime;
        private string chkSum;  //контрольная сумма документа MD5 в виде стоки из 32 знаков
        private int EOLinTOC;
        private List<int> ResLines; //число строк в пятке -- возможны альтернативные значения
        private Stamp stamp;        //каждый документ ссылается на цепочку сигнатур или Штамп
        private DateTime creationDate;  // дата создания Документа
        private string Loader;
        public string LoadDescription;  // строка - описание структуры документа
        private string LastUpdateFromFile;
        public int MyCol;           // количесто колонок, добавляемых слева в Документ в loadDoc
        public int usedColumns;     // общее кол-во использованных колонок в Body Документа
        private string HDR_name;    // Named Range Шапки
        public Mtr ptrn;
        public Mtr Body;
        public DataTable dt;
        public Mtr Summary;
        public Form form = null;     // Document's Format description - Document sub-class 
        public Dictionary<string, Dictionary<string, string>> docDic = new Dictionary<string, Dictionary<string, string>>();

//        private const int TOC_DIRDBS_COL = 10;  //в первой строке в колонке TOC_DIRDBS_COL записан путь к dirDBs
//        private const int TOC_LINE = 4;         //строка номер TOL_LINE таблицы ТОС отностися к самому этому документу.
        public Document(string name)    // конструктор создает пустой Документ с именем name
        { this.name = name; }
        public Document(Document d)     // конструктор - перегрузка для копирования Документа
        {
            this.name = d.name;
            this.Body = d.Body;
            this.type = d.type;
            this.i0 = d.i0; this.il = d.il;
            this.isOpen = d.isOpen; this.isChanged = d.isChanged; this.isNewDoc = d.isNewDoc;
            this.Wb = d.Wb; this.Sheet = d.Sheet; this.SheetN = d.SheetN;
            this.FileName = d.FileName; this.FileDirectory = d.FileDirectory;
            this.MadeStep = d.MadeStep; this.MadeTime = d.MadeTime;
            this.chkSum = d.chkSum;
            this.stamp = d.stamp;
            this.creationDate = d.creationDate;
            this.HDR_name = d.HDR_name;
            this.form = d.form;
        }

        /// <summary>
        /// Start(DirTemplates, DirValuue) - prepare further works with the Documents; setup data from TOC, set #Templates
        /// </summary>
        /// <param name="_DirTemplate">#Template string</param>
        /// <param name="_DirValue">#template value to setup</param>
        /// <journal> 22.1.2016
        /// 12.3.2016 - multilanguage Heders support
        /// 14.3.2016 - Form class support
        /// </journal>
        public static void Start(string[] _DirTemplate, string[] _DirValue )
        {
            Log.set("Start(Document #Templates ListCount=" + _DirTemplate.Length + ")");
            bool language = Msg.getLanguage() == Decl.ENGLISH;
            // считываем листы служебного файла TSmatch: TOC, Process, Forms
            FileDirTemplate = _DirTemplate; FileDirValue = _DirValue;
            string FileDir = _DirValue[0];
            if (string.IsNullOrEmpty(FileDir))
            {
                FileDir = Decl.DIR_MATCH;
            }
            //--------- обрабатываем ТОС -------------
            Document toc = new Document(Decl.DOC_TOC);
            toc.Wb = FileOp.fileOpen(FileDir, Decl.F_MATCH);
            toc.Sheet = toc.Wb.Worksheets[Decl.DOC_TOC];
            Form.setWb(toc.Wb);
            Excel.Worksheet hdrSht = toc.Wb.Worksheets[Decl.FORMS]; // All Forms stored as Named Ranges of TSmatch.xls pointed to Forms
            Mtr mtr = toc.Body = FileOp.getSheetValue(toc.Sheet);
            toc.i0 = mtr.Int(Decl.TOC_I0, Decl.DOC_I0);
            toc.il = mtr.Int(Decl.TOC_I0, Decl.DOC_IL);
            toc.isOpen = true;
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
                    doc.i0 = mtr.Int(i, Decl.DOC_I0, "не распознан i0 в строке " + i);
                    doc.il = mtr.Int(i, Decl.DOC_IL, "не распознан il в строке " + i);
                    doc.MadeTime = Lib.getDateTime(mtr[i, Decl.DOC_TIME]);
                    doc.EOLinTOC = mtr.Int(i, Decl.DOC_EOL, "не распознан EOL в строке " + i);
                    doc.ResLines = Lib.ToIntList(mtr.Strng(i, Decl.DOC_TYPE), '/');
                    doc.MyCol = mtr.Int(i, Decl.DOC_I0, "не распознан MyCol в строке " + i);
                    doc.MadeStep = mtr.Strng(i, Decl.DOC_MADESTEP);
                    doc.FileName = mtr.Strng(i, Decl.DOC_FILE);
                    doc.FileDirectory = mtr.Strng(i, Decl.DOC_DIR);
                    doc.SheetN = mtr.Strng(i, Decl.DOC_SHEET);
                    doc.creationDate = Lib.getDateTime(mtr[i, Decl.DOC_CREATED]);
                    doc.LoadDescription = mtr.Strng(i, Decl.DOC_STRUCTURE_DESCRIPTION);
                    //.... working with Document Form .....
///// 15.3.2016 //////////doc.form = new Form(mtr.Strng(i, Decl.DOC_FORMS)
/////temporary stop Form /                  , mtr.Strng(i, Decl.DOC_FORMS +1)
//////////////////////////                  , mtr.Strng(i, Decl.DOC_FORMS +2));
                    string hdrName = mtr.Strng(i, Decl.DOC_FORMS);
                    doc.HDR_name = hdrName;

                    string ptrnName = mtr.Strng(i, Decl.DOC_FORMS);
                    if (ptrnName != "") doc.ptrn = new Mtr(hdrSht.Range[ptrnName].get_Value());
                    //......................................
                    doc.Loader = mtr.Strng(i, Decl.DOC_LOADER);
                    doc.type = mtr.Strng(i, Decl.DOC_TYPE);
                    doc.isNewDoc = (doc.type == Decl.DOC_TYPE_N);
                    int j;
                    for (j = i + 1; j <= mtr.iEOL() && mtr.Strng(j, Decl.DOC_NAME) == ""; j++) ;
                    doc.stamp = new Stamp(i, j - 1);
                } //if docName !=""
            } // for по строкам TOC
#if not_ready
            //                   try { doc.creationDate = Lib.MatchLib.getDateTime(Double.Parse(rw.Range[Decl.DOC_CREATED].Text)); }
            //                   catch { doc.creationDate = new DateTime(0); }

            //                   try { doc.ptrn = hdrSht.get_Range((string)rw.Range[Decl.DOC_PATTERN].Text); } catch { doc.ptrn = null; }
            //                   try { doc.SummaryPtrn = hdrSht.get_Range((string)rw.Range[Decl.DOC_SUMMARY_PATTERN].Text); } catch { doc.SummaryPtrn = null; }
            //                   doc.Loader = rw.Range[Decl.DOC_LOADER].Text;
            //                   // флаг, разрешающий частичное обновление Документа пока прописан хардкодом
            //                   switch (docName)
            //                   {
            //                       case "Платежи":
            //                       case "Договоры": doc.isPartialLoadAllowed = true;
            //                           break;
            //                       default: doc.isPartialLoadAllowed = false;
            //                           break;
            //                   }
            //               }
            //           }

            //-----------------------------------------------------------------
            // из коллекции Documents переносим произошедшие изменения в файл
            //            if (doc.Body.Range["A" + TOC_DIRDBS_COL].Value2 != Decl.dirDBs)
            //            {
            //    Box.Show("Файл '" + F_MATCH + "' загружен из необычного места!");
            //    // переустановка match -- будем делать потом
            //            doc.isChanged = true;
            //          }
            //            doc.EOLinTOC = iEOL;
            //PK            doc.Body.Range["C4"].Value2 = iEOL.ToString();
            //            doc.isChanged = true;   // TOCmatch сохраняем всегда. Возможно, это времянка
            //            doc.isOpen = true;
            //            doc.saveDoc();
#endif //not_ready
                Log.exit();
        } // end Start
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dirTemplate"></param>
        /// <param name="_dirValue"></param>
        public static void setDocTemplate(string _dirTemplate, string _dirValue)
        {
            Log.set("setDocTemplate");
            bool existingTemplate = false;
            int index = 0;
            foreach (string t in FileDirTemplate)
            {
                if (_dirTemplate == t)
                {
                    FileDirValue[index] = _dirValue;
                    existingTemplate = true;
                    break;
                }
                index++;
            }
            if (!existingTemplate) Msg.F("ERR_05.2_setDocTMP_NOTMP", _dirTemplate);
            Log.exit();
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
        /// 5.1.14  - обработка шаблонов Документа
        /// 7.1.14  - отделяем пятку и помещаем в Body и Summary
        /// 5.4.14  - инициализируем docDic, то есть подготавливаем набор данных для Fetch
        /// 12.12.15 - для TSmatch не нужно обрабатывать F.SFDC
        /// 13.12.15 - берем каталог файла из TOC
        /// 22.12.15 - getDoc для нового документа - в Штампе он помечен N
        /// 2.1.16 - полностью переписано для TSmach: не отделяем пятку от Body; закводим новый Лист и шапкой 
        /// 6.1.16 - NOP если FiliDirectory содержит # - каталог Документа еще будет разворачиваться позже
        /// 19.2.16 - распознавание шаблонов в doc.FileDirectory
        ///  5.3.16 - null if Document not found or exist
        /// </journal>
        public static Document getDoc(string name, bool fatal = true)
        {
            Log.set("getDoc(" + name + ")");
            Document doc = null;
            try
            {
                doc = Documents[name];
                if (!doc.isOpen)
                {
                    if (doc.FileDirectory.Contains("#"))
                    {   // #Template substitute with the value in FileDirValues
                        int i = 0;
                        foreach (string str in FileDirTemplate)
                        {
                            if (str == doc.FileDirectory)
                                doc.FileDirectory = FileDirValue[i];
                            i++;
                        }
                    }
            //-------- Load Document from the file or create it ------------
                    bool create = doc.type[0] == 'N' ? true : false;
                    doc.Wb = FileOp.fileOpen(doc.FileDirectory, doc.FileName, create);
                    if(create) doc.Reset();
                    else doc.Body = FileOp.getSheetValue(doc.Wb.Worksheets[doc.SheetN]);
                    doc.Sheet = doc.Wb.Worksheets[doc.SheetN];
                    doc.isOpen = true;
                } // end if(!doc.isOpen)
            } // ent try
            catch (Exception e)
            {
                if (fatal)
                {
                    string msg = (Documents.ContainsKey(name)) ? "не существует" : " не удалось открыть";
                    msg += "\nERROR: " + e;
                    Msg.F("ERR_05.5_getDoc_NOT_OPEN", name, msg);
                }
                doc = null;
            }
            Log.exit();
            return doc;
        }
        /////////////// <summary>
        /////////////// NewSheet(name)  - создание нового листа с заголовком для Документа name
        /////////////// NewSheet()   - создание нового листа для Документа this с именем doc.SheetN
        /////////////// </summary>
        /////////////// <param name="name">имя создаваемого Листа</param>
        /////////////// <returns>вновь созданный Документ name</returns>
        /////////////// <journal>6.4.2014
        /////////////// 22.12.2015 - overload NewSheet(doc)
        /////////////// 4.1.2016 - убрал overload NewSheet(), переписал создание шапки
        /////////////// 17.1.2016 - Шапка целиком копируется из Named Range листа Forms
        /////////////// </journal>
        ////////////public static Document NewSheet(string name)
        ////////////{
        ////////////    Log.set("NewSheet(" + name + ")");
        ////////////    Document doc = Documents[name];
        ////////////    try
        ////////////    {
        ////////////        Excel.Workbook wb = doc.Wb;
        ////////////        wb.Sheets.Add();
        ////////////        wb.ActiveSheet.Name = doc.SheetN;   //создаем Лист по имени в TOC
        ////////////        doc.Sheet = wb.ActiveSheet;
        ////////////        // ---- запись шапки ---
        ////////////        if (doc.HDR_name != "")
        ////////////        {
        ////////////            Document toc = getDoc(Decl.DOC_TOC);
        ////////////            Excel.Range rng = FileOp.setRange(doc.Sheet);
        ////////////            FileOp.CopyRng(toc.Wb, doc.HDR_name, rng);
        ////////////            doc.Body = FileOp.getSheetValue(doc.Sheet);
        ////////////        }     
        ////////////    }
        ////////////    catch (Exception e) { Log.FATAL("ошибка NewSheet(" + name + ")"); }
        ////////////    finally { Log.exit(); }
        ////////////    return doc;
        ////////////}
        /// <summary>
        /// отделение основной части Документа (doc.Body) от пятки (doc.Summary)
        /// </summary>
        /// <journal>
        /// 22.12.2015 - убрал вызов старого iEOL() и iEOC() заменил на Mtr.iEOL() по Body
        /// </journal>
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
        /// Reset() - "сброс" Документа приводит к тому, что его содержимое выбрасывается, шапка переписывается
        /// </summary>
        /// <journal>9.1.2014
        /// 17.1.16 - полностью переписано с записью Шапки
        /// </journal>
        public void Reset()
        {
            Log.set("Reset(" + this.name + ")");
            Document toc = getDoc(Decl.DOC_TOC);
            this.Sheet = FileOp.SheetReset(this.Wb, this.name);
            Excel.Range rng = FileOp.setRange(this.Sheet);
            FileOp.CopyRng(toc.Wb, this.HDR_name, rng);
            this.Body = FileOp.getSheetValue(this.Sheet);
            Log.exit();
        }
        /* PK       /// <summary>
               /// добавляет строку к Body Документа
               /// </summary>
               /// <journal>9.1.2014</journal>
               public Excel.Range AddRow()
               {
                   Log.set("AddRow");
                   try
                   {
                       Body.Range["A" + (int)(Body.Rows.Count + 1)].EntireRow.Insert();
       //                Body.Rows[Body.Rows.Count].Insert(Excel.XlInsertShiftDirection.xlShiftDown);
       //                return Body.Rows[Body.Rows.Count];
                       return Body;
                   }
                   catch
                   { 
                       Log.FATAL("Ошибка при добавлении строки Документа \"" + name + "\"");
                       return null;
                   }
                   finally { Log.exit(); }
               }

               /// <summary>
               /// подсчет контрольной суммы Документа, как суммы ASCII кодов всех знаков во всех ячейках Body 
               /// </summary>
               /// <returns></returns>
               /// <journal>17.1.2014 PKh</journal>
               public long CheckSum()
               {
                   DateTime t0 = DateTime.Now;
                   long checkSum = 0;

                   int maxRow = Body.iEOL();
                   int maxCol = Body.iEOC();
                   for (int i=1; i <= maxRow; i++)
                       for (int j=1; j <= maxCol; j++)
                       {
                           string str = Body.Strng(i, j);
                           if (str.Length == 0) continue;
                           byte[] bt = Encoding.ASCII.GetBytes(str);
                           foreach (var h in bt) checkSum += h;
                       }

                   DateTime t1 = DateTime.Now;
                   new Log("-> " + (t1 - t0) + "\tChechSum=" + checkSum);

                   return checkSum;
               }
        PK */
        /// <summary>
        /// saveDoc(doc [,BodySave, string MD5]) - сохраняет Документ в Excel файл, если он изменялся
        /// </summary>
        /// <param name="name">имя документа</param>
        /// <param name="BodySave>true - doc.Body нужно сохранить, false - уже в Excel</param>
        /// <param name="MD5">MD5 документа. Если BodySave = false - обязательно</param>
        /// <journal>10.1.2016
        /// 18.1.16 - аккуратная обработка BodySave=false и MD5
        /// </journal>
        public static void saveDoc(Document doc, bool BodySave = true, string MD5 = "",int EOL = 0)
        {
            Log.set("saveDoc(\"" + doc.name + "\")");
            try
            {
                Document toc = Documents[Decl.DOC_TOC];
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
                    for (int n = 4; n < toc.Body.iEOL(); n++)
                    {   // находим и меняем строку документа doc TOC
                        if ((string)toc.Body[n, Decl.DOC_NAME] != doc.name) continue;
                        toc.Body[1,1]= Lib.timeStr();
                        toc.Body[n, Decl.DOC_TIME] = Lib.timeStr();
                        toc.Body[n, Decl.DOC_MD5] = doc.chkSum;
                        if( doc.type == "N")
                            toc.Body[n, Decl.DOC_CREATED] = Lib.timeStr();
                        toc.Body[n, Decl.DOC_EOL] = doc.EOLinTOC;
                        FileOp.setRange(toc.Sheet);
                        FileOp.saveRngValue(toc.Body, AutoFit:false);  //======= save TAC in TSmatch.xlsx
                        break;
                    }
                }  
            } catch (Exception e) { Log.FATAL("Ошибка \"" + e.Message + " сохранения файла \"" + doc.name + "\""); }
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
        /// Form - Document's Format description - Document sub-class. Forms get from TSmatch.xlsx/Forms Sheet
        /// </summary>
        /// <journal>14.3.2016</journal>
        public class Form
        {
            private static Excel.Workbook Wb;

            public string name
            {
                get { return Forms.Count == 0? null : Forms[name].name; }
                set
                {
                    if (Forms.ContainsKey(name)) return;
                    Forms.Add(name, new Form(name, Wb.Names.Item(name).RefersToRange));
                }
            }
            public Excel.Range rep
            {
                get { return rep; }
                set
                {
                    rep = value;
                    if (!Forms.ContainsKey(name)) Msg.F("ERR_05.4_FORM_NO_RNG", name);
                    Forms[name].rep = value;
                }
            }
            private Dictionary<string, Form> Forms = new Dictionary<string, Form>(); //sub-Forms
            public Form(string _name, Excel.Range _rep = null)
            {
                this.name = _name;
                this.rep = _rep;
            }
            public Form(string frm, string f1 = "", string f2 = "")
            {
                if (frm == "") return;
                bool language = Msg.getLanguage() == Decl.ENGLISH;
language = true; //en-US
                setForm(language, frm);
                setForm(language, f1);
                setForm(language, f2);
            }
            /// <summary>
            /// lngName(lng, s) - set Form s, when lng=true->"EN_"s, check if it defined, Add Form, and try s"_F"
            /// </summary>
            /// <param name="lng">bool English Language flag</param>
            /// <param name="s">string to handle</param>
            /// <returns></returns>
            private void setForm(bool lng, string s)
            {
                if (string.IsNullOrEmpty(s)) return;
                if (lng)
                {
                    s = "EN_" + s;
                    if (!FileOp.isNamedRangeExist(Wb, s)) Msg.F("ERR_05.4_FORM_NO_RNG", s);
                }
                if (FileOp.isNamedRangeExist(Wb, s)) Forms.Add(s, new Form(s, Wb.Names.Item(s).RefersToRange));
                s = s + "_F";
                if (FileOp.isNamedRangeExist(Wb, s)) Forms.Add(s, new Form(s, Wb.Names.Item(s).RefersToRange));
            }
            public static void setWb(Excel.Workbook _Wb)
            {
                Wb = _Wb;
            }
        } // end class Form
    } // end class Document
} // end namespace