using Newtonsoft.Json;
using Saturn.Data.Models.Items;
using Saturn.Data.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Models
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
    }
}
