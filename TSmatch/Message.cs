/*----------------------------------------------------------------------------
 * Message -- multilanguage message system
 * 
 * 16.05.2017  Pavel Khrapkin
 *
 *--- History ---
 * Feb-2016 Created
 * 20.3.2016 - Error message Code display even when Message system is not initialysed yet
 * 20.8.2016 - use log4net, bug fixes
 *  9.5.2017 - MessageBox.Show use
 * 11.5.2017 - Fatal error handling with Application.Current.Sutdown, AskFOK, and SPLAS messages
 * 16.5.2017 - AskYN
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using log4net;

using FileOp = match.FileOp.FileOp;
using Docs = TSmatch.Document.Document;
using Decl = TSmatch.Declaration.Declaration;
using Log = match.Lib.Log;

namespace TSmatch.Message
{
    public class Message
    {
        public static readonly ILog log = LogManager.GetLogger("Message");
        public enum Severity { INFO = 0, WARNING, FATAL, SPLASH };

        public static bool Trace = false;  // For TRACE mode chage to "true";

        readonly string MessageID;
        readonly string text;

        internal static List<Message> Messages = new List<Message>();

        Message(string _MessageID, string _text)
        {
            MessageID = _MessageID;
            text = _text;
        }
        /// <summary>
        /// Start() - Multilanguage Message system initialize from TSmatch.xlsx/Messages
        /// </summary>
        /// <history> 7.3.2016 P.Khrapkin
        /// 12.3.16 - bootstrap error handling</history>
        public static void Start()
        {
            int iLanguage = 3;   //iLanguage =2 - ru-Ru; iLanguage = 3 - en-US
            if (getLanguage() == Decl.RUSSIAN) iLanguage = 2;

            Docs doc = Docs.getDoc(Decl.MESSAGES);
            for (int i = doc.i0; i <= doc.il; i++)
            {
                string id = doc.Body.Strng(i, 1);
                if (string.IsNullOrWhiteSpace(id)) continue;    // ignore messages without ID
                string txt = doc.Body.Strng(i, iLanguage);
                while (i < doc.il)
                {                                               // multiline messages
                    if (!string.IsNullOrWhiteSpace(doc.Body.Strng(i + 1, 1).ToString())) break;
                    string line = doc.Body.Strng(++i, iLanguage);
                    if (string.IsNullOrWhiteSpace(line)) continue;  //go toll empty line
                    txt += "\n" + line;
                }
                foreach (var v in Messages)                     // healthchech for unique MessageID
                    if (id == v.MessageID) F("ERR_02_START_FAULT", id);
                Messages.Add(new Message(id, txt));
            }
        } // end Start()
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

        public static void txt(Severity type, string msgcode, params object[] p)
        {
            Message msg = Messages.Find(x => x.MessageID == msgcode);
            if (msg == null)
            {
                MessageBox.Show(msgcode, "(*) TSmatch " + type);
                Log.FATAL(msgcode);
            }
            else
            {
                string str = string.Format(msg.text, p);
                MessageBox.Show(str, "TSmatch " + type);
                Log.FATAL(str);
            }
        }
#if OLD
        public static void txt(Severity type, string msgcode, object p0 = null, object p1 = null, object p2 = null)
        {
            Message msg = Messages.Find(x => x.MessageID == msgcode);
            if (msg == null)
            {
                MessageBox.Show(msgcode, "(*) TSmatch " + type);
            }
            else
            {
                string str = string.Format(msg.text, p0, p1, p2);
                MessageBox.Show(str, "TSmatch " + type);
            }
        }
#endif 
        public static void txt(string str, params object[] p) { txt(Severity.FATAL, str, p); }
        public static void F(string str, params object[] p) { txt(Severity.FATAL, str, p); Stop(); }
        public static void W(string str, params object[] p) { txt(Severity.WARNING, str, p); }
        public static void I(string str, params object[] p) { txt(Severity.INFO, str, p); }
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
            var X = MessageBoxResult.Yes;
            string str = msgcode;
            Message msg = Messages.Find(x => x.MessageID == msgcode);
            if (msg == null)
            {
                str = string.Format(msgcode, p);
                X = MessageBox.Show(str, "(*) TSmatch", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            else
            {
                str = string.Format(msg.text, p);
                X = MessageBox.Show(str, "TSmatch", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            return X == MessageBoxResult.Yes;
        }

        public static void AskFOK(string msgcode, params object[] p)
        {
            var ok = MessageBoxResult.OK;
            string str = msgcode;
            Message msg = Messages.Find(x => x.MessageID == msgcode);
            if (msg == null)
            {
                str = string.Format(msgcode, p);
                ok = MessageBox.Show(str, "(*) TSmatch", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            }
            else
            {
                str = string.Format(msg.text, p);
                ok = MessageBox.Show(str, "TSmatch", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            }
            if (ok == MessageBoxResult.OK) return;
            Stop();
        }
        public static void Stop()
        {
            FileOp.AppQuit();
            Environment.Exit(0);
        }
    } // end class
} // end namespace
