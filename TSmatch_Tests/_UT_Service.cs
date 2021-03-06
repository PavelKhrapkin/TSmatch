﻿/*=======================================================
* _UT_Service - common service module for all Unit Tests
* 
* 29.11.2017 Pavel Khrapkin
*========================================================
* History:
* 2017.10.6  - delegate TryMsg
* 2017.11.12 - восстанавливаю по частям после инцедента 10.11.17
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UT_TSmatch
{
    public class _UT_MsgService : TSmatch.Message.Message
    {
        public _UT_MsgService(bool inUT = true) { in_UT = inUT; }

        public int cnt_msgs() { return _messages.Count; }

        internal void _W(string str, params object[] p)
        {
            try { W(str, p); }
            catch (ArgumentException e)
            {
                if (e.Message.Substring(0, 7) != "Msg.W: ") Assert.Fail();
            }
        }
        internal void _I(string str, params object[] p)
        {
            try { I(str, p); }
            catch (ArgumentException e)
            {
                if (e.Message.Substring(0, 7) != "Msg.I: ") Assert.Fail();
            }
        }
        internal void _F(string str, params object[] p)
        {
            try { F(str, p); }
            catch (ArgumentException e)
            {
                if (e.Message.Substring(0, 7) != "Msg.F: ") Assert.Fail();
            }
        }

        public string GetMsg() { return msg; }
        public string GetErrType() { return errType; }

        public string Try2(Func<int, string> func, int p1, string p2)
        {
            ////try { func(p1, p2); }
            ////catch { }
            ////GetTxt();
            return msg;
        }
    }
}