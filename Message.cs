/*----------------------------------------------------------------------------
 * Message -- multilanguage message system
 * 
 * 12.11.2017  Pavel Khrapkin
 *
 *--- Unit Tests ---
 * UT_Message: UT_Init, UT_AskS, UT_W, UT_S, UT_AskOK, UT_AskYN 25.09.2017 OK
 * --- History ---
 * Feb-2016 Created
 * 20.3.2016 - Error message Code display even when Message system is not initialysed yet
 * 20.8.2016 - use log4net, bug fixes
 *  9.5.2017 - MessageBox.Show use
 * 11.5.2017 - Fatal error handling with Application.Current.Sutdown, AskFOK, and SPLAS messages
 * 22.5.2017 - AskYN, Msg.OK()
 * 18.7.2017 - remake with Dictionary as a Messages store
 * 31.8.2017 - Msg.S return string; Dialog flag
 * 12.9.2017 - TSmatchMsg.resx and TSmatchMsg,ru.resx use as localization resources
 * 14.9.2017 - static constructor as a singleton for _messages fill; ArgumentException for UTs
 * 8.10.2017 - Instance F,W,I istead of static
 * 25.10.2017 - Exception instead of MessageBox in AskOK. AskYN for UT
 *  3.11.2017 - set Language from Property.Setting or from CultureInfo.CurrentCulture
 * 12.11.2017 - restore to non-static refactored code after excident 10.11
 * ---------------------------------------------------------------------------------------
 *      Methods:
 * static Message() - Singleton constructor initiate static msgs Dictionary set
 * F(Code,..) - Fatal error message output
 * W(Code,..) - Warning message output
 * I(Code,..) - Information Messag
 * S(Code,..) - return string made from Code and ".." arguments to output in WPF
 * AskFOK(text?) - ask text OK-Cancel, with Fatal/Stop at Cancel
 * AskYN(text?)  - ask with text as a prompt, and respont Yes or No
 * AskS(text:)   - request string entry with text as a prompt
 */
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Windows;
using FileOp = match.FileOp.FileOp;

namespace TSmatch.Message
{
    public class Message
    {
        public static readonly ILog log = LogManager.GetLogger("Message");
        protected bool in_UT;   //true - started from Unit Test

        /// <summary>
        /// _messages - set of <Message.Key, Message.Value> strings in local culture
        /// </summary>
        protected Dictionary<string, string> _messages = new Dictionary<string, string>();
        internal initMessageDic v = new initMessageDic();
        public enum Severity { INFO = 0, WARNING, FATAL, STRING };

        protected string msg, errType;

        public Message() { } // if(__messages.Count == 0) SetLanguage(Properties.Settings.Default.sLanguage); }

        public void SetLanguage(string sLang)
        {
            _messages = v.get_mesDic(sLang) as Dictionary<string, string>;
        }

        protected void txt(Severity type, string msgcode, object[] p, bool doMsgBox = true)
        {
            msgcode = msgcode.Replace(' ', '_');
            errType = "TSmatch " + type;
            bool knownMsg = _messages.ContainsKey(msgcode);

            try { msg = knownMsg ? string.Format(_messages[msgcode], p) : string.Format(msgcode, p); }
            catch { msg = knownMsg ? _messages[msgcode] : msgcode; errType = "(!)" + errType; }
            if (!knownMsg) errType = "(*)" + errType;
            string str= "F";
            switch (type)
            {
                case Severity.STRING: return;
                case Severity.FATAL: break;
                case Severity.WARNING: str = "W"; break;
                case Severity.INFO: str = "I"; break;
            }
            if (in_UT) throw new ArgumentException("Msg." + str + ": " + msg);
            // PKh> To have a look to the MessageBox with the message, add comment to the next line, which check doMsgBox
            if (doMsgBox)
                if (str == "I")
                    MessageBox.Show(msg, errType, MessageBoxButton.OK, MessageBoxImage.Asterisk, reply, MessageBoxOptions.ServiceNotification);
            if (type == Severity.FATAL) Stop();
        }

        public void F(string str, params object[] p) { txt(Severity.FATAL, str, p); }
        public void W(string str, params object[] p) { txt(Severity.WARNING, str, p); }
        public void I(string str, params object[] p) { txt(Severity.INFO, str, p); }
        public string S(string str, params object[] p)
        {
            txt(Severity.STRING, str, p);
            if (errType.Contains("(")) msg = errType + " " + msg;
            return msg;
        }

        public bool AskYN(string msgcode, params object[] p)
        {
            txt(Severity.INFO, msgcode, p, doMsgBox: false);
            var X = MessageBox.Show(msg, errType, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return X == MessageBoxResult.Yes;
        }

        private MessageBoxResult reply;

        public void AskOK(string msgcode, params object[] p)
        {
            txt(Severity.INFO, msgcode, p, doMsgBox: false);
            reply = MessageBox.Show(msg, errType, MessageBoxButton.OKCancel, MessageBoxImage.Question);
        }

        public void AskFOK(string msgcode, params object[] p)
        {
            AskOK(msgcode, p);
            if (reply == MessageBoxResult.OK) return;
            Stop();
        }

        public static void Stop()
        {
            FileOp.AppQuit();
            Environment.Exit(0);
        }
    } // end class

    /// <summary>
    /// initMessageDic setup Language as pointed sLange Culture Name string,
    /// store it in PropertySetting, and fill message Dictionary from TSmatchMsg.resx 
    /// </summary>
    internal class initMessageDic
    {
        private Dictionary<string, string> mDic = new Dictionary<string, string>();

        public initMessageDic() { }

        internal object get_mesDic(string sLang)
        {
            if (string.IsNullOrWhiteSpace(sLang))
            {
                sLang = CultureInfo.CurrentCulture.Name;
                Properties.Settings.Default.sLanguage = sLang;
                Properties.Settings.Default.Save();
            }
            if (mDic.Count != 0) mDic.Clear();
            ResourceManager mgr = Properties.TSmatchMsg.ResourceManager;
            CultureInfo culture = CultureInfo.GetCultureInfo(sLang);
            ResourceSet set = mgr.GetResourceSet(culture, true, true);
            foreach (System.Collections.DictionaryEntry o in set)
            {
                mDic.Add(o.Key as string, o.Value as string);
            }
            mgr.ReleaseAllResources();
            return mDic;
        }
    } //end cass initMessageDic
} // end namespace