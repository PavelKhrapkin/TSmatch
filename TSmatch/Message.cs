/*----------------------------------------------------------------------------
 * Message -- multilanguage message system
 * 
 * 3.10.2017  Pavel Khrapkin
 *
 *--- Unit Tests ---
 * UT_Message: UT_Init, UT_AskS, UT_W, UT_S with _UT_Service 3.10.2017 OK
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

        public enum Severity { INFO = 0, WARNING, FATAL, SPLASH };
        public static bool Trace = false;  // For TRACE mode chage to "true";
        public static bool Dialog = true;  //for Unit Test set Dialog = false;

        public Message() { }

        /// <summary>
        /// _messages - set of key- Message code string and message value- string in local culture
        /// </summary>
        protected static Dictionary<string, string> _messages = new Dictionary<string, string>();

        protected static ResourceManager mgr = Properties.TSmatchMsg.ResourceManager;

        /// <summary>
        /// singleton Message system initialization -- ToDo 31.8.17 make it with rsx Localization
        /// </summary>
        static Message()
        {
            ResourceSet set = mgr.GetResourceSet(CultureInfo.CurrentCulture, true, true);
            foreach (System.Collections.DictionaryEntry o in set)
            {
                _messages.Add(o.Key as string, o.Value as string);
            }
            mgr.ReleaseAllResources();
        }
  
        protected static string msg, errType;
        static void txt(Severity type, string msgcode, object[] p, bool doMsgBox=true)
        {
            msgcode = msgcode.Replace(' ', '_');
            errType = "TSmatch " + type;
            bool knownMsg = _messages.ContainsKey(msgcode);
            try { msg = knownMsg ? string.Format(_messages[msgcode], p) : string.Format(msgcode, p); }
            catch { msg = knownMsg? _messages[msgcode]: msgcode; errType = "(!)" + errType; }
            if (!knownMsg) errType = "(*)" + errType;
            if (!Dialog) throw new ArgumentException("Msg.F");
            if (doMsgBox) MessageBox.Show(msg, errType, MessageBoxButton.OK, MessageBoxImage.Asterisk, reply, MessageBoxOptions.ServiceNotification);
            if (type == Severity.FATAL) Stop();
        }

        public static void txt(string str, params object[] p) { txt(Severity.INFO, str, p, doMsgBox: false); }
        public static void F(string str, params object[] p)   { txt(Severity.FATAL, str, p); }
        public static void W(string str, params object[] p)   { txt(Severity.WARNING, str, p); }
        public static void I(string str, params object[] p)   { txt(Severity.INFO, str, p); }
        public static string S(string str, params object[] p)
        {
            txt(Severity.SPLASH, str, p, doMsgBox: false);
            if (errType.Contains("(")) msg = errType + " " + msg;
            return msg;
        }

        public static bool AskYN(string msgcode, params object[] p)
        {
            txt(Severity.INFO, msgcode, p, doMsgBox: false);
            var X = MessageBox.Show(msg, errType, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return X == MessageBoxResult.Yes;
        }

        private static MessageBoxResult reply;

        public static void AskOK(string msgcode, params object[] p)
        {
            txt(Severity.INFO, msgcode, p, doMsgBox: false);
            reply = MessageBox.Show(msg, errType, MessageBoxButton.OKCancel, MessageBoxImage.Question);
        }

        public static void AskFOK(string msgcode, params object[] p)
        {
            AskOK(msgcode, p);
            if (reply == MessageBoxResult.OK) return;
            Stop();
        }

        public static string AskS(string msgcode, params object[] p)
        {   // NotImplemetedYet !!
            txt(Severity.INFO, msgcode, p, doMsgBox: false);
 //           string str = string.Empty;
   //         str = MessageBox.Show(msg);
            return null;
        }
        public static void Stop()
        {
            FileOp.AppQuit();
            Environment.Exit(0);
        }
    } // end class
} // end namespace