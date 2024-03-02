using System;
using System.Deployment.Application;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;
using System.Threading.Tasks;
using Guna.UI2.WinForms;

namespace Launcher;

public partial class LoadForm : Form
{
    public LoadForm()
    {
        InitializeComponent();
        Focus();
    }

    private void LoadForm_Load(object sender, System.EventArgs e)
    {
        var config = new Config();
            config.CheckConfig();
            label1.Text = "";
            var allconfig = new allConfig();
            allconfig.SerializeLauncher();
            //allconfig.SerealizeServers();
            using (var fs = new FileStream("C:\\Launcher\\launcher.json", FileMode.Open))
            {
                string c;
                using (var sr = new StreamReader(fs))
                {
                    c = sr.ReadToEnd();
                }

                var json = JsonSerializer.Deserialize<allConfig>(c, new JsonSerializerOptions { WriteIndented = true });
                SettingsConfig cgg;
                json.SettingsConfigs.TryGetValue("LauncherSettings", out cgg);
                if (cgg.FirstStart == true)
                {
                    Guna2MessageDialog messageDialog = new Guna2MessageDialog()
                    {
                        Buttons = MessageDialogButtons.OK,
                        Caption = "First Start",
                        Icon = MessageDialogIcon.Information,
                        Parent = this,
                        Style = MessageDialogStyle.Dark,
                        Text = $"Привет, ты первый раз запустил этот лаунчер!\n" +
                               $"После закрытия данного окна, введи ID проекта в первое поле и нажми на выход." +
                               $"Сейчас я тебе кратно расскажу что ты должен делать..\n" +
                               $"Если ты хочешь играть на своем сервере со сборки аризоны (все модели отображаются корректно)\n" +
                               $"Заходишь в настройки и ставишь галочку на использовать лаунчер Arizona Games.\n" +
                               $"Если возникают ошибки, пиши на форум https://fooza.ru/forum"
                    };
                    messageDialog.Show();
                    new Settings().Show(this);
                    return;
                }

                if (checkID(cgg.LauncherId) == false)
                {
                    Guna2MessageDialog messageDialog = new Guna2MessageDialog()
                    {
                        Buttons = MessageDialogButtons.OK,
                        Caption = "First Start",
                        Icon = MessageDialogIcon.Information,
                        Parent = this,
                        Style = MessageDialogStyle.Dark,
                        Text = $"Возникла ошибка!\n" +
                               $"Возможно вы указали ID проекта который не существует!\n" +
                               $"Пожалуйста, исправьте это.\n" +
                               $"Нажмите на кнопку - сменить проект, чтобы сбросить настройки."
                    };
                    messageDialog.Show();
                    new Settings().Show(this);
                    return;
                }

                if (!cgg.FirstStart && checkID(cgg.LauncherId))
                {
                    ShowInTaskbar = false;
                    ShowIcon = false;
                    Hide();
                    new Form1().Show(this);
                }

            }
    }
    public bool checkID(int id)
    {
        var request = JsonSerializer.Deserialize<Response>(
            new allConfig().GetContentOrWebSite($"https://fooza.ru/Launcher-Web/include/api/checkID.php?id={id}"));
        ;
        return request.requestResponse;
    }
}