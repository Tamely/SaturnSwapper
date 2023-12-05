using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Radon.Runtime;
using Saturn.Backend.Data;
using Saturn.Backend.Data.Fortnite;
using Saturn.Backend.Data.FortniteCentral;
using Saturn.Backend.Data.SaturnAPI;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.Variables;

namespace Saturn.Client
{
    /// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeAsync();
            InitializeComponent();
        }
        private async Task InitializeAsync()
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            Title = "Saturn Swapper - v" + Constants.USER_VERSION + (Constants.isBeta ? "-BETA" : "");
            
            var services = new ServiceCollection();
            services.AddBlazorWebView();

            services.AddScoped<ISaturnAPIService, SaturnAPIService>();
            services.AddScoped<IFortniteCentralService, FortniteCentralService>();
            services.AddScoped<LocalizationResourceService>();
            services.AddScoped<DiscordService>();

            var serviceProvider = services.BuildServiceProvider();
            Resources.Add("services", serviceProvider);

            Loaded += OnLoaded;
            
            base.Width = 1179.0;
            base.Height = 660.0;
            base.MinWidth = 939.0;
            base.MinHeight = 517.0;
            base.ResizeMode = ResizeMode.CanResize;
        }
        
        private async void OnLoaded(object _, RoutedEventArgs e)
        {
            Logger.Log("Ensuring WebView2");
            await blazorWebView.WebView.EnsureCoreWebView2Async().ConfigureAwait(continueOnCapturedContext: true);
            blazorWebView.WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            blazorWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            blazorWebView.WebView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            blazorWebView.WebView.CoreWebView2.Settings.IsStatusBarEnabled = true;
            blazorWebView.WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
            Logger.Log("WebView2 has been ensured");
        }
        
        private void CurrentDomainOnProcessExit(object? sender, EventArgs e)
        {
            if (Constants.isClosingCorrectly) return;
            foreach (var file in Directory.EnumerateFiles(Constants.DataPath))
            {
                File.Delete(file);
            }

            foreach (var file in Shared.AllowedFiles)
            {
                if (File.Exists(DataCollection.GetGamePath() + file + ".utoc"))
                {
                    File.Delete(DataCollection.GetGamePath() + file + ".utoc");
                }
                
                if (File.Exists(DataCollection.GetGamePath() + file + ".ucas"))
                {
                    File.Delete(DataCollection.GetGamePath() + file + ".ucas");
                }
                
                if (File.Exists(DataCollection.GetGamePath() + file + ".pak"))
                {
                    File.Delete(DataCollection.GetGamePath() + file + ".pak");
                }
                
                if (File.Exists(DataCollection.GetGamePath() + file + ".sig"))
                {
                    File.Delete(DataCollection.GetGamePath() + file + ".sig");
                }
            }
        }
        
        private void CurrentDomainOnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        {
            Exception exception = e.Exception;
            if (exception.StackTrace != null && (exception.StackTrace!.Contains("ToastNotification") || (exception.StackTrace!.Contains("CUE4Parse") && !exception.StackTrace.Contains("IoPackage") && !exception.StackTrace.Contains("Property") && !exception.StackTrace.Contains("FAssetRegistryState") && !exception.StackTrace.Contains("UObject"))))
            {
                return;
            }
            
            if (exception.StackTrace != null && exception.StackTrace!.Contains("There is an update ready to download!"))
            {
                MessageBox.Show("There is an update ready to download! Please download it from discord.gg/SaturnSwapper.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
            }

            if (!(exception is WebException ex))
            {
                if (!exception.StackTrace.ToLower().Contains("webview2"))
                {
                    if (exception is FileNotFoundException || exception is DllNotFoundException)
                    {
                        Logger.Log("An exception occured.", LogLevel.Error);
                        Logger.Log(exception.GetType().Name + ": " + exception.ToString(), LogLevel.Error);
                        Logger.Log(exception.StackTrace ?? "", LogLevel.Error);
                        
                        MessageBox.Show(
                            "Important files seem to have been deleted from Saturn. Please redownload Saturn from discord.gg/SaturnSwapper and check if your antivirus might be deleting files.\n\n" +
                            e.Exception.Message, "Files missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Process.Start(Constants.LogFile);
                        Environment.Exit(0);
                    }
                    else if (!(exception is COMException) && !(exception is Win32Exception) &&
                             (exception.StackTrace == null || exception.StackTrace!.Contains("Saturn.Backend")))
                    {
                        Logger.Log("An exception occured.", LogLevel.Error);
                        Logger.Log(exception.GetType().Name + ": " + exception.ToString(), LogLevel.Error);
                        Logger.Log(exception.StackTrace ?? "", LogLevel.Error);
                        MessageBox.Show(
                            "An error has occurred performing this operation. Please try again later.\n\n" + exception.Message,
                            "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Process.Start(Constants.LogFile);
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Logger.Log("An exception occured.", LogLevel.Error);
                    Logger.Log(exception.GetType().Name + ": " + exception.ToString(), LogLevel.Error);
                    Logger.Log(exception.StackTrace ?? "", LogLevel.Error);
                    
                    if (MessageBox.Show("You need to install Microsoft Edge WebView 2 to use Saturn. Do you want to install it now?", "Missing WebView2", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Utilities.OpenBrowser("https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/2a723731-d64d-4119-8214-9781c986c21b/MicrosoftEdgeWebView2RuntimeInstallerX64.exe");
                    }
                    Process.Start(Constants.LogFile);
                    Environment.Exit(0);
                }
            }
        }
    }

    // Workaround for compiler error "error MC3050: Cannot find the type 'local:Main'"
    // It seems that, although WPF's design-time build can see Razor components, its runtime build cannot.
    public class Main
    {
    }
}