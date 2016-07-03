using System;
using System.Windows.Forms;
//-- мои модули
using Ifc = TSmatch.IFC.IFC;  //Debug -- later -- remove

using Log = match.Lib.Log;
using Boot = TSmatch.Startup.Bootstrap;
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
            Log.START("TSmatch v22.06.2016");
            Boot.Bootsrap();

            Mod.openModel();        // open most recent model - in Tekla, Excel File, orIFC file 

            //            Ifc.Read(@"F:\Pavel\match\matchCodes\С#\TSmatch\Look_around\Oleg\IfcManagerAppl\IfcManagerAppl\bin\Debug\out-2.ifc");

            Supplier.SupplReport();

            //Docs doc = Docs.getDoc("Уголок Стальхолдинг??");
            //Cmp.UpgradeFrExcel(doc, "DelEqPar1");

            Mod mod = Mod.UpdateFrTekla();
//            Mtch.UseRules(mod);
//            Console.ReadLine();
        }
    } // end class
} // end namespace
