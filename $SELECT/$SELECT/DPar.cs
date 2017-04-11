using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

using Lib = match.Lib.MatchLib;
using Sec = TSmatch.Section.Section;
using SType = TSmatch.Section.Section.SType;

namespace TSmatch.DPar
{
    public class DPar 
    {
        public static readonly ILog log = LogManager.GetLogger("DPar");

        public Dictionary<SType, string> dpar = new Dictionary<SType, string>();
        public Dictionary<SType, string> dpStr = new Dictionary<SType, string>();

        public DPar(string str)
        {
            string[] sections = str.Split(';');
            foreach(string sec in sections)
            {
                Sec txSec = new Sec(sec);
                if (txSec.type == SType.NOT_DEFINED) continue;
                dpar.Add(txSec.type, txSec.body);           // with ToLat.Tolower
                int indx = sec.IndexOf(':') + 1;
                dpStr.Add(txSec.type, sec.Substring(indx)); // initial body
            }
        }

        public void Ad(SType stype, string str)
        {
            dpStr.Add(stype, str);
            str = Lib.ToLat(str).ToLower().Replace(" ", "");
            dpar.Add(stype, str);
        }

        // if field of stype not recognized, return -1
        //.. f.e. it happaned with constant like "M:C245"
        public int Col(SType stype)
        {
            if (stype.ToString().Contains("UNIT_")) return -1;
            string str = string.Empty;
            try { str = dpar[stype]; }
            catch { }
            return Lib.ToInt(str); 
        }
    }
} // end namespace DPar
