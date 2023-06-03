using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Saturn.Backend.Data.Fortnite.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Saturn.Backend.Data.Fortnite
{
    public class DataCollection
    {
        public static string GetGamePath()
        {
            if (File.Exists("C:\\ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat"))
            {
                FortniteInstallationModel? model = JsonSerializer.Deserialize<FortniteInstallationModel>(File.ReadAllText("C:\\ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat"));
                if (model == null)
                    return string.Empty;
                if (model.InstallationList.Any(x => x.AppName == "Fortnite"))
                    return model.InstallationList.First(x => x.AppName == "Fortnite").InstallLocation + "\\FortniteGame\\Content\\Paks";
            }

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                            "//.config/legendary/installed.json"))
            {
                dynamic model = JsonConvert.DeserializeObject(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//.config/legendary/installed.json"));
                return model["Fortnite"]["install_path"] + "\\FortniteGame\\Content\\Paks";
            }

            return string.Empty;
        }

        public static string GetGameVersion()
        {
            if (File.Exists("C:\\ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat"))
            {
                FortniteInstallationModel? model = JsonSerializer.Deserialize<FortniteInstallationModel>(File.ReadAllText("C:\\ProgramData\\Epic\\UnrealEngineLauncher\\LauncherInstalled.dat"));
                if (model == null)
                    return string.Empty;
                if (model.InstallationList.Any(x => x.AppName == "Fortnite"))
                    return model.InstallationList.First(x => x.AppName == "Fortnite").AppVersion;
            }

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//.config/legendary/installed.json"))
            {
                dynamic model = JsonConvert.DeserializeObject(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "//.config/legendary/installed.json"));
                return model["Fortnite"]["version"];
            }

            return string.Empty;
        }
    }
}
