using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

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
            bool ok = true;
            if (!TS.isTeklaActive()) Log.FATAL("ERR_TEKLA_INACTIVE");
            TOCdir = TS.GetTeklaDir();
            MyPath = System.IO.Directory.GetCurrentDirectory();
            if(MyPath != TOCdir)
            {                   // Copy MyPath\TSmatch.exe TOCdir   (!) тут можно попытаться скопировать из TSmatch.ZIP если он есть в MyPath
                ok = FileOp.CopyFile(MyPath, Decl.TSMATCH_EXE, TOCdir, overwrite: true);
            }
            if (!FileOp.isFileExist(TOCdir, Decl.F_MATCH))
            {                   // Copy MyPath\TSmatch.xlsx TOCdir  (!) тут можно попытаться скопировать из TSmatch.ZIP если он есть в MyPath
                ok = FileOp.CopyFile(MyPath, Decl.F_MATCH, TOCdir, overwrite: true);
            }
            // make macroDir from TOCdir with Decl.BUTTON_DIR
            macroDir = TOCdir.Replace("exceldesign", Decl.BUTTON_DIR);
            Renew(macroDir, Decl.BUTTON_CS);    // Copy MyPath\Decl.BUTTON_CS macroDir
            Renew(macroDir, Decl.BUTTON_BMP);   // Copy MyPath\Decl.BUTTON_BMP macroDir
            Log.exit();
        }
        static bool Renew(string dir, string name)
        {
            bool ok = true;
            if (!FileOp.isFileExist(dir, name) && FileOp.isFileExist(MyPath, name))
                ok = FileOp.CopyFile(MyPath, name, dir);
            if (!ok && FileOp.isFileExist(MyPath, Decl.TSMATCH_ZIP))
            {
                ////using (ZipArchive zip = OpenRead("TSmatch.zip"))
                ////{ }
                ////    zip = ZipArchive.   .Open()
            }
            return ok;
        }
        static bool CopyOrExtract(bool update, string dir, string name)
        {
            bool ok = false;
            if (!update && FileOp.isFileExist(dir, name)) return true;
            bool isZIP = FileOp.isFileExist(MyPath, Decl.TSMATCH_ZIP);
            if (update && !FileOp.isFileExist(MyPath, name) && !isZIP) Msg.F("ERR Bootstrap");
            if (!update) ok = Renew(macroDir, name);
            else
                if (FileOp.isFileExist(MyPath, name)) ok = FileOp.CopyFile(MyPath, name, dir, overwrite: true);
            else { }
            return ok;
        }
        ////static void ZZ()
        ////{
        ////    string zipPath = @"c:\example\start.zip";
        ////    string extractPath = @"c:\example\extract";

        ////    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        ////    {
        ////        foreach (ZipArchiveEntry entry in archive.Entries)
        ////        {
        ////            if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        ////            {
        ////                entry.ExtractToFile(Path.Combine(extractPath, entry.FullName));
        ////            }
        ////        }
        ////    }
        ////}

    } // end class Bootsrap
} // end namespace
