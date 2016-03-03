using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//-- мои модули
using Decl = TSmatch.Declaration.Declaration;
using Log = match.Lib.Log;
using Docs = TSmatch.Document.Document;
using TS = TSmatch.Tekla.Tekla;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Matcher;
using Cmp = TSmatch.Component.Component;

namespace TSmatch
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.START("TSmatch v01.03.2016");
            Mtch.Start();

            //Docs doc = Docs.getDoc("Уголок Стальхолдинг");
            //Cmp.UpgradeFrExcel(doc, "DelEqPar1");

            Mod.UpdateFrTekla();
            Mtch.UseRules();
//            Console.ReadLine();
        }
    } // end class
} // end namespace
