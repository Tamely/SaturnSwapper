using System;
using System.Runtime.ExceptionServices;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Radon.Common;

namespace Radon.Ide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var services = new ServiceCollection();
            services.AddBlazorWebView();
            var serviceProvider = services.BuildServiceProvider();
            Resources.Add("services", serviceProvider);
            Title = "Radon IDE";
            Loaded += OnLoaded;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
            InitializeComponent();
        }
        
        public async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Logger.Log("Ensuring WebView2", LogLevel.Info);
            await BlazorWebView.WebView.EnsureCoreWebView2Async().ConfigureAwait(continueOnCapturedContext: true);
            BlazorWebView.WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            BlazorWebView.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            BlazorWebView.WebView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            BlazorWebView.WebView.CoreWebView2.Settings.IsStatusBarEnabled = true;
            BlazorWebView.WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
            Logger.Log("WebView2 has been ensured", LogLevel.Info);
        }
        
        private void CurrentDomainOnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        {
            Logger.Log(e.Exception.ToString(), LogLevel.Error);
        }
    }
}