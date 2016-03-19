/*----------------------------------------------------------------------------
 * Components -- работа с документами - прайс-листами поставщиков компонентов
 * 
 *  5.3.2016  П.Храпкин
 *
 *--- журнал ---
 * 28.2.2016 выделено из модуля Matcher
 *  5.3.2016 setComp(doc) - инициальзация данных для базы компонентов в doc
 * ---------------------------------------------------------------------------
 *      МЕТОДЫ:
 * setComp(doc) - инициальзация данных для базы компонентов в doc
 * getComp(doc) - загружает Excel файл - список комплектующих от поставщика
 * UddateFrInternet() - обновляет документ комплектующих из Интернет   
 *    --- Вспомогательные методы - подпрограммы ---
 * UpgradeFrExcel(doc, strToDo) - обновление Документа по правилу strToDo
 */

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;
using Decl = TSmatch.Declaration.Declaration;
using TS = TSmatch.Tekla.Tekla;
using Docs = TSmatch.Document.Document;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Matcher;

namespace TSmatch.Component
{
    class Component
    {
        private static Dictionary<string, Component> CompSet = new Dictionary<string, Component>();   //коллекция баз компонентов

        public readonly string description; // строка вида "Угoлoк cтaльнoй paвнoпoл. 25 x 4 cт3cп/пc5"
        public readonly double length;      // длина заготовки в м
        public readonly double weight_m;    // вес погонного метра заготовки -- пока тут пусто
        public readonly double? price;      // цена за 1 тонну в руб

        public Component(string _description, double _length, double _weight, double? _price)
        {
            description = Lib.ToLat(_description);
            length = _length;
            weight_m = _weight;
            price = _price;
        }
        public static List<Component> Comps = new List<Component>();

        public static void setComp(Docs doc)
        {
            Log.set("setComp(" + doc.name + ")");
            List<int> docCompPars = Mtch.GetPars(doc.LoadDescription);
            //-- заполнение массива комплектующих Comps
            Comps.Clear();
            for (int i = doc.i0; i <= doc.il; i++)
            {
                string str = doc.Body.Strng(i, docCompPars[0]);
                double lng = 0;
                //-- разбор параметров LoadDescription
                List<int> strPars = Mtch.GetPars(str);
                string docDescr = doc.LoadDescription;
                int parShft = 0;
                while (docDescr.Contains('/'))
                {
                    string[] s = doc.LoadDescription.Split('/');
                    List<int> c = Mtch.GetPars(s[0]);
                    int pCol = c[c.Count() - 1];    // колонка - последний параметр до '/'
                    List<int> p = Mtch.GetPars(s[1]);
                    lng = strPars[p[0] - 1];    // длина заготовки = параметр в str; индекс - первое число после '/'
                    docDescr = docDescr.Replace("/", "");
                    parShft++;
                }
                if (lng == 0)
                    lng = doc.Body.Int(i, docCompPars[1]) / 1000;    // для lng указана колонка в Comp    
                double? price = doc.Body.Double(i, docCompPars[2] + parShft);
                double wgt = 0.0;   //!!! времянка -- пока вес будем брать из Tekla 
                Comps.Add(new Component(str, lng, wgt, price));
            }
            Log.exit();
        }
        /// <summary>
        /// getComp(doc) - загружает Документ - прайс-лист комплектующих
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        /// <journal> 21.2.2016
        /// 24.2.2016 - выделил в отдельный модуль Components
        /// 27.2.2016 - оформил внутреннюю структуру Component и встроил в getComp(doc)
        ///             для разбора, где указана длина комплектующего, используем строку
        ///             вида <col>/<№ параметра в str> 
        /// </journal>
        public static void getComp(Docs doc)
        {
            Log.set("getComp(" + doc.name + ")");
            try
            {

            }
            catch
            {

            }


            ////List<int> docCompPars = Mtch.GetPars(doc.LoadDescription);
            //////-- заполнение массива комплектующих Comps
            ////Comps.Clear();
            ////for (int i = doc.i0; i <= doc.il; i++)
            ////{
            ////    string str = doc.Body.Strng(i, docCompPars[0]);
            ////    double lng = 0;
            ////    //-- разбор параметров LoadDescription
            ////    List<int> strPars = Mtch.GetPars(str);
            ////    string docDescr = doc.LoadDescription;
            ////    int parShft = 0;
            ////    while (docDescr.Contains('/'))
            ////    {
            ////        string[] s = doc.LoadDescription.Split('/');
            ////        List<int> c = Mtch.GetPars(s[0]);
            ////        int pCol = c[c.Count() - 1];    // колонка - последний параметр до '/'
            ////        List<int> p = Mtch.GetPars(s[1]);         
            ////        lng = strPars[p[0] - 1];    // длина заготовки = параметр в str; индекс - первое число после '/'
            ////        docDescr = docDescr.Replace("/", "");
            ////        parShft++;
            ////    }
            ////    if (lng == 0)
            ////        lng = doc.Body.Int(i, docCompPars[1])/1000;    // для lng указана колонка в Comp    
            ////    double? price = doc.Body.Double(i, docCompPars[2] + parShft);
            ////    double wgt = 0.0;   //!!! времянка -- пока вес будем брать из Tekla 
            ////    Comps.Add(new Component(str, lng, wgt, price));
            ////}
            Log.exit();
        }
        public static Docs UpgradeFrExcel(Docs doc, string strToDo)
        {
            Log.set("UpgradeFrExcel(" + doc.name + ", " + strToDo + ")");
            if (strToDo != "DelEqPar1") Log.FATAL("не написано!");
//!!            List<string> Comp = getComp(doc);
            //////int i = doc.i0;
            //////foreach (string s in Comp)
            //////{
            //////    string str = Lib.ToLat(s);
            //////    List<int> pars = Mtch.GetPars(s);
            //////    if (pars[0] == pars[1])
            //////    {        
            //////        string toDel = pars[0].ToString() + " x ";
            //////        str = str.Replace(toDel + toDel, toDel); 
            //////    }
            //////    doc.Body[i++, 1] = str;
            //////}
            //////doc.isChanged = true;
            //////Docs.saveDoc(doc);

            //for (int i = doc.i0, iComp = 0; i <= doc.il; i++)
            //{
            //    // doc.Body.Strng(i, 1) = Copm;
            //}
            Log.exit();
            return doc;
        }
    } // end class components
} // end namespace
