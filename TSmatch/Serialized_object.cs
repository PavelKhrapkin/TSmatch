/*------------------------------------------------------------------------------
 * Serialyzed_object containes serialysed list of obect for MD5 key generation
 * 
 *  21.07.2017 П.Л. Храпкин
 *  
 *--- Unit Tests ---
 * UT_Model/UT_get_pricingMD5 2017.07.21 OK
 *--- History  ---
 *  7.05.2017 created ansed with MD5gen with elements
 *  9.05.2017 Serialiesd_Group add with similar code
 * 21.07.2017 class Match found is not Serializable -- changed Serialized_Group field set
 * ----------------------------------------------------------------------------
 *      Classes & constructors:
 * Serialyzed_elements(elements)    - prepare MD5 key calculation of List<ElmAttSet> elements
 * Serialyzed_Groups(elementGroups) - similar preparaion fer Groups with Matches and Rules
 */
using System;
using System.Collections.Generic;

using Elm = TSmatch.ElmAttSet.ElmAttSet;
using ElmGr = TSmatch.Group.Group;

namespace TSmatch.Model
{
    [Serializable]
    class Serialized_element
    {
        public string id, mat, mat_type, prf;
        public double wgt, lng, vol, pr;

        public Serialized_element(Elm elm) 
        {
            id = elm.guid;
            mat = elm.mat;
            mat_type = elm.mat_type;
            prf = elm.prf;
            vol = elm.volume;
            wgt = elm.weight;
            lng = elm.length;
            pr = elm.price;
        }
    }

    [Serializable]
    class Serialized_Group
    {
        public List<string> guids = new List<string>();
        public string mat, prf, Mat, Prf;
        public double wgt, lng, vol, pr;
        public DateTime Rule_date, CompSet_date;
        public string Supplier_name, CompSet_name;
        public string LoadDescriptor, Rule_Text;

        public Serialized_Group(ElmGr elmGr)
        {
            guids = elmGr.guids;
            mat = elmGr.mat;
            Mat = elmGr.Mat;
            prf = elmGr.prf;
            Prf = elmGr.Prf;
            wgt = elmGr.totalWeight;
            lng = elmGr.totalLength;
            vol = elmGr.totalVolume;
            pr = elmGr.totalPrice;
            Supplier_name = elmGr.SupplierName;
            CompSet_name = elmGr.CompSetName;
            try
            {
                Rule_Text = elmGr.match.rule.text;
                LoadDescriptor = elmGr.match.rule.CompSet.doc.LoadDescription;
            }
            catch
            {
                Rule_Text = LoadDescriptor = string.Empty;
                Rule_date = DateTime.MinValue;
            }
        }
    }

}