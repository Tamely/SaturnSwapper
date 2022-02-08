using System.Collections.Generic;
using System.Linq;
using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace Saturn.Backend.Data.Models.Items
{
    public class GalaxyPlugin
    {
        [J("Name")] public string Name { get; set; }
        [J("Icon")] public string Icon { get; set; }
        [J("Swapicon")] public string swappedIcon { get; set; }
        [J("Message")] public string? Message { get; set; }
        [J("Type")] public string? Type { get; set; }
        [J("Assets")] public List<Asset> Assets { get; set; }
    }

    public class Asset
    {
        [J("CompressionType")] public string CompressionMethod { get; set; }
        [J("AssetPath")] public string Path { get; set; }
        [J("AssetUcas")] public string? Ucas { get; set; }
        [J("Swaps")] public List<Swap> Swaps { get; set; }
    }

    public class Swap
    {
        [J("type")] public string Type { get; set; } = "string";
        [J("search")] public string Search { get; set; }
        [J("replace")] public string Replace { get; set; }
    }
}
