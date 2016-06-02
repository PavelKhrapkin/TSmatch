/* ***********************************************
 * author :  Oleg Turetskiy
 * email  :  olegtster@gmail.com
 * file   :  IfcManager
 * history:  created by Oleg Turetskiy at 05/29/2016 15:10:23
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IfcManager.Core;

namespace IfcManagerAppl
{
    class IfcManagerAppl
    {
        static int Main(string[] args)
        {
            var manager = new IfcManager.Core.IfcManager();

//            args = new String[3];
//            args[0] = "out-2.ifc";
//            args[1] = "-p";
//            args[2] = "Weight";
//            args[2] = "-xml";

            if (args.Length < 2)
            {
                System.Console.WriteLine("Please enter file name.");
                System.Console.WriteLine("Usage: \tIfcManagerAppl <file name> -c [<-xml> or <-ifcxml>] // convert to ifcxml (for IFC2X3) or xml (for IFC4) \r\n" +
                                                 "\t\t\tor\r\n" +
                                                 "\tIfcManagerAppl <fileName> -p <propertyName> // find of element(s) containing property");
                return 1;
            }
            else
            {
                try
                {
                    manager.init(args[0]);
                    switch (args[1])
                    {
                        case "-c": manager.convertIfcFile(args[0], (args.Length> 2) ? args[2] : "-ifcxml");
                            break;
                        case "-p": 
                            List<String> objectNameList = manager.getElementsByProperty((args.Length> 2) ? args[2] : null);
                            printList(objectNameList);
                            break;
                        default: System.Console.WriteLine("Error: incorrect key");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    manager.closeModel();
                }
                return 0;
            }
        }

        static private void printList(List<String> objects) 
        {
            objects.ForEach(delegate(String name)
            {
                Console.WriteLine("Object: " + name);
            });
        }
    }
}
