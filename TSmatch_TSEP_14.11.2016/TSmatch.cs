///
/// This is a template of a macro that calls an external application
///----------------------
/// this cs file should be placed in Directory "ApplicationsFolder"
/// C:\ProgramData\Tekla Structures\21.1\Environments\common\macros\modeling
/// Here also place picture as BMP file, which appeared on Tekla screen as an Icon
/// Click this button start invoke application "ApplicationName"

using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            // Application name should match the full name of your executable.
            // Note: if you not intend to use the unicode symbol "@" then you need to use the escape character "\" for reserved characters  
            string ApplicationName = @"TSmatch.exe";
//            string ApplicationsFolder = @"C:\Users\Pavel_Khrapkin\Desktop\TSmatch\C#\Dec-2015\2015.12.12 PK_ProcEngine\ConsoleApplication1\bin\Debug";
			string ApplicationsFolder = @"C:\ProgramData\Tekla Structures\21.1\Environments\common\exceldesign";
            string ApplicationFile = Path.Combine(ApplicationsFolder, 
                ApplicationName);

            if (File.Exists(ApplicationFile))
            {
                Process.Start(ApplicationFile);
            }
            else
            {
                MessageBox.Show("Application file doesn't exist.");
            }
        }
    }
}
