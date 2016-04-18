using System;
using System.Windows.Forms;
//-- мои модули
using Log = match.Lib.Log;
using Mod = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Matcher;
using Supplier = TSmatch.Suppliers.Supplier;

namespace TSmatch
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {     
            Log.START("TSmatch v17.04.2016");
            TSmatch.Startup.Bootstrap.Bootsrap();

            Supplier.SupplReport();

//            Mod.openModel();
            //Docs doc = Docs.getDoc("Уголок Стальхолдинг??");
            //Cmp.UpgradeFrExcel(doc, "DelEqPar1");

            Mod mod = Mod.UpdateFrTekla();
//            Mtch.UseRules(mod);
//            Console.ReadLine();
        }
    } // end class
} // end namespace
