using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Ext;
internal class Globals
{
    public static string ConfigFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Settings\\Config.ini";

    public static string ConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Settings";
}
