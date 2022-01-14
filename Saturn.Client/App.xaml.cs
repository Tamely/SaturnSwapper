using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Saturn.Backend.Data.Utils;

namespace Saturn.Client
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Directory.CreateDirectory(Config.BasePath);
            Directory.CreateDirectory(Config.LogPath);

            if (!File.Exists(Config.OodlePath))
                new WebClient().DownloadFile("https://cdn.discordapp.com/attachments/754879989614379042/926560284271870022/oo2core_5_win64.dll", Config.OodlePath);

            Logger.Start();
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                Logger.Log("There was an error in the application.\n" + error.ExceptionObject);

                if (error.ExceptionObject.ToString().ToLower().Contains("webview"))
                {
                    MessageBox.Show(
                        "There was an error in the application, please report this log to Support! This is most likely caused by you not installed WebView2!",
                        "Ask for Support in Tamely's Discord Server!");
                }
                else if (error.ExceptionObject.ToString().ToLower().Contains("checkifuserisbeta"))
                {
                    MessageBox.Show(
                        "There was an error in the application, please report this log to Support! This is most likely caused by you not being on Discord Desktop!",
                        "Ask for Support in Tamely's Discord Server!");
                }
                else
                {
                    MessageBox.Show(
                        "There was an error in the application, please report this log to Support!",
                        "Ask for Support in Tamely's Discord Server!");
                }
                Process.Start("notepad.exe", Config.LogFile);
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // When the program closes
        }
    }
}