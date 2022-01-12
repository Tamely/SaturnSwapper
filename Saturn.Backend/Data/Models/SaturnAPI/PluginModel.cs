using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saturn.Backend.Data.Models.SaturnAPI;

public class Swap
{
    [JsonProperty("search")]
    public string Search { get; set; }

    [JsonProperty("replace")]
    public string Replace { get; set; }
}

public class Asset
{
    [JsonProperty("AssetPath")]
    public string AssetPath { get; set; }

    [JsonProperty("Swaps")]
    public List<Swap> Swaps { get; set; }
}

public class PluginModel
{
    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Icon")]
    public string Icon { get; set; }

    [JsonProperty("SwapIcon")]
    public string SwapIcon { get; set; }

    [JsonProperty("Message")]
    public string Message { get; set; }

    [JsonProperty("Assets")]
    public List<Asset> Assets { get; set; }
}