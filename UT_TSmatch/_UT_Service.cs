/*=======================================================
* _UT_Service - common service module for all Unit Tests
* 
* 6.10.2017 Pavel Khrapkin
*========================================================
* History:
* 2017.10.6  - delegate TryMsg
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
        public int cnt_msgs() { return _messages.Count; }

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

        public void _txt(string str, params object[] p)
        {
            try { txt(str, p); }
            catch { }; // (ArgumentException e) when (e.Message == "Message_UT_NoDialog") { };
        }

        private string _S(string str, params object[] p)
        {
            bool d = Dialog;
            Dialog = true;
            string s = S(str, p);
            Dialog = d;
            return s;
        }

        internal void GetTxt()
        {
            msg = Msg.msg;
            errType = Msg.errType;
        }



// try { resxError(ResErr.NoFile, "TSmatch.xlsx"); } catch { } U.GetTxt();
// Assert.AreEqual("[Bootstrap.resError]: Не найден файл \"TSmatch.xlsx\"", U.msg);

        public string Try2(Func<object, object, string> func, object p1, object p2)
        {
            try { func(p1, p2); }
            catch { }
            GetTxt();
            return msg;

        }

        public string Msg_S(string str, params object[] p) { return _S(str, p); }
        public void Msg_W(string str, params object[] p) { _txt(str, p); }
        public void Msg_F(string str, params object[] p) { _txt(str, p); }
        public void Msg_I(string str, params object[] p) { _txt(str, p); }
    }
}
