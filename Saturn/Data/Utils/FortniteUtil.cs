using Newtonsoft.Json;
using Saturn.Data.Models.Epic_Games;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Utils
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
    }
}
