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
using System.Runtime.InteropServices;
using IfcEngineCS;
using IfcManager.Core;

using FileOp = match.FileOp.FileOp;
using ElmAttributes = TSmatch.ElmAttSet;

namespace TSmatch.IFC
{
    public class IFC
    {
        static string schemaName;
        public static void Start(string _schemaName)
        {
            schemaName = _schemaName;
        }
        public static List<ElmAttributes.ElmAttSet> Read(string dir, string FileName)
        { return Read(Path.Combine(dir, FileName)); }    
        public static List<ElmAttributes.ElmAttSet> Read(string ifcFileName)
        {
            var manager = new IfcManager.Core.IfcManager();

            //string dir = Path.GetDirectoryName(ifcFileName);
            //string nam = Path.GetFileName(ifcFileName);
            //FileOp.fileOpen(dir, nam);

            manager.init(ifcFileName, schemaName);

            List<String> objectNameListNet = manager.getElementsByProperty("NetVolume");
 //           List<String> objectNameList = manager.getElementsByProperty("Volume");

///            List<String> objectNameListGross = manager.getElementsByProperty("GrossVolume");
            //            printList(objectNameList);
            return null;
        }
    } // end class TSmatch.IFC

/* ***********************************************
 * author :  Oleg Turetskiy
 * email  :  olegtster@gmail.com
 * file   :  IfcManager
 * history:  created by Oleg Turetskiy at 05/29/2016 15:00:54
 *           modified by Pavel Khrapkin 2.6.2016
 * ***********************************************/
namespace IfcManager.Core
    {
        public class IfcManager
        {
            IfcEngine _ifcEngine = null;
            IntPtr _ifcModel = IntPtr.Zero;
            String path = String.Empty;

            #region init
            public void init(string ifcFile, string ifcSchema)
            {
                _ifcEngine = new IfcEngine();
                _ifcModel = _ifcEngine.OpenModel(IntPtr.Zero, ifcFile, ifcSchema);
                #region commented Oleg' code
                //if (!String.Empty.Equals(ifcFilePath))
                //{
                //    _ifcEngine = new IfcEngine();
                //    path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

                //    // open model
                //    _ifcModel = _ifcEngine.OpenModel(IntPtr.Zero, @ifcFilePath, @String.Concat(path, @"/IFC2X3_TC1.exp"));
                //    if (IntPtr.Zero.Equals(_ifcModel))
                //    {
                //        throw new Exception("Error: incorrect file name");
                //    }
                //}
                //else
                //{
                //    throw new Exception("Error: incorrect file name");
                //}
                #endregion
            }
            #endregion init

            #region closeModel
            public void closeModel()
            {
                if (!IntPtr.Zero.Equals(_ifcModel))
                {
                    //close model
                    _ifcEngine.CloseModel(_ifcModel);
                }
            }
            #endregion closeModel

            #region convertIfcFile
            public void convertIfcFile(String ifcFilePath, String Arg3 = "-ifcxml")
            {
                System.Console.WriteLine("begin convert");
                if (_ifcModel != IntPtr.Zero && !String.Empty.Equals(path))
                {
                    switch (Arg3)
                    {
                        case "-xml": // for IFC4
                            String fullFileName = String.Concat(System.IO.Path.GetFileNameWithoutExtension(ifcFilePath), ".xml");
                            _ifcEngine.SaveModelAsSimpleXmlUnicode(_ifcModel, @String.Concat(path, @"/" + fullFileName));
                            break;
                        case "-ifcxml": // for IFC2X3
                            fullFileName = String.Concat(System.IO.Path.GetFileNameWithoutExtension(ifcFilePath), ".ifcxml");
                            _ifcEngine.SaveModelAsXmlUnicode(_ifcModel, @String.Concat(path, @"/" + fullFileName));
                            break;
                        default: throw new Exception("incorrect output file format");
                    }

                }
                System.Console.WriteLine("end convert");
            }
            #endregion convertIfcFile

            #region getElementsByProperty
            public List<String> getElementsByProperty(String propertyName)
            {
                var objectNameList = new List<String>();
                if (null != propertyName && !String.Empty.Equals(propertyName))
                {
                    System.Console.WriteLine("begin process");

                    var propertyInstance = IntPtr.Zero;
                    List<IntPtr> listPropSets = new List<IntPtr>();
                    if ((propertyInstance = findProperty(propertyName)) != IntPtr.Zero)
                    {
                        if ((listPropSets = findPropertySets(propertyInstance)).Count() > 0)
                        {
                            objectNameList = createResult(findElements(listPropSets));
                        }
                    }
                    else
                    {
                        throw new Exception("Model doesn't contain the property with name: " + propertyName);
                    }

                    System.Console.WriteLine("end process");
                }
                else
                {
                    throw new Exception("Property name is incorrect");
                }
                return objectNameList;
            }

