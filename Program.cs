using System;
using System.Windows.Forms;
//-- мои модули
using Log = match.Lib.Log;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Matcher;

namespace TSmatch
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {     
            Log.START("TSmatch v12.03.2016");
            Mtch.Start();

//            Mod.openModel();
            //Docs doc = Docs.getDoc("Уголок Стальхолдинг??");
            //Cmp.UpgradeFrExcel(doc, "DelEqPar1");

            Mod.UpdateFrTekla();
            Mtch.UseRules();
//            Console.ReadLine();
        }
    } // end class
} // end namespace
