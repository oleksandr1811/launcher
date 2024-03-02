using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher.CustomControl;

public partial class servermenu : UserControl
{
    public servermenu()
    {
        InitializeComponent();
    }

    public string ServerName { get; set; }
    public string ServerIP { get; set; }
    public int ServerPort { get; set; }
    public string imgUrl { get; set; }

    public Image SetImage(string url)
    {
        var request = System.Net.WebRequest.Create(url);
        var response = request.GetResponse();
        Bitmap loadedBitmap = null;
        using (var responseStream = response.GetResponseStream())
        {
            loadedBitmap = new Bitmap(responseStream);
        }

        return (Image)loadedBitmap;
    }

    private void servermenu_Load(object sender, EventArgs e)
    {
        Task.Run(() =>
        {
            var hostname = new Form1().GetHostName(ServerIP, ServerPort);
                if (hostname == "Сервер не отвечает")
                {
                    OnLoad(new EventArgs());
                }
                else
                {
                    label1.Text = hostname.Normalize();
                    label2.Text =
                        $"{new Form1().GetServerOnline(ServerIP, ServerPort)} из {new Form1().GetMaxPlayers(ServerIP, ServerPort)}";
                    guna2CirclePictureBox1.Image = SetImage(imgUrl);
                }
        });
    }

    private string _selectIp;
    private int _selectPort;


    public void startArizona(SettingsConfig settingsConfig)
    {
        if (!File.Exists("server.txt"))
        {
            return;
        }
        else
        {
            _selectIp = File.ReadAllText("server.txt");
            _selectPort = Convert.ToInt32(File.ReadAllText("port.txt"));
            if (_selectIp == "" || _selectPort == 0)
            {
                var guna2MessageDialog = new Guna2MessageDialog()
                {
                    Text = $"При попытке запуска произошла ошибка!\n" +
                           $"Возможное проблемы, это:\n" +
                           $"1. Не выбран сервер\n" +
                           $"2. Не указан ник.\n" +
                           $"3. Не указан путь к сборке аризоны.",
                    Buttons = MessageDialogButtons.OK,
                    Caption = "Server Connect System",
                    Icon = MessageDialogIcon.Information,
                    Style = MessageDialogStyle.Dark
                };
                guna2MessageDialog.Show();
                return;
            }

            // -arizona -c -h 46.174.50.193 -p 7777 -mem 4096 -n Test_Arizona -ldo -seasons 
            var form = new Form2();
            form.Height = SystemInformation.PrimaryMonitorSize.Height;
            form.Width = SystemInformation.PrimaryMonitorSize.Width;
            form.FormBorderStyle = FormBorderStyle.None;
            form.ShowInTaskbar = false;
            form.ShowIcon = false;
            form.WindowState = FormWindowState.Maximized;
            form.Show();
            form.Focus();

            var t = new Thread(() =>
            {
                if (settingsConfig.CloseLauncher)
                {
                    Thread.Sleep(3000);
                    Process.Start($"{settingsConfig.GtaPath}\\gta_sa.exe",
                        $"-arizona -c -h {_selectIp} -p {_selectPort} -mem 4096 -n {settingsConfig.NickName} -ldo -seasons");
                    Application.Exit();
                }
                else
                {
                    Thread.Sleep(3000);
                    Directory.SetCurrentDirectory($"{settingsConfig.GtaPath}");
                    form.Close();
                    Process.Start($"{settingsConfig.GtaPath}\\gta_sa.exe",
                            $"-arizona -c -h {_selectIp} -p {_selectPort} -mem 4096 -n {settingsConfig.NickName} -ldo -seasons")
                        .WaitForExit();
                }
            });
            t.Start();
        }
    }

    public void StartGame(SettingsConfig settingsConfig)
    {
        if (!File.Exists("server.txt"))
        {
            return;
        }
        else
        {
            _selectIp = File.ReadAllText("server.txt");
            _selectPort = Convert.ToInt32(File.ReadAllText("port.txt"));
            if (_selectIp == "" && _selectPort == 0)
            {
                var guna2MessageDialog = new Guna2MessageDialog()
                {
                    Text = $"Error: Ошибка получения IP адреса сервера...\n" +
                           $"Выберите сервер в таблице справа.",
                    Buttons = MessageDialogButtons.OK,
                    Caption = "Server Connect System",
                    Icon = MessageDialogIcon.Information,
                    Style = MessageDialogStyle.Dark
                };
                guna2MessageDialog.Show();
                return;
            }
            else if (settingsConfig.NickName == "")
            {
                var guna2MessageDialog = new Guna2MessageDialog()
                {
                    Text = $"Error: Установите ник в настройках.",
                    Buttons = MessageDialogButtons.OK,
                    Caption = "Server Connect System",
                    Icon = MessageDialogIcon.Information,
                    Style = MessageDialogStyle.Dark
                };
                guna2MessageDialog.Show();
                return;
            }

            if (settingsConfig.CloseLauncher)
            {
                Process.Start(settingsConfig.GtaPath + "\\samp.exe",
                    _selectIp + ":" + _selectPort + " -n" + settingsConfig.NickName);
                Application.Exit();
            }
            else
            {
                Process.Start(settingsConfig.GtaPath + "\\samp.exe",
                    _selectIp + ":" + _selectPort + " -n" + settingsConfig.NickName).WaitForExit();
            }
        }
    }

    private void servermenu_Click(object sender, EventArgs e)
    {
        var guna2MessageDialog = new Guna2MessageDialog()
        {
            Text = $"Выбрать сервер {label1.Text}\n" +
                   $"Для входа?",
            Buttons = MessageDialogButtons.YesNo,
            Caption = "Server Connect System",
            Icon = MessageDialogIcon.Information,
            Style = MessageDialogStyle.Dark
        };
        var res = guna2MessageDialog.Show();
        if (res == DialogResult.Yes)
        {
            File.WriteAllText("server.txt", $"{ServerIP}");
            File.WriteAllText("port.txt", $"{ServerPort}");
            BorderStyle = BorderStyle.Fixed3D;
        }
        else
        {
            BorderStyle = BorderStyle.None;
            File.WriteAllText("server.txt", $"");
            File.WriteAllText("port.txt", $"");
        }
    }
}