using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Saturn.Backend.Core.Utils;

namespace Saturn.Client
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/wwwroot"))
            {
                string videoLink = "https://youtu.be/qEzkRXiHNBs";
                MessageBox.Show($"There was a problem reaching the wwwroot directory. To fix this, press OK and watch the video, or type this link in your browser: {videoLink}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                FileUtil.OpenBrowser(videoLink);
            }
            
            Directory.CreateDirectory(Config.BasePath);
            Directory.CreateDirectory(Config.LogPath);
            Directory.CreateDirectory(Config.PluginsPath);

            if (!File.Exists(Config.OodlePath))
                new WebClient().DownloadFile("https://cdn.discordapp.com/attachments/754879989614379042/926560284271870022/oo2core_5_win64.dll", Config.OodlePath);

            Logger.Start();
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                Logger.Log("There was an error in the application.\n" + error.ExceptionObject);

                if (error.ExceptionObject.ToString().ToLower().Contains("couldn't find a compatible webview2"))
                {
                    string videoLink = "https://youtu.be/xuY3ddXBbsg";
                    MessageBox.Show($"There was a problem connecting to WebView2 services. To fix this, press OK and watch the video, or type this link in your browser: {videoLink}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    FileUtil.OpenBrowser(videoLink);
                }
                else if (error.ExceptionObject.ToString().Contains("Saturn.Backend.Data.Utils.FortniteUtil.GetFortnitePath()") || error.ExceptionObject.ToString().Contains("System.ArgumentException: Given directory must exist (Parameter '_workingDirectory')"))
                {
                    string videoLink = "https://youtu.be/j8KQdmHtq4k";
                    MessageBox.Show($"There was a problem getting Fortnite's path from the Epic Games Launcher. To fix this, press OK and watch the video, or type this link in your browser: {videoLink}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    FileUtil.OpenBrowser(videoLink);
                }
                else if (error.ExceptionObject.ToString().Contains("(tamelyapi.azurewebsites.net:443)"))
                {
                    string videoLink = "https://youtu.be/JCod7KudTtg";
                    MessageBox.Show($"There was a problem connecting to the internet. If you are sure you have internet connection, to fix this, press OK and watch the video, or type this link in your browser: {videoLink}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    FileUtil.OpenBrowser(videoLink);
                }
                else
                {
                    MessageBox.Show(
                        "There was an error in the application, please press OK then report the log to Support!", "Ask for Support in Tamely's Discord Server!");
                    Process.Start("notepad.exe", Config.LogFile);
                }
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // When the program closes
        }
    }
}