            // find property
            private IntPtr findProperty(String strPropertyName)
            {
                IntPtr iEntitiesCount;
//16/6/16                IntPtr properties = getAggregator("ifcPropertySingleValue", out iEntitiesCount);
                IntPtr properties = getAggregator("ifcQuantityVolume", out iEntitiesCount); //!!16/6/16
                IntPtr propertyInstance = IntPtr.Zero;
                foreach (IntPtr iPropertyInstance in findEntity(properties, iEntitiesCount))
                {
                    String strName = getAttrValueAsString(iPropertyInstance, "Name");
                    if (strPropertyName.Equals(strName))
                    {
                        propertyInstance = iPropertyInstance;
                        break;
                    }
                    else
                    {
                        propertyInstance = IntPtr.Zero;
                    }
                }
                return propertyInstance;
            }

            // find propertySet that contains the property
            private List<IntPtr> findPropertySets(IntPtr iPropertyInstance)
            {
                var listPropSetInst = new List<IntPtr>();
                if (iPropertyInstance != IntPtr.Zero)
                {
                    IntPtr iPropertySetsCount;
                    IntPtr propertySets = getAggregator("ifcPropertySet", out iPropertySetsCount);
                    foreach (IntPtr iPropertySetInstance in findEntity(propertySets, iPropertySetsCount))
                    {
                        IntPtr propertiesInstance;
                        _ifcEngine.GetAttribute(iPropertySetInstance, "HasProperties", IfcEngine.SdaiType.Aggregation, out propertiesInstance);
                        if (propertiesInstance != IntPtr.Zero)
                        {
                            var iPropertiesCount = _ifcEngine.GetMemberCount(propertiesInstance);
                            foreach (IntPtr iPropertyInst in findEntity(propertiesInstance, iPropertiesCount))
                            {
                                if (iPropertyInst.Equals(iPropertyInstance))
                                {
                                    listPropSetInst.Add(iPropertySetInstance);
                                }
                            }
                        }
                    }
                }
                return listPropSetInst;
            }

            private List<IntPtr> findElements(List<IntPtr> listPropSets)
            {
                var objectList = new List<IntPtr>();
                foreach (IntPtr iPropertySetInstance in listPropSets)
                {
                    // find element that contains the propertySet
                    if (iPropertySetInstance != IntPtr.Zero)
                    {
                        IntPtr iEntityCount;
                        IntPtr relDefProperties = getAggregator("ifcRelDefinesByProperties", out iEntityCount);
                        foreach (IntPtr iRelDefPropInstance in findEntity(relDefProperties, iEntityCount))
                        {
                            IntPtr propertySetDef;
                            _ifcEngine.GetAttribute(iRelDefPropInstance, "RelatingPropertyDefinition", IfcEngine.SdaiType.Instance, out propertySetDef);

                            if (propertySetDef.Equals(iPropertySetInstance))
                            {
                                IntPtr objectInstances;
                                _ifcEngine.GetAttribute(iRelDefPropInstance, "RelatedObjects", IfcEngine.SdaiType.Aggregation, out objectInstances);
                                var iObjectCount = _ifcEngine.GetMemberCount(objectInstances);
                                foreach (IntPtr iObjectInstance in findEntity(objectInstances, iObjectCount))
                                {
                                    objectList.Add(iObjectInstance);
                                }
                            }
                        }
                    }
                }
                return objectList;
            }
            #endregion getElementsByProperty

            #region utilities
            private System.Collections.Generic.IEnumerable<IntPtr> findEntity(IntPtr entities, IntPtr entityCount)
            {
                for (int iEntity = 0; iEntity < entityCount.ToInt32(); iEntity++)
                {
                    IntPtr iEntityInstance = IntPtr.Zero;
                    _ifcEngine.GetAggregationElement(entities, iEntity, IfcEngine.SdaiType.Instance, out iEntityInstance);
                    yield return iEntityInstance;
                }
            }

            private IntPtr getAggregator(String entityName, out IntPtr memberCount)
            {
                IntPtr aggregator = _ifcEngine.GetEntityExtent(_ifcModel, entityName);
                memberCount = _ifcEngine.GetMemberCount(aggregator);
                return aggregator;
            }

            private String getAttrValueAsString(IntPtr iObjectInstance, String attrName)
            {
                IntPtr name;
                _ifcEngine.GetAttribute(iObjectInstance, attrName, IfcEngine.SdaiType.Unicode, out name);
                return Marshal.PtrToStringUni(name);
            }

            private List<String> createResult(List<IntPtr> objectList, String propValue = "")
            {
                var result = new List<String>();
                objectList.ForEach(objectInstance => result.Add(
                    "Object Name: " +
                    getAttrValueAsString(objectInstance, "Name") + " " +
                    "GUID: " +
                    getAttrValueAsString(objectInstance, "GlobalId") + " " +
                    (propValue.Equals("") ? "" : "Property Value: " + propValue)));
                return result;
            }
            #endregion utilities
        }
    }
} // end namespace
