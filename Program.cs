using System;
using System.Windows.Forms;
using log4net;
using log4net.Config;

//-- мои модули
using Decl = TSmatch.Declaration.Declaration;
using Log = match.Lib.Log;
using BootMode = TSmatch.Bootstrap.Bootstrap.BootInitMode;
//using TSmatch.Startup.Bootstrap;
//using Model = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Mtch;
using Supplier = TSmatch.Suppliers.Supplier;
using System.Collections.Generic;

[assembly: XmlConfigurator(Watch = true)]

namespace TSmatch
{
    class Program
    {
        public static readonly ILog log = LogManager.GetLogger("Program");  //(typeof(Program));

        [STAThread]
        static void Main(string[] args)
        {
            Log.START("TSmatch v2016.12.22");

            var bootstrap = new Bootstrap.Bootstrap();
            var model = (Model.Model) bootstrap.init(BootMode.Model);
            model.Read();   // Read() - загружаем модель, последнюю по времени или ту, что в САПР
            model.Handler();
            model.Report();


            //            model.Read("out.ifc");    // файл "out.ifc" создан Tekla Артемом Литвиновым только из стандартных элементов. IFC его не может прочитать
            //            model.Read("MyColumn.ifc");   // IFC не парсируется
            //--6.08.2016 model.Read("out.ifc")
            //model.Read("out3314.ifc");         // open most recent model - in Tekla, Excel File, or IFC file 

            //            Supplier.SupplReport();

            //Docs doc = Docs.getDoc("Уголок Стальхолдинг??");
            //Cmp.UpgradeFrExcel(doc, "DelEqPar1");

            //6/8/16            Mod mod = Mod.UpdateFrTekla();
            //            Mtch.UseRules(mod);
            //            Console.ReadLine();
            Console.ReadKey();
        }
    } // end class
} // end namespace
