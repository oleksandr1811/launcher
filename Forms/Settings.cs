using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace Launcher;

public partial class Settings : Form
{
    public Settings()
    {
        Text = "Настройки";
        StartPosition = FormStartPosition.CenterScreen;
        /* if (!File.Exists("C:\\Launcher\\settingsConfig.json"))
         {
             var config = new SettingsConfig();
             config.Inizialize();
         }*/
        ShowInTaskbar = false;
        InitializeComponent();
    }

    private void Settings_Load(object sender, EventArgs e)
    {
        using (var fs = new FileStream("C:\\Launcher\\launcher.json", FileMode.OpenOrCreate))
        {
            string fileContents;
            using (var reader = new StreamReader(fs))
            {
                fileContents = reader.ReadToEnd();
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var obj = JsonSerializer.Deserialize<allConfig>(fileContents, options);
            SettingsConfig settings;
            obj.SettingsConfigs.TryGetValue("LauncherSettings", out settings);
            var a = new allConfig().GetContentOrWebSite($"https://fooza.ru/Launcher-Web/include/api/getLauncherInfo.php?launcherid={settings.LauncherId}");
            if (a == "Error")
            {
                Guna2MessageDialog dialog = new Guna2MessageDialog
                {
                    Buttons = MessageDialogButtons.OK,
                    Caption = "Загрузка настроек",
                    Icon = MessageDialogIcon.Error,
                    Parent = this,
                    Style = MessageDialogStyle.Dark,
                    Text = $"При попытке загрузить настройки лаунчера произошла ошибка!\n" +
                           $"Загружены стандартные значения."
                };
                dialog.Show();
            }
            else
            {
                var json = JsonSerializer.Deserialize<configs.Launcher>(a);
                guna2CirclePictureBox2.ImageLocation = json.logoImg;
            }
            if (settings.FirstStart)
            {
                label9.Text = "НАСТРОЙКИ [ПЕРВЫЙ ЗАПУСК]";
                LauncherID.Enabled = true;
                GtaPathText.Enabled = true;
                CheckGameFile.Enabled = false;
                CheckProgramFile.Enabled = false;
                StartOnMyGTA.Enabled = false;
                AutoUpdateOnStart.Enabled = false;
                guna2TextBox5.Enabled = false;
                CloseLauncherOfStartSAMP.Enabled = false;
                UseArizonaLauncher.Enabled = false;
                changeProject.Enabled = false;
            }
            else
            {
                LauncherID.Text = settings.LauncherId.ToString();
                GtaPathText.Text = settings.GtaPath;
                StartOnMyGTA.Checked = settings.StartOnMyGta;
                AutoUpdateOnStart.Checked = settings.AutoUpdate;
                guna2TextBox5.Text = settings.NickName;
                CloseLauncherOfStartSAMP.Checked = settings.CloseLauncher;
                UseArizonaLauncher.Checked = settings.ArizonaLauncher;
            }

            fs.Dispose();
        }
    }

    private void ExitButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void Exit_Click(object sender, EventArgs e)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var cfg = new Dictionary<string, SettingsConfig>();

        if (UseArizonaLauncher.Checked && StartOnMyGTA.Checked)
        {
            MessageBox.Show("Нельзя запуститься одновременно со своей сборки и с лаунчера аризоны!");
            return;
        }

        if (File.Exists("C:\\Launcher\\Arizona\\game.zip"))
            cfg["LauncherSettings"] = new SettingsConfig
            {
                LauncherId = Convert.ToInt32(LauncherID.Text),
                FirstStart = false,
                AutoUpdate = AutoUpdateOnStart.Checked,
                GtaPath = GtaPathText.Text,
                NickName = guna2TextBox5.Text,
                StartOnMyGta = StartOnMyGTA.Checked,
                GtaInstalled = true,
                CloseLauncher = CloseLauncherOfStartSAMP.Checked,
                ArizonaLauncher = UseArizonaLauncher.Checked
            };
        else
            cfg["LauncherSettings"] = new SettingsConfig
            {
                LauncherId = Convert.ToInt32(LauncherID.Text),
                FirstStart = false,
                AutoUpdate = AutoUpdateOnStart.Checked,
                GtaPath = GtaPathText.Text,
                NickName = guna2TextBox5.Text,
                StartOnMyGta = StartOnMyGTA.Checked,
                GtaInstalled = false,
                CloseLauncher = CloseLauncherOfStartSAMP.Checked,
                ArizonaLauncher = UseArizonaLauncher.Checked
            };
        var a = new allConfig
        {
            SettingsConfigs = cfg
        };

        using (var fs = new FileStream("C:\\Launcher\\launcher.json", FileMode.Create, FileAccess.Write,
                   FileShare.ReadWrite))
        {
            JsonSerializer.Serialize(fs, a, options);
        }

        Application.Restart();
    }

