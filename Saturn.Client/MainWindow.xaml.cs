using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Saturn.Backend.Data.Services;

namespace Saturn.Client
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var services = new ServiceCollection();
            services.AddBlazorWebView();

            services.AddScoped<IConfigService, ConfigService>();

            services.AddScoped<IDiscordRPCService, DiscordRPCService>();
            services.AddScoped<IFortniteAPIService, FortniteAPIService>();
            services.AddScoped<ISaturnAPIService, SaturnAPIService>();
            
            services.AddScoped<ICloudStorageService, CloudStorageService>();

            services.AddScoped<ISwapperService, SwapperService>();

            Resources.Add("services", services.BuildServiceProvider());

            InitializeComponent();
        }
    }

    // Workaround for compiler error "error MC3050: Cannot find the type 'local:Main'"
    // It seems that, although WPF's design-time build can see Razor components, its runtime build cannot.
    public class Main
    {
    }
}