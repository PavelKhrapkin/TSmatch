using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSmatch.Declaration
{
    /// <summary>
    /// Declaration -- определения глобальных констант
    /// </summary>
    /// <journal> 20.12.2013
    /// 7.1.2014    - добавлена секция констант Шаблонов
    /// 23.1.14     - переход к DataTable
    /// 12.12.15    - адаптация к TSmatch
    /// 22.12.15    - секция типов Штампа Документа
    /// 2.1.2016    - моды работы с Документом
    /// 9.1.2016    - разделитель списков в строке ";"
    /// 24.1.16     - константы Matching_Rules, регулярнае выражения
    /// 17.2.16     - в Documents добавлены - строки границы таблицы I0 и IL и их обработка
    /// 19.2.16     - шаблоны каталогов в doc.FileDirectory
    ///  6.3.16     - измененил STAMP_TYPE_N, заменил на DOC_TYPE_N
    ///  7.3.16     - заложена система сообщений с языком из Windows Culture.Info
    ///  8.3.16     - #шаблоны
    /// 12.3.16     - multilanguage support
    /// 20.3.16     - bootstrap support - copy from current Path to Tekla directories
    /// 25.3.16     - Sullier List attributes
    /// 30.3.16     - Resource constants created
    ///  9.4.16     - TSmatchINFO/Report section
    /// </journal>
    class Declaration
    {
        //------------ GENERAL PURPASE CONSTANTS -----------------
        public const string ENGLISH = "en-US";
        public const string RUSSIAN = "ru-RU";

        public const char STR_DELIMITER = ';';  // знак - резделитель в списке в строках

        public const string TSMATCH = "TSmatch";                // general Application Name
        public const string WIN_TSMATCH_DIR = TSMATCH + "_Dir"; // Windows Path Paramentr
        public const string F_MATCH = TSMATCH + ".xlsx";        // central dispatch file
        public const string TSMATCH_EXE = TSMATCH + ".exe";     // Application exe file       
        public const string BUTTON_DIR  = @"macros\modeling";
        public const string BUTTON_CS   = "TSmatch.cs";
        public const string BUTTON_BMP  = "TSmatch.BMP";
        public const string TSMATCH_ZIP = "Tsmatch.zip";
        public const string DIR_MATCH = @"C:\ProgramData\Tekla Structures\21.1\Environments\common\exceldesign";
        public const string TSMATCH_DIR = "TSmatch";
        public const string TSMATCH_TYPE = "TSmatch";

        //--------- Sheets of TSmatch.xlsm --------------
        public const string DOC_TOC = "TOC";    // Table Of Content - Лист - таблица-содержание всех Документов в TSmatch
        public const string SUPPLIERS = "Suppliers";    //Лист общей информации по Поставщикам
        public const string CONST = "Constants";
        public const string FORMS = "Forms";
        public const string LOG = "Log";

        //------------ RESOURCE CONSTANTS ----------------
        // Resources - are various files, necessary for TSmatch operation in PC.
        // Constants hereunder descripe the Resource name, type and date to be checked as "Actual"
        //!! we should set here (EXPECT DATE) time earlier, than is written in [1,1] of the Document
        public enum RESOURCE_TYPES { Document, File, Directory, BMP, Application };
        public enum RESOURCE_FAULT_REASON { NoFile, Obsolete, NoTekla };

        public const string R_TEKLA = "Tekla";              //Application Tekla Structure is up an running
        public static readonly string R_TEKLA_TYPE = RESOURCE_TYPES.Application.ToString();
        public const string R_TEKLA_DATE = "";

        public const string R_TSMATCH = F_MATCH;            //File TSmatch.xlsx - central dispatch file
        public static readonly string R_TSMATCH_TYPE = RESOURCE_TYPES.File.ToString();
        public const string R_TSMATCH_DATE = "12.4.2016";

        public const string R_TOC = DOC_TOC;                //TSmatch.xlsx/TOC - Table of Content
        public const string R_TOC_TYPE = TEMPL_TOC;     
        public const string R_TOC_DATE = "12.4.2016 10:10";

        public const string R_SUPPLIERS = SUPPLIERS;        //TSmatch.xlsx/Suppliers - Поставщики         
        public const string R_SUPPLIERS_TYPE = TEMPL_TOC;
        public const string R_SUPPLIERS_DATE = "2.4.2016 12:10";

        public const string R_MSG = MESSAGES;               //TSmatch.xlsx/Messages - Сообщения       
        public const string R_MSG_TYPE = TEMPL_TOC;
        public const string R_MSG_DATE = "4.4.2016 22:40";

        public const string R_RULES = RULES;               //TSmatch.xlsx/Rules - Правила       
        public const string R_RULES_TYPE = TEMPL_TOC;
        public const string R_RULES_DATE = "2.4.2016 4:20";

        public const string R_MODELS = MODELS;              //TSmatch.xlsx/Models - Модели
        public const string R_MODELS_TYPE = TEMPL_TOC;
        public const string R_MODELS_DATE = "5.4.2016 13:00";

        public const string R_CONST = CONST;               //TSmatch.xlsx/Constants - Таблицы и константы TSmatch
        public const string R_CONST_TYPE = TEMPL_TOC;
        public const string R_CONST_DATE = "2.4.2016";

        //-----------константы таблицы Документов -----------------
        public const int TOC_I0 = 4;    // строка TOC в таблице TOC

        public const int DOC_TIME = 1; // дата и время последнего изменения Документа
        public const int DOC_NAME = 2; // имя Документа
        public const int DOC_EOL  = 3; // EOL Документа
        public const int DOC_I0   = 4; // номер строки - начала таблицы
        public const int DOC_IL   = 5; // номер последней строки таблицы 
        public const int DOC_TYPE = 6; // Тип Документа
        public const int DOC_MADESTEP = 7; // последний выполненный Шаг
        public const int DOC_DIR =  8; // каталог, где лежит Документ
        public const int DOC_FILE = 9; // файл match, содержащий Документ
        public const int DOC_SHEET = 10; // лист Документа
        public const int DOC_STMPTXT  = 11; // текст Штампа
        public const int DOC_STMPTYPE = 12; // тип Штампа
        public const int DOC_STMPROW  = 13; // строка Штампа
        public const int DOC_STMPCOL  = 14; // колонка Штампа
        public const int DOC_CREATED  = 15; // дата создания Документа
        public const int DOC_FORMS    = 17; // основной шаблон Документа. Помимо заголовков Шапки, он может иметь 
                                            //..имя другой формы - для данных, или имена двух форм с подформами _F 
        public const int DOC_HYPERLINK = 18; //гиперссылка на источник в Интернете
        public const int DOC_SUPPLIER  = 19; // Организация- поставщика сортамента
        public const int DOC_ADR = 20;    // адрес поставщика
        public const int DOC_LOADER = 21; // Loader Документа
        public const int DOC_STRUCTURE_DESCRIPTION = 22;    // строка - описание структуры документа
        public const int DOC_MD5 = 28; // Check Sum MD5

        //-----------константы моды работы с Документом -----------------
        public enum DOC_RW_TYPE { RW, RO, CREATEOROPEN, N1, N2, N };
        public const int READWRITE = 0;   // Мода Документа - открыть на чтение и запись -- по умолчанию
        public const int READONLY = 3;   // Мода Документа - только чтение
        public const int APPEND = 11;   // Мода Документа - дописывать
        public const int CREATEOROPEN = 21;   // Мода Документа - если не найден - создать

        //----------- # шаблоны каталогов документов (поле doc.FileDirectory) --------------------
        public static readonly string[] TOC_DIR_TEMPLATES
            = { TEMPL_TOC, TEMPL_MODEL, TEMPL_COMP, TEMPL_TMP, TEMPL_DEBUG };
        public const string TEMPL_TOC   = "#TOC";           //каталог TSmatch.xlsx 
        public const string TEMPL_MODEL = "#Model";         //каталоги Моделей
        public const string TEMPL_COMP  = "#Components";    //каталог файлов комплектующих - базы поставщиков
        public const string TEMPL_TMP   = "#TMP";           //каталог временного файла
        public const string TEMPL_DEBUG = "#DEBUG";         //каталог для собственной отладки

        //----------- ТИПЫ ШТАМПОВ / ТИПЫ ДОКУМЕНТОВ ----------------------
        ////public const string STAMP_TYPE_EQ = "=";  // точное соответствие
        ////public const string STAMP_TYPE_INC = "I"; // Includes
        public const string DOC_TYPE_N = "N";   // New Doc без Штампа или No Stamp
        ////public const string DOC_TYPE_N1 = "N1"; // New Doc, создавать новый Лист в самой левой позиции, если его нет; для Summary
        ////public const string STAMP_TYPE_N2 = "N2"; // New Doc, если нет, создавать в Листе2

        //-----------константы таблицы Процессов -----------------
        public const string PROCESS = "Process";

        public const int STEP_TIME = 0;    // Дата и время выполнения Шага. Если тут пусто или "/" - это строка-комментарий
        public const int PROC_NAME = 1;    // Имя Процесса. В сроках Шага тут пусто
        public const int STEP_PREVSTEP = 2;    // Предшествующий Шаг. Шаг этой строки выполняем только
                                               //..если предыдущий Шаг выполнен или тут пусто
        public const int STEP_NAME = 3;    // Имя Шага
        public const int STEP_DONE = 4;    // Шаг выполнен "1", иначе - выполняем
        public const int STEP_PARAM = 5;    // Параметры Шага
        public const int STEP_INPDOCS = 6;    // Входные Документы Шага
        public const int STEP_OUTDOCS = 7;    // Выходные Документы Шага

        //------------ константы журнала моделей -------------------
        public const string MODELS = "Models";

        public const int MODEL_DATE = 1;    // Дата и время записи в Журнал моделей
        public const int MODEL_NAME = 2;    // Имя модели
        public const int MODEL_DIR  = 3;    // Каталог модели
        public const int MODEL_MADE = 4;    // Модель обработана Шагом (Made)
        public const int MODEL_PHASE = 5;   // Текущая фаза проекта
        public const int MODEL_MD5  = 6;    // Контрольная сумма - MD5
        public const int MODEL_R_LIST = 7;  // колонка - список номеров строк Правил

        //------------ константы правил Matching_Rules -------------------
        public const string RULES = "Rules";

        public const int RULE_DATE = 1;    // Дата и время записи Правила в TSmatch
        public const int RULE_NAME = 2;    // Имя Правила
        public const int RULE_TYPE = 3;    // Тип Правила
        public const int RULE_RULE = 4;    // Текст Правила
        public const int RULE_COMPSETNAME  = 5; // Price-List - set of Component data from ..
        public const int RULE_SUPPLIERNAME = 6; //..Supplier' name

        public const string ATT_DELIM = @"(${must}|,|=| |\t|\*|x|X|х|Х)";  //делимитры в Правилах
        public const string ATT_MUST  = @"(?<must>(&'.+')|(&"".+"")|(&«.+»)|(&“.+”)"; // &обязательно
        public const string ATT_PARAM = @"(?<param>(\$|p|р|п|P|Р|П)\w*\d)"; //параметры в Правилах

        //------------ Файл TSmatchINFO.xlsx - записывается в каталог модели ----------
        public const string MODELINFO = "ModelINFO";    //Лист общей информации по модели
        public const string RAW = "Raw";                //Лист необработанных данных по компонентам модели
        public const string MODEL_SUPPLIERS = "ModSuppliers";   // Лист поставщиков для проекта / модели
        public const string REPORT = "Report";          //Лист - отчет по модели

        public const string TMP_MODELINFO = "TMP_" + MODELINFO;
        public const string TMP_RAW       = "TMP_" + RAW;
        public const string TMP_REPORT    = "TMP_" + REPORT;

        //------------ TSmatchINFO.xlsx/Report - Отчет по результатам подбора сортамента ----------
        public const int REPORT_N   = 1;     // Report' line Numer
        public const int REPORT_MAT = 2;     // Group Material
        public const int REPORT_PRF = 3;     // Group Profile
        public const int REPORT_LNG = 4;     // Group total Length [mm]
        public const int REPORT_WGT = 5;     // Group total Weight [kg]
        public const int REPORT_VOL = 6;     // Group total Volume [m3]
        public const int REPORT_SUPL_LINE = 7;   // Supplied component description line
        public const int REPORT_SUPL_TYPE = 8;   // Supplied component type
        public const int REPORT_SUPL_LN_N = 9;   // Supplied component' line number in price-list
        public const int REPORT_SUPL_WGT  = 10;  // Supplied component' total weight
        public const int REPORT_SUPL_PRICE = 11; // Supplied component' total price

        //------------ TSmatchINFO.xlsx/Suppliers - Поставщики сортамента ----------

        public const int SUPL_DATE = 1;     // Date when Supplier record  was updated in TSmatch.xlsx
        public const int SUPL_NAME = 2;     // Supplier' name
        public const int SUPL_URL  = 3;     // Supplier' hyperlink
        public const int SUPL_COUNTRY = 4;  // Supplier' Country
        public const int SUPL_INDEX = 5;    // Supplier' Post Index
        public const int SUPL_CITY = 6;     // Supplier' City
        public const int SUPL_STREET = 7;   // Supplier' Street
        public const int SUPL_TEL = 8;      // Supplier' Telephone
        public const int SUPL_LISTCOUNT = 9;// Supplier' price-list/worksheets/Documents count in TSmatch.xlsx

        //-----------константы Шаблонов -----------------
        public const string PTRN_HDR = "A1";     // заголовки колонок   
        public const string PTRN_WIDTH = "A3";     // ширина колонок
        public const int PTRN_FETCH = 6;    // Fetch запрос

        public const string PTRN_COPYHDR = "CopyHdr"; // указание копировать заголовок из Шаблона

        //----------- Language Adaptel strings sets for ru-RU and en-US ----------------
        public const string MESSAGES = "Messages";      // Multilanguage Message doc in TSmatch
        public const int MSG_ID = 1;
        public const int MSG_TXT_RU = 2;
        public const int MSG_TXT_EN = 3;
    }
}