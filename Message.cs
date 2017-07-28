/*----------------------------------------------------------------------------
 * Message -- multilanguage message system
 * 
 * 18.07.2017  Pavel Khrapkin
 *
 *--- History ---
 * Feb-2016 Created
 * 20.3.2016 - Error message Code display even when Message system is not initialysed yet
 * 20.8.2016 - use log4net, bug fixes
 *  9.5.2017 - MessageBox.Show use
 * 11.5.2017 - Fatal error handling with Application.Current.Sutdown, AskFOK, and SPLAS messages
 * 22.5.2017 - AskYN, Msg.OK()
 * 18.7.2017 - remake with Dictionary as a Messages store
 * ---------------------------------------------------------------------------------------
 *      Methods:
 * Start()    - Copy messages into the static list from TSmatch.xlsx/Messages Sheet
 * F(Code,..) - Fatal error message output
 * W(Code,..) - Warning message output
 * I(Code,..) - Information Messag
 * AskFOK(text?) - ask text OK-Cancel, with Fatal/Stop at Cancel 
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

        public static Dictionary<string, string> msgs = new Dictionary<string, string>();
     
        //        static Message() singleton Meggage system initialization -- now manualy
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
                try { msgs.Add(keyMsg, mes.Trim()); }
                catch { F("Messages.Init fault", i-1, keyMsg, mes);  }
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
        public static void mes(string str, int severity = 0)
        {
            if (severity == (int)Severity.FATAL) Log.FATAL(str);
            if (severity == (int)Severity.WARNING
                || severity == (int)Severity.INFO) new Log(str);
            return;
        }

        public static string msg, errType;
        static void txt(Severity type, string msgcode, object[] p, bool doMsgBox=true)
        {
            bool knownMsg = msgs.ContainsKey(msgcode);
            msg = knownMsg? string.Format(msgs[msgcode], p): string.Format(msgcode, p);
            errType = "TSmatch " + type;
            if (!knownMsg) errType = "(*)" + errType;
            if(doMsgBox) MessageBox.Show(msg, errType);
            if (type == Severity.FATAL) Stop();
        }

        public static void txt(string str, params object[] p) { txt(Severity.INFO, str, p, doMsgBox: false); }
        public static void F(string str, params object[] p)   { txt(Severity.FATAL, str, p); }
        public static void W(string str, params object[] p)   { txt(Severity.WARNING, str, p); }
        public static void I(string str, params object[] p)   { txt(Severity.INFO, str, p); }

#if NotWorkingYet
        public static void S(string str, object p0 = null, object p1 = null, object p2 = null)
        {
            txt(Severity.SPLASH, str, p0, p1, p2);
            SplashScreen splashScreen = new SplashScreen("SplashScreenImage.bmp");
            splashScreen.Show(true);
        }
#endif // Splash еще не умею...
        public static bool AskYN(string msgcode, params object[] p)   //16/5 0 = null, object p1 = null, object p2 = null)
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

        public static void Stop()
        {
            FileOp.AppQuit();
            Environment.Exit(0);
        }
    } // end class
} // end namespace