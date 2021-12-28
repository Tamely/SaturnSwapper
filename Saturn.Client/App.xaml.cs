using System;
using System.Diagnostics;
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
            Logger.Start();
            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                Logger.Log("There was an error in the application.\n" + error.ExceptionObject);
                MessageBox.Show(
                    "There was an error in the application, please report this log to Support when you make a ticket!",
                    "Make a ticket in the Discord Server!");
                Process.Start("notepad.exe", Config.LogFile);
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // When the program closes
        }
    }
}