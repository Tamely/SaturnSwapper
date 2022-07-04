using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using Saturn.Backend.Core.Utils;

namespace Saturn.Backend.Core.Services;

public interface INotificationService
{
    public Task Success(string message, bool bShowButton = false,
        string buttonMessage = "OK");
    public Task Error(string message, bool bShowButton = false,
        string buttonMessage = "Open Log");

    public Task Warn(string message);
}

public class NotificationService : INotificationService
{
    private readonly string _exePath;
    public NotificationService()
    {
        _exePath = new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;

        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            var args = ToastArguments.Parse(toastArgs.Argument);
            if (!args.Contains("action"))
                return;

            switch (args.Get("action"))
            {
                case "Launch Fortnite":
                    FortniteUtil.LaunchFortnite();
                    break;
                case "Open Log":
                    Process.Start("notepad.exe", Config.LogFile);
                    break;
            }
        };
    }

    public async Task Success(string message, bool bShowButton = false, string buttonMessage = "OK")
    {
        var builder = new ToastContentBuilder()
            .AddAudio(new ToastAudio() { Silent = true })
            .AddAppLogoOverride(new Uri($"{_exePath}\\wwwroot\\img\\Saturn.png"), ToastGenericAppLogoCrop.Circle)
            .AddText("Success!");
        
        if (bShowButton)
            builder.AddButton(new ToastButton()
                .SetContent(buttonMessage)
                .AddArgument("action", buttonMessage)
                .SetBackgroundActivation());
        
        builder.AddText(message);

        builder.Show();
    }
    
    public async Task Error(string message, bool bShowButton = false, string buttonMessage = "Open Log")
    {
        var builder = new ToastContentBuilder()
            .AddAudio(new ToastAudio() { Silent = true })
            .AddAppLogoOverride(new Uri($"{_exePath}\\wwwroot\\img\\Saturn.png"), ToastGenericAppLogoCrop.Circle)
            .AddText("There has been an error!");
        
        if (bShowButton)
            builder.AddButton(new ToastButton()
                .SetContent(buttonMessage)
                .AddArgument("action", buttonMessage)
                .SetBackgroundActivation());
        
        builder.AddText(message);

        builder.Show();
    }
    
    public async Task Warn(string message)
    {
        var builder = new ToastContentBuilder()
            .AddAudio(new ToastAudio() { Silent = true })
            .AddAppLogoOverride(new Uri($"{_exePath}\\wwwroot\\img\\Saturn.png"), ToastGenericAppLogoCrop.Circle)
            .AddText(message);

        builder.Show();
    }
}