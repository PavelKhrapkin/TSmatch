/*-----------------------------------------------------------------------
 * IFC -- Interaction with model in IFC file.
 * 
 * 31.5.2016  Pavel Khrapkin
 *  
 *----- History ------------------------------------------
 * 13.5.2016 PKh start IFCenfine.dll use. Contact with Peter Bomsoms@rdf.bg http://rdf.bg/downloads/ifcengine-20160428.zip
 * 15.5.2016 Contact with Ph.D Lin Jiarui in Bejin ifcEngineCS https://github.com/LinJiarui/IfcEngineCS
 * 31.5.2016 Oleg Turetsky made sample based on incEngineCS. PKh started IFC class implementation for TSmatch
 * -------------------------------------------
 * public Structure AttSet - set of model component attribuyes, extracted from Tekla by method Read
 *                           AttSet is Comparable, means Sort is applicable, and 
 *
 *      METHPDS:
 * Read()           - read current model from Tekla, return List<AttSet> - list of this model attributes
 *                    AttSet contains Materins, Profile, Weight, Volume etc
 * ModAtrMD5()      - calculate MD5 - contol sum of the current model
 * GetIFCDir(mode) - return Path to the model directory, or Path to exceldesign in Tekla environmen
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IfcManager.Core;

using FileOp = match.FileOp.FileOp;
using ElmAttributes = TSmatch.ElmAttSet.ElmAttSet;

namespace TSmatch.IFC
{
    public class IFC
    {
        public static void Start()
        {

        }
        
        public static List<ElmAttributes> Read(string FileName)
        {
            var manager = new IfcManager.Core.IfcManager();

            string dir = Path.GetDirectoryName(FileName);
            string nam = Path.GetFileName(FileName);
            FileOp.fileOpen(dir, nam);

            List<String> objectNameList = manager.getElementsByProperty("Volume");
//            printList(objectNameList);
            return null;
        }
    } // end class TSmatch.IFC
} // end namespace
