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

//8/10 using Msg = TSmatch.Message.Message;

namespace UT_TSmatch
{
    public class _UT_Msg : TSmatch.Message.Message
    {
        public string msg, errType;

        public _UT_Msg(bool dialog = false) { DDialog = dialog; }
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
            try { ttxt(str, p); }
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

        public string GetMsg() { return TSmatch.Message.Message.msg; }
        public string GetErrType() { return TSmatch.Message.Message.errType; }

        //           Assert.ThrowsException() -- пока не умею этим пользоваться

        // try { resxError(ResErr.NoFile, "TSmatch.xlsx"); } catch { } U.GetTxt();
        // Assert.AreEqual("[Bootstrap.resError]: Не найден файл \"TSmatch.xlsx\"", U.msg);

        //       public delegate string mes(Enum x, string str);

        public string Try2(Func<int, string> func, int p1, string p2)
        {
            ////try { func(p1, p2); }
            ////catch { }
            ////GetTxt();
            return msg;

        }

        public string Msg_S(string str, params object[] p) { return _S(str, p); }
        public void Msg_W(string str, params object[] p) { _txt(str, p); }
        public void Msg_F(string str, params object[] p) { _txt(str, p); }
        public void Msg_I(string str, params object[] p) { _txt(str, p); }
    }
}
