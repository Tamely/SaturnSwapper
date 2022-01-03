using System.Collections.Generic;
using Newtonsoft.Json;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils;

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
        [JsonProperty("convertedItems")] public List<ConvertedItem> ConvertedItems { get; set; }
        [JsonProperty("shouldDebugShow")] public bool ShouldDebugShow { get; set; } = true;
        [JsonProperty("shouldPickaxeSwapRarity")] public bool ShouldPickaxeSwapRarity { get; set; } = true;
    }
}