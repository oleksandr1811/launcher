using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Ionic.Zip;
using Launcher.CustomControl;
using Launcher.Properties;
using Microsoft.Win32;

namespace Launcher
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            Text = "Главное окно";
            StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetLauncherSettings();
            progressbar.Value = 0;
            Task.Run(GetFirstServ);
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    if (File.Exists("server.txt") && File.Exists("port.txt"))
                    {
                        if (File.ReadAllText("server.txt") == "" && File.ReadAllText("port.txt") == "")
                        {
                            StartGameButton.Enabled = false;
                            label4.Text = "Вход на Not Found";
                        }
                        else
                        {
                            label4.Text = $@"Вход на {File.ReadAllText("server.txt")}:{File.ReadAllText("port.txt")} ({GetHostName(File.ReadAllText("server.txt"), Convert.ToInt32(File.ReadAllText("port.txt")))})";
                            StartGameButton.Enabled = true;
                        }
                    }
                }
            });
            Guna2CustomRadioButton1_Click(guna2CustomRadioButton1, EventArgs.Empty);
        }

        private void GetLauncherSettings()
        {
            var a = new allConfig().GetContentOrWebSite($"https://fooza.ru/Launcher-Web/include/api/getLauncherInfo.php?launcherid={settingsConfig().LauncherId}");
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
                    guna2Button1.Enabled = false;
                    guna2Button3.Enabled=false;
                    guna2Button2.Enabled = false;
                }
                else
            {
                var json = JsonSerializer.Deserialize<configs.Launcher>(a);
                    Logotype.ImageLocation = json.logoImg;
                    guna2HtmlLabel1.Text = json.name;
                    new Settings().guna2CirclePictureBox2.ImageLocation = json.logoImg;
                    guna2Button1.Click += delegate(object sender, EventArgs args)
                    {
                        Process.Start(json.vk);
                    };
                    guna2Button2.Click += delegate(object sender, EventArgs args)
                    {
                        Process.Start(json.site);
                    };
                    guna2Button3.Click += delegate(object sender, EventArgs args)
                    {
                        Process.Start(json.forum);
                    };
            }
        }



        public allConfig globalConfig()
        {
            string cc;
            using (var fs = new FileStream("C:\\Launcher\\launcher.json", FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    cc = sr.ReadToEnd();
                }
            }

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Deserialize<allConfig>(cc, options);
        }

        public SettingsConfig settingsConfig()
        {
            SettingsConfig c;
            globalConfig().SettingsConfigs.TryGetValue("LauncherSettings", out c);
            return c;
        }
        public Servera servers()
        {
            var server =
                JsonSerializer.Deserialize<Servera>(
                    new allConfig().GetContentOrWebSite(
                        $"https://fooza.ru/Launcher-Web/include/api/getServers.php?id={settingsConfig().LauncherId}"));
            return server;
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            var settings = new Settings();
            settings.Show(this);
        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        public int GetMaxPlayers(string ip, int port)
        {
            var api = new SampQuery(ip, (ushort)port, 'i');
            var response = api.Read();
            if (response.ContainsKey("maxplayers"))
            {
                var players = response["maxplayers"];
                return Convert.ToInt32(players);
            }

            return 0;
        }

        public int GetServerOnline(string ip, int port)
        {
            var api = new SampQuery(ip, (ushort)port, 'i');
            var response = api.Read();
            if (response.ContainsKey("players"))
            {
                var players = response["players"];
                return Convert.ToInt32(players);
            }

            return 0;
        }

        public string GetHostName(string ip, int port)
        {
            var api = new SampQuery(ip, (ushort)port, 'i');
            var response = api.Read();
            if (response.ContainsKey("hostname"))
            {
                var hostname = response["hostname"];
                return hostname;
            }

            return "Сервер не отвечает.";
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            // -arizona -c -h 80.66.82.144 -p 7777 -mem 4096 -n Yan_Monesy
            try
            {
                var saveKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SAMP", true);
                saveKey.SetValue("gta_sa_exe", settingsConfig().GtaPath + "\\gta_sa.exe");
                saveKey.SetValue("PlayerName", settingsConfig().NickName);
                saveKey.Close();
                if (settingsConfig().ArizonaLauncher == true)
                {
                    if (settingsConfig().GtaInstalled == true)
                        new servermenu().startArizona(settingsConfig());
                    else
                        Download();
                }
                else
                {
                    new servermenu().StartGame(settingsConfig());
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Download()
        {
            if (!Directory.Exists("C:\\Launcher\\Arizona"))
            {
                Directory.CreateDirectory("C:\\Launcher\\Arizona");
                Download();
            }

            using (var downloadService = new WebClient())
            {
                downloadService.DownloadFileTaskAsync("https://fooza.ru/Launcher-Web/files/game.zip",
                    "C:\\Launcher\\Arizona\\game.zip");
                downloadService.UseDefaultCredentials = false;
                MessageBox.Show("Загрузка началась!\n" +
                                "Пожалуйста, подождите..");
                downloadService.DownloadProgressChanged += (s, e) => { progressbar.Value = (int)e.ProgressPercentage; };
                downloadService.DownloadFileCompleted += (s, e) =>
                {
                    if (e.Error == null) Task.Run(UnpackGta);
                };
            }
        }

        private async void UnpackGta()
        {
            IProgress<int> progress = new Progress<int>(p => progressbar.Value = p);
            try
            {
                await Task.Run(() =>
                {
                    var oldProgress = 0;
                    using (var zip = new ZipFile("C:\\Launcher\\Arizona\\game.zip"))
                    {
                        var currentEntry = 0;
                        var totalEntries = zip.Entries.Count;
                        zip.ExtractProgress += (s, e) =>
                        {
                            //ServerName.Text = "File: "+e.CurrentEntry.FileName.ToString();
                            if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
                            {
                                currentEntry++;
                            }
                            else if (e.TotalBytesToTransfer > 0)
                            {
                                var newProgress = currentEntry * 100 / totalEntries +
                                                  (int)(e.BytesTransferred * 100 / e.TotalBytesToTransfer /
                                                        totalEntries);
                                if (newProgress != oldProgress)
                                {
                                    progress.Report(newProgress);
                                    oldProgress = newProgress;
                                }
                            }
                        };

                        zip.ExtractAll("C:\\Launcher\\Arizona", ExtractExistingFileAction.OverwriteSilently);
                    }

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    var cfg = new Dictionary<string, SettingsConfig>();
                    cfg["LauncherSettings"] = new SettingsConfig
                    {
                        LauncherId = settingsConfig().LauncherId,
                        FirstStart = false,
                        AutoUpdate = settingsConfig().AutoUpdate,
                        GtaPath = "C:\\Launcher\\Arizona",
                        NickName = settingsConfig().NickName,
                        StartOnMyGta = settingsConfig().StartOnMyGta,
                        GtaInstalled = true,
                        CloseLauncher = settingsConfig().CloseLauncher,
                        ArizonaLauncher = settingsConfig().ArizonaLauncher
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
                });
            }
            catch (ZipException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string[] GetNews(string id)
        {
            var a = new allConfig().GetContentOrWebSite(
                $"https://fooza.ru/Launcher-Web/include/api/getNews.php?id={id}");
            Newss b;
            if (a == "Не удалось найти новости")
            {
                NewsPicture1.ImageLocation = "https://blog.vverh.digital/wp-content/uploads/2020/06/oblojka-404.png";
                //  NewsPicture2.ImageLocation = "https://blog.vverh.digital/wp-content/uploads/2020/06/oblojka-404.png";
                return new[] { "Не удалось найти новости" };
            }

            b = JsonSerializer.Deserialize<Newss>(a);
            return b.News;
        }

        public NewsConfig newsConfig(string newsid)
        {
            var a = new allConfig().GetContentOrWebSite(
                $"https://fooza.ru/Launcher-Web/include/api/getNewsInfo.php?newsid={newsid}");
            if (a != "Нет новостей!")
            {
                var newsConfig = JsonSerializer.Deserialize<NewsConfig>(
                    new allConfig().GetContentOrWebSite(
                        $"https://fooza.ru/Launcher-Web/include/api/getNewsInfo.php?newsid={newsid}"));
                return newsConfig;
            }

            return new NewsConfig()
            {
                Description = "Новости не установлены",
                imageUrl = "",
                newsid = "-1",
                ownerid = "-1",
                pictureid = "1"
            };
        }


        private void Guna2CustomRadioButton4_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                foreach (var VARIABLE in GetNews(settingsConfig().LauncherId.ToString()))
                    if (VARIABLE != "Не удалось найти новости")
                        if (newsConfig(VARIABLE).pictureid == "4")
                        {
                            label1.Text = $"{newsConfig(VARIABLE).Name}";
                            label3.Text = newsConfig(VARIABLE).Description;
                            NewsPicture1.ImageLocation = newsConfig(VARIABLE).imageUrl;
                            return;
                        }
            });
        }

        private void Guna2CustomRadioButton3_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                foreach (var VARIABLE in GetNews(settingsConfig().LauncherId.ToString()))
                    if (VARIABLE != "Не удалось найти новости")
                        if (newsConfig(VARIABLE).pictureid == "3")
                        {
                            label1.Text = $"{newsConfig(VARIABLE).Name}";
                            label3.Text = newsConfig(VARIABLE).Description;
                            NewsPicture1.ImageLocation = newsConfig(VARIABLE).imageUrl;
                            return;
                        }
            });
        }

        private void Guna2CustomRadioButton2_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                foreach (var VARIABLE in GetNews(settingsConfig().LauncherId.ToString()))
                    if (VARIABLE != "Не удалось найти новости")
                        if (newsConfig(VARIABLE).pictureid == "2")
                        {
                            label1.Text = $"{newsConfig(VARIABLE).Name}";
                            label3.Text = newsConfig(VARIABLE).Description;
                            NewsPicture1.ImageLocation = newsConfig(VARIABLE).imageUrl;
                            return;
                        }
            });
        }

        private void Guna2CustomRadioButton1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                foreach (var VARIABLE in GetNews(settingsConfig().LauncherId.ToString()))
                    if (VARIABLE != "Не удалось найти новости")
                        if (newsConfig(VARIABLE).pictureid == "1")
                        {
                            label1.Text = $"{newsConfig(VARIABLE).Name}";
                            label3.Text = newsConfig(VARIABLE).Description;
                            NewsPicture1.ImageLocation = newsConfig(VARIABLE).imageUrl;
                            return;
                        }
            });
        }

        public void GetFirstServ()
        {
            var t = new Thread(() =>
            {
                var controls = new List<Control>();
                foreach (var item in new Form1().servers().Servers)
                {
                    var aa = new servermenu()
                    {
                        ServerName = new Web().ServerConfig(item).ServerName,
                        ServerIP = new Web().ServerConfig(item).ServerIP,
                        ServerPort = Convert.ToInt32(new Web().ServerConfig(item).ServerPort),
                        imgUrl = new Web().ServerConfig(item).imgUrl,
                        Dock = DockStyle.Top
                    };
                    controls.Add(aa);
                }

                foreach (var item in controls)
                    guna2Panel3.Invoke(new EventHandler(delegate { guna2Panel3.Controls.Add(item); }));
            });
            t.Start();
        }

        private void NewsPicture1_Click(object sender, EventArgs e)
        {
            if (guna2CustomRadioButton1.Checked)
            {
                foreach (var newsid in GetNews(settingsConfig().LauncherId.ToString()))
                    if (newsConfig(newsid).pictureid == "1")
                    {
                        Process.Start($"{newsConfig(newsid).url}");
                        return;
                    }
            }
            else if (guna2CustomRadioButton2.Checked)
            {
                foreach (var newsid in GetNews(settingsConfig().LauncherId.ToString()))
                    if (newsConfig(newsid).pictureid == "2")
                    {
                        Process.Start($"{newsConfig(newsid).url}");
                        return;
                    }
            }
            else if (guna2CustomRadioButton3.Checked)
            {
                foreach (var newsid in GetNews(settingsConfig().LauncherId.ToString()))
                    if (newsConfig(newsid).pictureid == "3")
                    {
                        Process.Start($"{newsConfig(newsid).url}");
                        return;
                    }
            }
            else if (guna2CustomRadioButton4.Checked)
            {
                foreach (var newsid in GetNews(settingsConfig().LauncherId.ToString()))
                    if (newsConfig(newsid).pictureid == "1")
                    {
                        Process.Start($"{newsConfig(newsid).url}");
                        return;
                    }
            }
        }

        private void guna2CustomGradientPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void Logotype_Click(object sender, EventArgs e)
        {

        }
    }
}

public class Connect
{
    public string Ip { get; set; }
    public int Port { get; set; }
}