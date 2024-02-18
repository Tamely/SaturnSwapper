using Newtonsoft.Json;

namespace Saturn.Backend.Data.SaturnAPI.Models
{
    public struct KeySearchModel
    {
        [JsonProperty("hwid")] public string HWID { get; set; }
        [JsonProperty("found")] public bool Found { get; set; }
    }
}
