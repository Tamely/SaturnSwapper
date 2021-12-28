using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saturn.Data.Models.Epic_Games
{
    public class InstallationList
    {
        [JsonProperty("InstallLocation")]
        public string InstallLocation { get; set; }

        [JsonProperty("NamespaceId")]
        public string NamespaceId { get; set; }

        [JsonProperty("ItemId")]
        public string ItemId { get; set; }

        [JsonProperty("ArtifactId")]
        public string ArtifactId { get; set; }

        [JsonProperty("AppVersion")]
        public string AppVersion { get; set; }

        [JsonProperty("AppName")]
        public string AppName { get; set; }
    }

    public class InstalledApps
    {
        [JsonProperty("InstallationList")]
        public List<InstallationList> InstallationList { get; set; }
    }


}
