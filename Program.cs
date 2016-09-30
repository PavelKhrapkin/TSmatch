using System;
using System.Windows.Forms;
using log4net;
using log4net.Config;

//-- мои модули
using Log = match.Lib.Log;
//using TSmatch.Startup.Bootstrap;
//using Model = TSmatch.Model.Model;
using Mtch = TSmatch.Matcher.Matcher;
using Supplier = TSmatch.Suppliers.Supplier;

[assembly: XmlConfigurator(Watch = true)]

namespace TSmatch
{
    class Program
    {
        public static readonly ILog log = LogManager.GetLogger("Program");  //(typeof(Program));

        [STAThread]
        static void Main(string[] args)
        {
            Log.START("TSmatch v20.08.2016");

            var bootApp = new Startup.Bootstrap();
            bootApp.start();

            var model = (Model.Model) bootApp.init(Declaration.Declaration.MODEL);
            model.Read();   // Read() - загружаем модель, последнюю по времени
            model.Handler();
            model.Report();


            //            model.Read("out.ifc");    // файл "out.ifc" создан Tekla Артемом Литвиновым только из стандартных элементов. IFC его не может прочитать
            //            model.Read("MyColumn.ifc");   // IFC не парсируется
            //--6.08.2016 model.Read("out.ifc")
            //model.Read("out3314.ifc");         // open most recent model - in Tekla, Excel File, or IFC file 

            Supplier.SupplReport();

            //Docs doc = Docs.getDoc("Уголок Стальхолдинг??");
            //Cmp.UpgradeFrExcel(doc, "DelEqPar1");

//6/8/16            Mod mod = Mod.UpdateFrTekla();
            //            Mtch.UseRules(mod);
            //            Console.ReadLine();
        }
    } // end class
} // end namespace
