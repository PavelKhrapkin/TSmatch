/*-----------------------------------------------------------------------
 * IFC -- Interaction with model in IFC file.
 * 
 * 21.8.2017  Oleg Turetsky, Pavel Khrapkin
 *  
 *------ ToDo ----------
 * -ISSUE- в Read на входе "Desktop\MyColumb", а открывается файл "\Desktop\out.ifc"
 * -ISSUE- в файле MyColumn getElementsByProperty("NetVolume") возвращает Count = 0
 * - разобраться по документации и out.ifc с получением Профиля = IFCMEMBERTYPE
 *----- History ------------------------------------------
 * 13.5.2016 PKh start IFCenfine.dll use. Contact with Peter Bomsoms@rdf.bg http://rdf.bg/downloads/ifcengine-20160428.zip
 * 15.5.2016 Contact with Ph.D Lin Jiarui in Bejin ifcEngineCS https://github.com/LinJiarui/IfcEngineCS
 * 31.5.2016 Oleg Turetsky made sample based on incEngineCS. PKh started IFC class implementation for TSmatch
 *  1.8.2016 Oleg has changed IfcManager code
 * 19.8.2016 Implemented IAdapterCAD interface
 * 21.8.2017 CheckIfcGuid
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
using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using Elms = TSmatch.ElmAttSet.ElmAttSet;
using ElmAttributes = TSmatch.ElmAttSet;
using TSmatch.IFC.IfcManager.Core;

namespace TSmatch.IFC
{

    public class IFC : IAdapterCAD
    {
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("IFC");

        static string schemaName;
        public void init(string _schemaName)
        {
            schemaName = _schemaName;
//12/11            if (string.IsNullOrEmpty(schemaName)) Msg.F("IFC.init: No schema");
        }
        public static List<Elms> Read(string dir, string FileName)
        { return Read(Path.Combine(dir, FileName)); }

        public static List<Elms> Read(string ifcFileName)
        {
            var manager = new IfcManager.Core.IfcManager();

//12/11            if (!FileOp.isFileExist(ifcFileName)) Msg.F("IFC.Read: no file", ifcFileName);

            log.Info("TRACE: Read(\"" + ifcFileName + "\"");

            manager.init(ifcFileName, schemaName);

            List<IfcManager.Core.IfcManager.IfcElement> elements = new List<IfcManager.Core.IfcManager.IfcElement>();
            elements = manager.getElementsByProperty("NetVolume");
            IFC.MergeIfcToElmAttSet(elements);

            elements.Clear();
            elements = manager.getElementsByProperty("Weight");
            IFC.MergeIfcToElmAttSet(elements);

            elements.Clear();
            elements = manager.getElementsByMaterials();
            IFC.MergeIfcToElmAttSet(elements);

            elements.Clear();
            elements = manager.getElementsByProperty("Profile");
            IFC.MergeIfcToElmAttSet(elements);

            List<ElmAttributes.ElmAttSet> result = new List<ElmAttributes.ElmAttSet>();
            result = ElmAttributes.ElmAttSet.Elements.Values.ToList();
            foreach (var elm in result)
            {
                string[] matToSplit = elm.mat.Split('/');
                switch (matToSplit.Count())
                {
                    case 2:
                        elm.mat_type = matToSplit[0];
                        elm.mat = matToSplit[1];
                        break;
                    case 1:
                        elm.mat_type = "STEEL";
                        elm.prf = elm.mat;  // А400 - это арматура; почемуто ее марку указывают как материал
                                            //..здесь еще надо разобраться с ГОСТ-5781 
                                            //..и присвоить значения элемента mat, prf и др
                        break;
//12/11                    default: Msg.F("IFC error Material Parse", elm.mat);
                        break;
                }
            }
            result.Sort();
            return result;
        }

        private static List<ElmAttributes.ElmAttSet> MergeIfcToElmAttSet(List<IfcManager.Core.IfcManager.IfcElement> elements)
        {
            foreach (var ifc_elm in elements)
            {
                string guid = ifc_elm.guid;
                if (!ElmAttSet.ElmAttSet.Elements.ContainsKey(guid))
                {
                    ElmAttSet.ElmAttSet new_elm = new ElmAttSet.ElmAttSet(ifc_elm);
//                    new_elm = new ElmAttSet.ElmAttSet(ifcElemOrg);
                }
                else
                {
                    ElmAttributes.ElmAttSet elm = null;
                    ElmAttSet.ElmAttSet.Elements.TryGetValue(guid, out elm);
                    if (!String.IsNullOrEmpty(ifc_elm.material)) elm.mat = ifc_elm.material;
                    if (!String.IsNullOrEmpty(ifc_elm.type_material)) elm.mat_type = ifc_elm.type_material;
                    if (!String.IsNullOrEmpty(ifc_elm.profile)) elm.prf = ifc_elm.profile;
                    if (!String.IsNullOrEmpty(ifc_elm.length)) elm.length = Lib.ToDouble(ifc_elm.length);
                    if (!String.IsNullOrEmpty(ifc_elm.weight)) elm.weight = Lib.ToDouble(ifc_elm.weight);
                    if (!String.IsNullOrEmpty(ifc_elm.volume)) elm.volume = Lib.ToDouble(ifc_elm.volume);
                    if (!String.IsNullOrEmpty(ifc_elm.price)) elm.price = Lib.ToDouble(ifc_elm.price);
                }
            }
            return ElmAttSet.ElmAttSet.Elements.Values.ToList();
        }

        /// <summary>
        /// CheckIfcGuids() - read Ifc file and check GUIDs of elements in it for Werfau needs
        /// </summary>
        internal void CheckIfcGuids(string ifcFile)
        {
//12/11            if (!FileOp.isFileExist(ifcFile)) Msg.F("No Ifc input file", ifcFile);
            List<Elms> elements = new List<Elms>();
            elements = Read(ifcFile);
            throw new NotImplementedException();
        }

        public List<ElmAttributes.ElmAttSet> Read()
        {
            throw new NotImplementedException();
        }

        public string getModelDir()
        {
            throw new NotImplementedException();
        }

        public string getModelName()
        {
            throw new NotImplementedException();
        }

        public string getModelMD5()
        {
            throw new NotImplementedException();
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
            private static readonly log4net.ILog log = log4net.LogManager.GetLogger("IfcManager");

            public const string NETVOLUME = "NetVolume";

            IfcEngine _ifcEngine = null;
            IntPtr _ifcModel = IntPtr.Zero;
            String path = String.Empty;

            public Dictionary<string, string> ifcProp2IfcPropType { get; set; }
            public Dictionary<string, string> ifcProp2IfcPropValueType { get; set; }
            public Dictionary<string, string> ifcPropSetTypeByIfcPropType{ get; set; }
            public Dictionary<string, string> ifcElemntContainerTypeByIfcPropType { get; set; }

            #region init
            public void init(string ifcFile, string ifcSchema)
            {
                _ifcEngine = new IfcEngine();
                _ifcModel = _ifcEngine.OpenModel(IntPtr.Zero, ifcFile, ifcSchema);

                ifcProp2IfcPropType = new Dictionary<string, string>();
                ifcProp2IfcPropValueType = new Dictionary<string, string>();
                ifcPropSetTypeByIfcPropType = new Dictionary<string, string>();
                ifcElemntContainerTypeByIfcPropType = new Dictionary<string, string>();

                ifcProp2IfcPropType.Add("NetVolume", "IfcQuantityVolume");
                ifcProp2IfcPropValueType.Add("NetVolume", "VolumeValue");

                ifcProp2IfcPropType.Add("Weight", "IfcPropertySingleValue");
                ifcProp2IfcPropValueType.Add("Weight", "NominalValue");

                ifcProp2IfcPropType.Add("Profile", "IfcPropertySingleValue");
                ifcProp2IfcPropValueType.Add("Profile", "NominalValue");

                ifcPropSetTypeByIfcPropType.Add("IfcQuantityVolume", "IfcElementQuantity");
                ifcPropSetTypeByIfcPropType.Add("IfcPropertySingleValue", "IfcPropertySet");


                ifcElemntContainerTypeByIfcPropType.Add("IfcElementQuantity", "Quantities");
                ifcElemntContainerTypeByIfcPropType.Add("IfcPropertySet", "hasProperties");
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
                return Marshal.PtrToStringAuto(name);
            }

            private string getInstanceType(IntPtr instance)
            {
                IntPtr ifcTypeIns = _ifcEngine.GetInstanceType(instance);
                IntPtr name = IntPtr.Zero;
                _ifcEngine.GetEntityName(ifcTypeIns, IfcEngine.SdaiType.String, out name);
                return Marshal.PtrToStringAnsi(name);
            }
  
            #endregion utilities

            #region Interface class
            public class IfcElement
            {
                public string name;
                public string guid;
                public string material;
                public string type_material;
                public string profile;
                public string length;
                public string weight;
                public string volume;
                public string price;
            }
            #endregion Interface class

            #region getElementsByProperty
            public List<IfcElement> getElementsByProperty(string propertyName)
            {
                var result = new List<IfcElement>();
                String propValue = string.Empty;
                var propertyInstances = findProperty(propertyName);
                if (propertyInstances.Count > 0)
                {
                    foreach (KeyValuePair<IntPtr, string> pair in propertyInstances)
                    {
                        var listPropSets = findPropertySets(pair.Key/*, propertySetType, containerName*/);
                        if (listPropSets.Count() > 0)
                        {
                            var elements = findElements(listPropSets);
                            result.AddRange(createRes(elements, propertyName, pair.Value));
                        }
                    }
                }
                else
                {
                    throw new Exception("Model doesn't contain the property with name: " + propertyName);
                }
                return result;
            }

            private List<IfcElement> createRes(List<IntPtr> elements, string propertyName, string propertyValue)
            {
                List<IfcElement> result = new List<IfcElement>();
                foreach(var element in elements)
                {
                    IfcElement ifcElement = new IfcElement();

                    ifcElement.name = getAttrValueAsString(element, "Name");
                    ifcElement.guid = getAttrValueAsString(element, "GlobalId");
                    switch (propertyName)
                    {
                        case NETVOLUME:
                            ifcElement.volume = propertyValue;
                            break;
                        case "Weight":
                            ifcElement.weight = propertyValue;
                            break;
                        case "Material":
                            ifcElement.material = propertyValue;
                            break;
                        case "Profile":
                            ifcElement.profile = propertyValue;
                            break;
                    }
                    result.Add(ifcElement);
                }
                return result;
            }

            private List<KeyValuePair<IntPtr, string>> findProperty(String strPropertyName)
            {
                IntPtr iEntitiesCount;
                string propertyType = string.Empty;
                var propertyInstances = new List<KeyValuePair<IntPtr, string>>();

                ifcProp2IfcPropType.TryGetValue(strPropertyName, out propertyType);
                if (!string.IsNullOrEmpty(propertyType))
                {
                    IntPtr properties = getAggregator(propertyType, out iEntitiesCount);
                    
                    foreach (IntPtr iPropertyInstance in findEntity(properties, iEntitiesCount))
                    {
                        String strName = getAttrValueAsString(iPropertyInstance, "Name");
                        if (strPropertyName.Equals(strName))
                        {
                            string propValueType = string.Empty;
                            ifcProp2IfcPropValueType.TryGetValue(strPropertyName, out propValueType);
                            if (!string.IsNullOrEmpty(propValueType))
                            {
                                string propValue = getAttrValueAsString(iPropertyInstance, propValueType);
                                if (strPropertyName.Equals("Profile"))
                                {
                                    propertyInstances.Add(new KeyValuePair<IntPtr, string>(iPropertyInstance, propValue));
                                }
                                else
                                {
                                    double x = Lib.ToDouble(propValue);
                                    if (x != 0.0)
                                    {
                                        propertyInstances.Add(new KeyValuePair<IntPtr, string>(iPropertyInstance, propValue));
                                    }
                                }
                            }
                        }
                    }
                }
                return propertyInstances;
            }

            private List<IntPtr> findPropertySets(IntPtr iPropertyInstance /*, string propertySetType, string containerName */)
            {
                var listPropSetInst = new List<IntPtr>();
                string propType = getInstanceType(iPropertyInstance);
                string propertySetType = string.Empty;
                ifcPropSetTypeByIfcPropType.TryGetValue(propType, out propertySetType);

                if (iPropertyInstance != IntPtr.Zero)
                {
                    IntPtr iPropertySetsCount;
                    IntPtr propertySets = getAggregator(propertySetType, out iPropertySetsCount);
                    foreach (IntPtr iPropertySetInstance in findEntity(propertySets, iPropertySetsCount))
                    {
                        IntPtr propertiesInstance;
                        string propSetType = getInstanceType(iPropertySetInstance);
                        string containerName = string.Empty;
                        ifcElemntContainerTypeByIfcPropType.TryGetValue(propSetType, out containerName);
                        //                        _ifcEngine.GetAttribute(iPropertySetInstance, "HasProperties", IfcEngine.SdaiType.Aggregation, out propertiesInstance);
                        //_ifcEngine.GetAttribute(iPropertySetInstance, "Quantities", IfcEngine.SdaiType.Aggregation, out propertiesInstance);
                        _ifcEngine.GetAttribute(iPropertySetInstance, containerName, IfcEngine.SdaiType.Aggregation, out propertiesInstance);
                       
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
            #endregion getElementsByPropert

            #region getElementsByMaterials
            public List<IfcElement> getElementsByMaterials()
            {
                var result = new List<IfcElement>();
                IntPtr matCount = IntPtr.Zero;
                IntPtr materials = getAggregator("IfcMaterial", out matCount);
                for(int iEntity = 0; iEntity < matCount.ToInt32(); iEntity++)
                {
                    IntPtr matInstance = IntPtr.Zero;
                    _ifcEngine.GetAggregationElement(materials, iEntity, IfcEngine.SdaiType.Instance, out matInstance);

                    string matValue = getAttrValueAsString(matInstance, "Name");

                    List<IntPtr> objects = getRelatedObjects(matInstance);
                    result.AddRange(createRes(objects, "Material", matValue));
                }
                return result;
            }
            public List<IntPtr> getRelatedObjects(IntPtr matInstance)
            {
                var result = new List<IntPtr>();
                var relAssCount = IntPtr.Zero;
                var relAss = IntPtr.Zero;
                IntPtr relAssociatesMats = getAggregator("IfcRelAssociatesMaterial", out relAssCount);
                for( int i=0; i < relAssCount.ToInt32(); i++ )
                {
                    _ifcEngine.GetAggregationElement(relAssociatesMats, i, IfcEngine.SdaiType.Instance, out relAss);
                    IntPtr relatingMaterial;
                    _ifcEngine.GetAttribute(relAss, "RelatingMaterial", IfcEngine.SdaiType.Instance, out relatingMaterial);
                    if (matInstance.Equals(relatingMaterial))
                    {
                        IntPtr objectInstances;
                        _ifcEngine.GetAttribute(relAss, "RelatedObjects", IfcEngine.SdaiType.Aggregation, out objectInstances);
                        var iObjectCount = _ifcEngine.GetMemberCount(objectInstances);
                        foreach (IntPtr iObjectInstance in findEntity(objectInstances, iObjectCount))
                        {
                            result.Add(iObjectInstance);
                        }
                    }
                }
                return result;
            }
            #endregion getElementsByMaterials
        }
    }
} // end namespace
