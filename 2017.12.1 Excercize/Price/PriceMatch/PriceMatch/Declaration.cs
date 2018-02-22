/* ----------------------------------------
 * Declaration of global constant and tags
 * 
 * 18.02.2018 Pavel Khrapkin
 * 
 * ----- Regions content ---------
 * TSmatchINFO Tabs - Documents TSmatchINFO.xlsx - not in use yet
 * SectionTab       - STypes for standard Section headers
 * IdTypes for classes - types of classes with Id, tag - part of Id
 */

using System;

namespace PriceMatch
{
    public class Declaration
    {
        public static readonly DateTime MinDate = new DateTime(2010, 1, 1);

        #region --- TSmatchINFO.xlsx Tabs = Documents ---
        public const string sINFO = "ModelINFO";
        public const string sReport = "Report";
        public const string sRules = "Rules";
        #endregion --- TSmatchINFO.xlsx Tabs = Documents ---

        #region --- SectionTab TSmatch Section Headers ---
        public const string STypeNOT_DEFINED = "";
        public const string STypeMAT = "MATERIAL";
        public const string STypePRF = "PROFILE";
        public const string STypePRICE = "PRICE";
        public const string STypeDESCR = "DESCRIPTION";
        public const string STypeLNG = "LENGTH";
        public const string STypeVOL = "VOLUME";
        public const string STypeWGT = "WEIGHT";
        public const string STypeUNIT_LNG = "UNT_L";
        public const string STypeUNIT_VOL = "UNT_V";
        public const string STypeUNIT_WGT = "UNT_W";
        public const string STypeUNIT_W_KG = "UNT_W_KG";
        public const string STypeUNIT_Q = "UNT_Q";
        #endregion --- SectionTab TSmatch Section Headers ---

        #region --- IdTypes for classes ---
        public const string IdTypeInutPO = "PO";
        public const string IdTypeItemPO = "ItemPO";
        public const string IdTypeRule = "Rule";
        public const string IdTypeSupl = "Supl";
        public const string IdTypePS = "PS";
        public const string IdTypeProd = "Prod";
        #endregion --- IdTypes for classes ---
    }
}
