/*=======================================================
* _UT_Service - common service module for all Unit Tests
* 
* 13.09.2017 Pavel Khrapkin 2.8.2017
*========================================================
*/
using System;
using System.Globalization;
using System.Resources;

using Msg = TSmatch.Message.Message;

namespace UT_TSmatch
{
    public class _UT_Msg : Msg
    {
        public string msg, errType;

        public _UT_Msg(bool dialog = false) { Dialog = dialog; }
        public int cnt_msgs() { return Msg._messages.Count; }

        public void SetCulture(string culture, bool dialog = false)
        {
            var newCulture = CultureInfo.GetCultureInfo(culture);
            _messages.Clear();
            ResourceSet set = mgr.GetResourceSet(newCulture, true, true);
            foreach (System.Collections.DictionaryEntry o in set)
            {
                _messages.Add(o.Key as string, o.Value as string);
            }
            mgr.ReleaseAllResources();
            Dialog = dialog;
        }

        internal void GetTxt()
        {
            msg = Msg.msg;
            errType = Msg.errType;
        }
    }
}
