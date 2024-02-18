using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using Saturn.Backend.Data;
using Saturn.Backend.Data.Variables;

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
                Utilities.OpenBrowser(videoLink);
            }

            Directory.CreateDirectory(Constants.BasePath);
            Directory.CreateDirectory(Constants.DataPath);
            Directory.CreateDirectory(Constants.LogPath);
            Directory.CreateDirectory(Constants.ExternalPath);
            Directory.CreateDirectory(Constants.PluginPath);
            Directory.CreateDirectory(Constants.MappingsPath);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // When the program closes
        }
    }
}