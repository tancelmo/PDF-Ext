using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using PDF_Ext.Helpers;

namespace PDF_Ext;
internal class StartUp
{
    public static void Language(WindowEx window)
    {
        ConfigFile configFile = new ConfigFile(Globals.ConfigFilePath);
        var language = configFile.Read("Language", "General");
        if (language == "")
        {
            language = Thread.CurrentThread.CurrentCulture.ToString();
            configFile.Write("Language", language, "General");
        }
        CultureInfo ci = new CultureInfo(language);
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = language;
    }

    public static void CreateFilesIfNotExist()
    {
        if (!Directory.Exists(Globals.ConfigPath))
        {
            Directory.CreateDirectory(Globals.ConfigPath);


        }

        if (!File.Exists(Globals.ConfigFilePath))
        {

            CreateConfigFile();
            Debug.WriteLine(Globals.ConfigFilePath + "######################");
        }
    }

    internal static void CreateConfigFile()
    {
        using (StreamWriter writer = new StreamWriter(Globals.ConfigFilePath, append: true))
        {
            writer.Write(
@"[General]
Language=
Theme=");
        }
    }
}
