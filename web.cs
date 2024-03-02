using Launcher.CustomControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace Launcher;

public class Web
{
    /*var options = new JsonSerializerOptions
    {
        WriteIndented = true
    };
    var oldJson1 = new allConfig();
    using (var fs1 = new FileStream("C:\\Launcher\\servers.json", FileMode.Open, FileAccess.Read,
               FileShare.ReadWrite))
    {
        oldJson1 = JsonSerializer.Deserialize<allConfig>(fs1, options);
    }
    var cfg = oldJson1.ServerConfigs;
    foreach (var variable in new Form1().servers().Servers)
        if (oldJson1.ServerConfigs.ContainsKey(variable) == false)
            cfg.Add(variable, new ServerConfig
            {
                ServerIP = ServerConfig(variable).ServerIP,
                ServerPort = ServerConfig(variable).ServerPort,
                Selected = false,
                ServerName = variable,
                imgUrl = ServerConfig(variable).imgUrl
            });

    var a = new allConfig
    {
        ServerConfigs = cfg
    };
    using (var fs = new FileStream("C:\\Launcher\\servers.json", FileMode.Create, FileAccess.Write,
               FileShare.ReadWrite))
    {
        JsonSerializer.Serialize(fs, a, options);
    }*/


    public ServerConfig ServerConfig(string name)
    {
        var a = JsonSerializer.Deserialize<ServerConfig>(
            new allConfig().GetContentOrWebSite(
                $"https://fooza.ru/Launcher-Web/include/api/getServerInfo.php?id={name}"));
        return a;
    }
}