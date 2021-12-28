using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saturn.Backend.Data.Models.CloudStorage
{
    public class Changes
    {
        [JsonProperty("skinName")] public string SkinName { get; set; }
        [JsonProperty("customAssetURL")] public string CustomAssetUrl { get; set; }
        [JsonProperty("searches")] public List<string> Searches { get; set; }
        [JsonProperty("replaces")] public List<string> Replaces { get; set; }
        [JsonProperty("characterParts")] public List<string> CharacterParts { get; set; }
        [JsonProperty("hatSkins")] public List<string> HatSkins { get; set; }
    }
}