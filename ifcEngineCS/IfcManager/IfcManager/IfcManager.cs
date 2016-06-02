/* ***********************************************
 * author :  Oleg Turetskiy
 * email  :  olegtster@gmail.com
 * file   :  IfcManager
 * history:  created by Oleg Turetskiy at 05/29/2016 15:00:54
 *           modified by
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using IfcEngineCS;

namespace IfcManager.Core
{
    public class IfcManager
    {
        IfcEngine _ifcEngine = null;
        IntPtr _ifcModel = IntPtr.Zero;
        String path = String.Empty;

        #region init
        public void init(String ifcFilePath)
        {
            if (!String.Empty.Equals(ifcFilePath))
            {
                _ifcEngine = new IfcEngine();
                path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                
                // open model
                _ifcModel = _ifcEngine.OpenModel(IntPtr.Zero, @ifcFilePath, @String.Concat(path, @"/IFC2X3_TC1.exp"));
                if (IntPtr.Zero.Equals(_ifcModel))
                {
                    throw new Exception("Error: incorrect file name");
                }
            }
            else
            {
                throw new Exception("Error: incorrect file name");
            }
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
                    case "-ifcxml" : // for IFC2X3
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
        public List<String> getElementsByProperty(String Arg3)
        {
            var objectNameList = new List<String>();
            if (null != Arg3 && !String.Empty.Equals(Arg3))
            {
                System.Console.WriteLine("begin process");

                var propertyInstance = IntPtr.Zero;
                List<IntPtr> listPropSets = new List<IntPtr>();
                if ((propertyInstance = findProperty(Arg3)) != IntPtr.Zero)
                {
                    if ((listPropSets = findPropertySets(propertyInstance)).Count() > 0)
                    {
                        objectNameList = createResult(findElements(listPropSets));
                    }
                }
                else
                {
                    throw new Exception("Model doesn't contain the property with name: " + Arg3);
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
            IntPtr properties = getAggregator("ifcPropertySingleValue", out iEntitiesCount);
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

        private List<String> createResult(List<IntPtr> objectList)
        {
            var result = new List<String>();
            objectList.ForEach(objectInstance => result.Add(getAttrValueAsString(objectInstance, "Name")));
            return result;
        }
        #endregion utilities
    }
}
