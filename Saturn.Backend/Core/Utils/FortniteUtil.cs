using Newtonsoft.Json;
using Saturn.Backend.Core.Models.Epic_Games;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Saturn.Backend.Core.Utils
{
    public class FortniteUtil
    {
        public static string PakPath
            => GetFortnitePath() + @"\FortniteGame\Content\Paks";

        public static string GetFortnitePath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Epic\\UnrealEngineLauncher\\LauncherInstalled.dat");

            return !File.Exists(path)
                   ? null
                   : JsonConvert.DeserializeObject<InstalledApps>(File.ReadAllText(path)).InstallationList
                                .FirstOrDefault(x => x.AppName == "Fortnite").InstallLocation;
        }

        public static void LaunchFortnite()
        {
            Process.Start(new ProcessStartInfo // Create a new process
            {
                FileName = "cmd.exe", // Use cmd.exe
                Arguments = "/c start com.epicgames.launcher://apps/Fortnite?action=launch&silent=true", // Set the arguments we want
                WindowStyle = ProcessWindowStyle.Hidden, // Hide the window
                CreateNoWindow = true // Don't create a new window
            }); // Start the process
        }
        public static string GetFortniteVersion()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Epic\\UnrealEngineLauncher\\LauncherInstalled.dat");

            return JsonConvert.DeserializeObject<InstalledApps>(File.ReadAllText(path)).InstallationList
                              .FirstOrDefault(x => x.AppName == "Fortnite").AppVersion;
        }
    }
}