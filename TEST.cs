using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

using Lib = match.Lib.MatchLib;
using Msg = TSmatch.Message.Message;
using FP = TSmatch.FingerPrint.FingerPrint;
using Docs = TSmatch.Document.Document;

namespace TSmatch.Test
{
    [TestClass]
    class Test
    {
//28/02        [TestMethod]
        public static readonly ILog log = LogManager.GetLogger("TEST");

        public static void TSmatchTEST()
        {
            log.Info("\n\n-------------- TSmatch TEST track v2017.03.7 --------");
            // 24/1 //            var bootstrap = new Bootstrap.Bootstrap();
            Section.Section.testSection();
            log.Info("Test Section\t\t\tOK");

            Parameter.Parameter.testParameter();
            log.Info("Test Parameter\t\tOK");

            FP.testFP();
            log.Info("Test FingerPrint\t\tOK");

            Component.Component.testComponent();
            log.Info("Test Components\t\tOK");

            Matcher.Mtch.testMtch();
            log.Info("Test Matcher\t\t\tOK");

        }
    }

    public class assert
    {
        public static void Eq(dynamic check, dynamic val)
        {
            if (val.GetType() == typeof(int) && check.GetType() == typeof(int))
            {
                int c = (int)check, v = (int)val;
                if (c != v) Msg.F("TEST.assert int FALSE", c, v);
                return;
            } 
            if(val.GetType() == typeof(string))
            {
                string c = string.Empty;
                if (check.GetType() != typeof(string)) c = (string)check.ToString();
                else c = (string)check;
                string v = (string)val;
                if (c != v) Msg.F("TEST.assert string FALSE", c, v);
                return;
            }
            if (val.GetType() == typeof(double))
            {
                double c = 0.0;
                if (check.GetType() != typeof(double)) c = (double)check;
                else c = check;
                double v = (double)val;
                if (c != v) Msg.F("TEST.assert double FALSE", c, v);
                return;
            }
            if (val.GetType() == typeof(bool))
            {
                bool c = false;
                if (check.GetType() != typeof(bool)) c = (bool)check;
                else c = check;
                bool v = (bool)val;
                if (c != v) Msg.F("TEST.assert bool FALSE", c, v);
                return;
            }
            Msg.F("TEST.assert UNKNOWN type - non-Integer or non-String");
        }
    }
}
