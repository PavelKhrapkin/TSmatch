﻿using System;
using System.Windows.Forms;
//-- мои модули
using Ifc = TSmatch.IFC.IFC;  //Debug -- later -- remove

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
            Log.START("TSmatch v31.05.2016");
            TSmatch.Startup.Bootstrap.Bootsrap();

            Ifc.Start();
            Ifc.Read(@"C:\TeklaStructuresModels\2016\ЗС2\IFC\out.ifc");

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
