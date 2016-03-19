using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TS = TSmatch.Tekla.Tekla;
using Log = match.Lib.Log;
using Msg = TSmatch.Message.Message;
using FileOp = match.FileOp.FileOp;
using Decl = TSmatch.Declaration.Declaration;

namespace TSmatch.Startup
{
    public static class Bootstrap
    {
        static string MyPath;
        static string TOCdir;
        static string macroDir;

        public static void Bootsrap()
        {
            Log.set("Bootstrap");
            if (!TS.isTeklaActive()) Log.FATAL("ERR_TEKLA_INACTIVE");
            TOCdir = TS.GetTeklaDir();
            MyPath = System.IO.Directory.GetCurrentDirectory();
            if(MyPath != TOCdir)
            {
                if (FileOp.CopyFile(MyPath, Decl.F_MATCH, TOCdir)) Msg.F("Err");
                // Copy MyPath\TSmatch.exe TOCdir
            }
            if (!FileOp.isFileExist(TOCdir, Decl.F_MATCH))
            {
                // Copy MyPath\TSmatch.xlsx TOCdir
            }
            // make macroDir from TOCdir with Decl.BUTTON_DIR
            //            if (!FileOp.isFileExist(MyPath, Decl.BUTTON.CS) && 
            //                 FileOp.isFileExist(MyPath, Decl.BUTTON.BMP)
            {
                // Copy MyPath\Decl.BUTTON_CS macroDir
                // Copy MyPath\Decl.BUTTON_BMP macroDir
            }
            Log.exit();
        }
    } // end class Bootsrap
} // end namespace
