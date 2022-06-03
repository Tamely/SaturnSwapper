using System.Diagnostics;
using System.IO;

namespace Saturn.Backend.Core.Utils;

public class Uninstaller
{
    public static void Uninstall()
    {
        foreach (var gameFile in Directory.GetFiles(FortniteUtil.GetFortnitePath()))
            if (gameFile.Contains("SaturnClient"))
                File.Delete(gameFile);
        Logger.Log("Uninstalled Saturn files from Fortnite");
        
        // Get the path to the exe
        var exePath = Process.GetCurrentProcess().MainModule.FileName;
        // Get the path to the directory the exe is in
        var exeDir = new FileInfo(exePath).DirectoryName;
        
        // Create a bat file to wait 5 seconds then delete "Saturn.exe", the "wwwroot" folder, and Config.BasePath then delete itself
        var batFile = Path.Combine(Path.GetTempPath(), "Uninstaller.bat");
        File.WriteAllText(batFile, $"timeout 3\r\ndel \"{Path.Combine(exeDir, "oo2core_9_win64.dll")}\r\ndel \"{Path.Combine(exeDir, "Saturn.exe")}\"\r\ndel \"{Path.Combine(exeDir, "wwwroot")}\r\ndel \"{Path.Combine(exeDir, "Saturn.exe.WebView2")}\"\r\ndel \"{Config.BasePath}\"\r\ndel \"{Config.BasePath}\\Plugins\"\r\ndel \"{Config.BasePath}\\Mappings\"\r\ndel \"{Config.BasePath}\\Logs\"\r\ndel \"{batFile}\"");
        Logger.Log("Created uninstall bat file");
        
        // Run the bat file
        Process.Start(batFile);
        
        Process.GetCurrentProcess().Kill();
    }
}