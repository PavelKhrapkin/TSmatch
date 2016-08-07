using System;
using System.Windows.Forms;
//-- мои модули
using Ifc = TSmatch.IFC.IFC;  //Debug -- later -- remove

using Log = match.Lib.Log;
//using TSmatch.Startup.Bootstrap;
//using Model = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Matcher;
using Supplier = TSmatch.Suppliers.Supplier;

namespace TSmatch
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {     
            Log.START("TSmatch v06.08.2016");

            var bootApp = new TSmatch.Startup.Bootstrap();
            bootApp.start();

            var model = (Model.Model) bootApp.init(Declaration.Declaration.MODEL);
//            model.Read("MyColumn.ifc");   // IFC не парсируется
            //--6.08.2016 model.Read("out.ifc") ниже написано для отладки IFC. В дальнейшем будет Read() - недавняя модель или выборка из каталога модели
            model.Read("out3314.ifc");         // open most recent model - in Tekla, Excel File, or IFC file 

            Supplier.SupplReport();

            //Docs doc = Docs.getDoc("Уголок Стальхолдинг??");
            //Cmp.UpgradeFrExcel(doc, "DelEqPar1");

//6/8/16            Mod mod = Mod.UpdateFrTekla();
            //            Mtch.UseRules(mod);
            //            Console.ReadLine();
        }
    } // end class
} // end namespace
