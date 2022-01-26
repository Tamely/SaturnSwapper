using Newtonsoft.Json;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils;
using System.Collections.Generic;

namespace Saturn.Backend.Data.Models
{
    public class Configuration
    {
        public Configuration()
        {
            InstallLocation = FortniteUtil.GetFortnitePath();
            ConvertedItems = new List<ConvertedItem>();
        }

        [JsonProperty("installLocation")] public string InstallLocation { get; set; }
        [JsonProperty("fortniteBuild")] public string FortniteBuild { get; set; }
        [JsonProperty("convertedItems")] public List<ConvertedItem> ConvertedItems { get; set; }
        [JsonProperty("shouldDebugShow")] public bool ShouldDebugShow { get; set; } = true;
        [JsonProperty("shouldPickaxeSwapRarity")] public bool ShouldPickaxeSwapRarity { get; set; } = true;
        [JsonProperty("shouldPickaxeSwapSeries")] public bool ShouldPickaxeSwapSeries { get; set; } = true;
        [JsonProperty("shouldShowStyles")] public bool ShouldShowStyles { get; set; } = true;
        [JsonProperty("isLobbyBackgroundConverted")] public bool IsLobbyBackgroundConverted { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
    }
}