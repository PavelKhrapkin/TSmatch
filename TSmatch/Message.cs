/*----------------------------------------------------------------------------
 * Message -- multilanguage message system
 * 
 * 31.08.2017  Pavel Khrapkin
 *
 *--- History ---
 * Feb-2016 Created
 * 20.3.2016 - Error message Code display even when Message system is not initialysed yet
 * 20.8.2016 - use log4net, bug fixes
 *  9.5.2017 - MessageBox.Show use
 * 11.5.2017 - Fatal error handling with Application.Current.Sutdown, AskFOK, and SPLAS messages
 * 22.5.2017 - AskYN, Msg.OK()
 * 18.7.2017 - remake with Dictionary as a Messages store
 * 31.8.2017 - Msg.S return string 
 * ---------------------------------------------------------------------------------------
 *      Methods:
 * Init()     - Singleton constructor initiate static msgs Dictionary set
 * F(Code,..) - Fatal error message output
 * W(Code,..) - Warning message output
 * I(Code,..) - Information Messag
 * S(Code,..) - return string made from Code and ".." arguments to output in WPF
 * AskFOK(text?) - ask text OK-Cancel, with Fatal/Stop at Cancel
 * AskYN(text?)  - ask with text as a prompt, and respont Yes or No
 * AskS(text:)   - request string entry with text as a prompt
 */
using System;
using System.Collections.Generic;
using System.Windows;
using System.Globalization;
using log4net;

using FileOp = match.FileOp.FileOp;
using Docs = TSmatch.Document.Document;
using Decl = TSmatch.Declaration.Declaration;
using Lib = match.Lib.MatchLib;
using Log = match.Lib.Log;

namespace TSmatch.Message
{
    public class Message
    {
        public static readonly ILog log = LogManager.GetLogger("Message");

        public enum Severity { INFO = 0, WARNING, FATAL, SPLASH };
        public static bool Trace = false;  // For TRACE mode chage to "true";

        public Message() { }

        /// <summary>
        /// _messages - set of key- Message code string and message value- string in local culture
        /// </summary>
        protected static Dictionary<string, string> _messages = new Dictionary<string, string>();

        /// <summary>
        /// singleton Message system initialization -- ToDo 31.8.17 make it with rsx Localization
        /// </summary>
        public static void Init()
        {
            int iLanguage = 3;   //iLanguage =2 - ru-Ru; iLanguage = 3 - en-US
            if (getLanguage() == Decl.RUSSIAN) iLanguage = 2;

            Docs doc = Docs.getDoc(Decl.MESSAGES);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                string keyMsg = doc.Body.Strng(i, 1);
                if (string.IsNullOrWhiteSpace(keyMsg)) continue;
                string mes = doc.Body.Strng(i, iLanguage);
                bool emptyNextLine;
                do
                {
                    string nextLine = doc.Body.Strng(++i, iLanguage);
                    emptyNextLine = string.IsNullOrWhiteSpace(nextLine);
                    mes += "\n\r" + nextLine;
                } while (!emptyNextLine);
                try { _messages.Add(keyMsg, mes.Trim()); }
                catch { F("Messages.Init fault", i - 1, keyMsg, mes); }
            }
        }

        /// <summary>
        /// getLanguage() - return Windows system language
        /// </summary>
        /// <returns></returns>
        public static string getLanguage()
        {
            CultureInfo ci = CultureInfo.InstalledUICulture;
            return ci.CompareInfo.Name;
        }
  
        public static string msg, errType;
        static void txt(Severity type, string msgcode, object[] p, bool doMsgBox=true)
        {
            bool knownMsg = _messages.ContainsKey(msgcode);
            msg = knownMsg? string.Format(_messages[msgcode], p): string.Format(msgcode, p);
            errType = "TSmatch " + type;
            if (!knownMsg) errType = "(*)" + errType;
            if(doMsgBox) MessageBox.Show(msg, errType);
            if (type == Severity.FATAL) Stop();
        }

        public static void txt(string str, params object[] p) { txt(Severity.INFO, str, p, doMsgBox: false); }
        public static void F(string str, params object[] p)   { txt(Severity.FATAL, str, p); }
        public static void W(string str, params object[] p)   { txt(Severity.WARNING, str, p); }
        public static void I(string str, params object[] p)   { txt(Severity.INFO, str, p); }
        public static string S(string str, params object[] p)
        {
            txt(Severity.SPLASH, str, p, doMsgBox: false);
            if (errType.Contains("(*)")) msg = errType + msg;
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
        {
            txt(Severity.INFO, msgcode, p, doMsgBox: false);
 //           string 
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