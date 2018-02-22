/* --------------------------------------------------
 * Lib - module - general purpases method library
 * 
 *  20.2.2018 Pavel Khrapkin
 *  
 *  --- History: ---
 *  5.2.2018 перенес в PriceResx
 * 15.2.2018 WrXML filemode append:false
 * 20.2.2018 WrXML check IfFileExist()
 * ------ Methods -------
 * ToLat(str)   - convert all characters of input string to Latin
 * GetMD5(obj)  - calculate MD5 of object
 * WrXML(path, file, obj)   - wrile serializable obj to XML file
 * WrXMP(filePath, obj)
 * RdXML(path, file, obj)   - read XML file to obj
 * IdGen(idType) - generete string Id for idType class
 */
using System;
using System.IO;
using System.Xml.Serialization;

namespace PriceMatch
{
    public class Lib
    {
        private FileOp fo = new FileOp();

        public static string ToLat(string str)
        {
            const string cyr = "АВЕКМНОРСТХаеорсух";
            const string lat = "ABEKMHOPCTXaeopcyx";
            if (string.IsNullOrEmpty(str)) return str;
            string lt = string.Empty;
            foreach (var s in str)
            {
                int ind = cyr.IndexOf(s);
                lt += ind < 0 ? s : lat[ind];
            }
            return lt;
        }

        public static string GetMD5(object obj)
            => MD5gen.MD5HashGenerator.GenerateKey(obj);

        public void WrXML(string filePath, dynamic obj)
        {
            string getExt = Path.GetExtension(filePath);
            if (getExt != ".xml")
                filePath = Path.ChangeExtension(filePath, "xml");
            XmlSerializer serialyzer = new XmlSerializer(obj.GetType());
            using (TextWriter writer = new StreamWriter(filePath, append: false))
            { serialyzer.Serialize(writer, obj); }
        }

        public void WrXML(string path, string file, dynamic obj)
            => WrXML(Path.Combine(path, file), obj);

        public T RdXML<T>(string filePath) where T : new()
        {
            string getExt = Path.GetExtension(filePath);
            if (getExt != ".xml")
                filePath = Path.ChangeExtension(filePath, "xml");
            if (!fo.IsFileExist(filePath))
                throw new System.Exception("No XML file");
            TextReader reader = null;
            try
            {
                var serialyzer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serialyzer.Deserialize(reader);
            }
            finally { if (reader != null) reader.Close(); }
        }

        public string IdGen(string idType)
            => idType + "_" + Path.GetRandomFileName()
                .Replace(".", "").Substring(0, 4);
    }
} // end namespace