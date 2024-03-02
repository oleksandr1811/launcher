using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace Launcher;

public class allConfig
{
    public Dictionary<string, ServerConfig> ServerConfigs { get; set; }

    public Dictionary<string, NewsConfig> NewConfigs { get; set; }

    public Dictionary<string, SettingsConfig> SettingsConfigs { get; set; }

    public void SerializeLauncher()
    {
        var readKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SAMP");
        var gta = (string)readKey.GetValue("gta_sa_exe");
        var name = (string)readKey.GetValue("PlayerName");
        readKey.Close();
        if (!File.Exists("C:\\Launcher\\launcher.json"))
            using (var fs = new FileStream("C:\\Launcher\\launcher.json", FileMode.Create,
                       FileAccess.ReadWrite,
                       FileShare.ReadWrite))
            {
                var a = new allConfig
                {
                    SettingsConfigs = new Dictionary<string, SettingsConfig>
                    {
                        ["LauncherSettings"] = new()
                        {
                            LauncherId = 1,
                            ArizonaLauncher = false,
                            AutoUpdate = true,
                            CloseLauncher = false,
                            FirstStart = true,
                            GtaInstalled = false,
                            GtaPath = gta,
                            NickName = name,
                            StartOnMyGta = true
                        }
                    }
                };
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                JsonSerializer.Serialize(fs, a, options);
                fs.Dispose();
            }
    }

    public string GetContentOrWebSite(string url)
    {
        var request =
            (HttpWebRequest)WebRequest.Create(url);
        var response = (HttpWebResponse)request.GetResponse();
        var reader = new StreamReader(response.GetResponseStream());
        var output = new StringBuilder();
        output.Append(reader.ReadToEnd());
        response.Close();
        return output.ToString();
    }
}