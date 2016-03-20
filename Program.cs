using System;
using System.Windows.Forms;
//-- мои модули
using Log = match.Lib.Log;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Matcher;
using Boot = TSmatch.Startup.Bootstrap;

namespace TSmatch
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {     
            Log.START("TSmatch v19.03.2016");
            Boot.Bootsrap();
            Mtch.Start();

//            Mod.openModel();
            //Docs doc = Docs.getDoc("Уголок Стальхолдинг??");
            //Cmp.UpgradeFrExcel(doc, "DelEqPar1");

            Mod mod = Mod.UpdateFrTekla();
            Mtch.UseRules(mod);
//            Console.ReadLine();
        }
    } // end class
} // end namespace
