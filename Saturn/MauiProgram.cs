using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Saturn.Data;
using Saturn.Data.Services;

namespace Saturn
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .RegisterBlazorMauiWebView()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddBlazorWebView();

            builder.Services.AddScoped<ISwapperService, SwapperService>();

            builder.Services.AddScoped<IFortniteAPIService, FortniteAPIService>();
            builder.Services.AddScoped<ISaturnAPIService, SaturnAPIService>();
            builder.Services.AddScoped<IConfigService, ConfigService>();

            builder.Services.AddScoped<IDiscordRPCService, DiscordRPCService>();
           

            

            return builder.Build();
        }
    }
}