    private void GtaPathText_Click(object sender, EventArgs e)
    {
        folderBrowserDialog1.Description = "Выбрать папку с GTA";
        folderBrowserDialog1.ShowNewFolderButton = false;
        folderBrowserDialog1.ShowDialog(this);
        GtaPathText.Text = folderBrowserDialog1.SelectedPath;
    }

    private void StartOnMyGTA_CheckedChanged(object sender, EventArgs e)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var oldJson = new allConfig();
        var cfg = new Dictionary<string, SettingsConfig>();
        using (var fs1 = new FileStream("C:\\Launcher\\launcher.json", FileMode.Open, FileAccess.Read,
                   FileShare.ReadWrite))
        {
            oldJson = JsonSerializer.Deserialize<allConfig>(fs1, options);
        }

        SettingsConfig config;
        oldJson.SettingsConfigs.TryGetValue("LauncherSettings", out config);
        cfg["LauncherSettings"] = new SettingsConfig
        {
            LauncherId = Convert.ToInt32(LauncherID.Text),
            FirstStart = false,
            AutoUpdate = AutoUpdateOnStart.Checked,
            GtaPath = GtaPathText.Text,
            NickName = guna2TextBox5.Text,
            StartOnMyGta = StartOnMyGTA.Checked,
            GtaInstalled = config.GtaInstalled,
            CloseLauncher = CloseLauncherOfStartSAMP.Checked
        };
        var a = new allConfig
        {
            SettingsConfigs = cfg
        };
        using (var fs = new FileStream("C:\\Launcher\\launcher.json", FileMode.Create, FileAccess.Write,
                   FileShare.ReadWrite))
        {
            JsonSerializer.Serialize(fs, a, options);
        }
    }

    private void AutoUpdateOnStart_CheckedChanged(object sender, EventArgs e)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var oldJson = new allConfig();
        var cfg = new Dictionary<string, SettingsConfig>();
        using (var fs1 = new FileStream("C:\\Launcher\\launcher.json", FileMode.Open, FileAccess.Read,
                   FileShare.ReadWrite))
        {
            oldJson = JsonSerializer.Deserialize<allConfig>(fs1, options);
        }

        SettingsConfig config;
        oldJson.SettingsConfigs.TryGetValue("LauncherSettings", out config);
        cfg["LauncherSettings"] = new SettingsConfig
        {
            LauncherId = Convert.ToInt32(LauncherID.Text),
            FirstStart = false,
            AutoUpdate = AutoUpdateOnStart.Checked,
            GtaPath = GtaPathText.Text,
            NickName = guna2TextBox5.Text,
            StartOnMyGta = StartOnMyGTA.Checked,
            GtaInstalled = config.GtaInstalled,
            CloseLauncher = CloseLauncherOfStartSAMP.Checked
        };
        var a = new allConfig
        {
            SettingsConfigs = cfg
        };
        using (var fs = new FileStream("C:\\Launcher\\launcher.json", FileMode.Create, FileAccess.Write,
                   FileShare.ReadWrite))
        {
            JsonSerializer.Serialize(fs, a, options);
        }
    }

    private void guna2TextBox5_TextChanged(object sender, EventArgs e)
    {
    }

    private void CloseLauncherOfStartSAMP_CheckedChanged(object sender, EventArgs e)
    {
    }

    private void changeProject_Click(object sender, EventArgs e)
    {
        File.Delete("C:\\Launcher\\launcher.json");
        File.Delete("C:\\Launcher\\servers.json");
        Thread.Sleep(150);
        Application.Restart();
    }

    private void UseArizonaLauncher_CheckedChanged(object sender, EventArgs e)
    {
        using (var fs = new FileStream("C:\\Launcher\\launcher.json", FileMode.OpenOrCreate))
        {
            string fileContents;
            using (var reader = new StreamReader(fs))
            {
                fileContents = reader.ReadToEnd();
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var obj = JsonSerializer.Deserialize<allConfig>(fileContents, options);
            SettingsConfig settings;
            obj.SettingsConfigs.TryGetValue("LauncherSettings", out settings);

            if (settings.GtaInstalled)
                GtaPathText.Text = "C:\\Launcher\\Arizona";
            else
                GtaPathText.Enabled = false;

            fs.Dispose();
        }
    }

    private void guna2Button1_Click(object sender, EventArgs e)
    {
    }
}