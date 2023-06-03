using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saturn.Backend.Data.SaturnAPI.Models;

public class IndexModel
{
    public List<string> ProgramsReferencing { get; set; }
    public string APIVersion { get; set; }
    public long minsUp { get; set; }
    public string DiscordServer { get; set; }
    public string swapperVersion { get; set; }
    [JsonProperty("keyLink")] public string KeyLink { get; set; }
    [JsonProperty("mappingsLink")] public string MappingsLink { get; set; }
}