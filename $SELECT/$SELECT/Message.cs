/*----------------------------------------------------------------------------
 * Message -- multilanguage message system
 * 
 * 20.8.2016  Pavel Khrapkin
 *
 *--- History ---
 * Feb-2016 Created
 * 20.3.2016 - Error message Code display even when Message system is not initialysed yet
 * 20.8.2016 - use log4net, bug fixes
 * ---------------------------------------------------------------------------------------
 *      Methods:
 * Start()    - Copy messages into the static list from TSmatch.xlsx/Messages Sheet
 * F(Code,..) - Fatal error message output
 * W(Code,..) - Warning message output
 * I(Code,..) - Information Message
 */
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using log4net;

using Docs = TSmatch.Document.Document;
using Decl = TSmatch.Declaration.Declaration;
using Log = match.Lib.Log;

namespace TSmatch.Message
{
    public class Message
    {
        public static readonly ILog log = LogManager.GetLogger("Message");
        public enum Severity { INFO = 0, WARNING, FATAL};

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
            if ( getLanguage() == Decl.RUSSIAN) iLanguage = 2;

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
        public static void txt(Severity type, string msgcode, object p0=null, object p1=null, object p2=null)
        {
            Message msg = Messages.Find(x => x.MessageID == msgcode);
            if (msg == null)
            {
                log.Fatal("TSmatch internal Message system error: Message \n\t" + msgcode
                    + "\nnot foung in collection Messages." 
                    + " Possibly Message system not initiated yet, or this message not listed in TSmatch.xlsx/Message");
                Log.FATAL("ERR_03 - Message Code Not Found");
            }
            string str = string.Format(msg.text, p0, p1, p2);
            mes(str, (int)type);

            //////bool ok = false;
            //////foreach (var v in Messages)
            //////{
            //////    if (v.MessageID == msgcode)
            //////    {
            //////        string s = String.Format(v.text, p0, p1, p2);
            //////        mes(s, (int)type);
            //////        ok = true;
            //////        break;
            //////    }
            ////////////////////}
            ////////////////////if (!ok)
            ////////////////////    if (Messages.Count > 0) F("ERR_03_NOMSG", msgcode);
            ////////////////////    else Log.FATAL("Internal ERROR: Message system is not initiated");
        }
        public static void txt(string str, object p0= null, object p1 = null, object p2 = null)
        { txt(Severity.FATAL, str, p0, p1, p2); }
        public static void F(string str, object p0=null, object p1=null, object p2=null)
        { txt(Severity.FATAL, str, p0, p1, p2); }
        public static void W(string str, object p0 = null, object p1 = null, object p2 = null)
        { txt(Severity.WARNING, str, p0, p1, p2); }
        public static void I(string str, object p0 = null, object p1 = null, object p2 = null)
        { txt(Severity.INFO, str, p0, p1, p2); }
    } // end class
} // end namespace
