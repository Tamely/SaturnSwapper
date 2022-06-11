using Newtonsoft.Json;
using System.Collections.Generic;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.FortniteAPI;

namespace Saturn.Backend.Core.Models.Items
{
    public class ConvertedItem
    {
        public string ItemDefinition { get; set; }
        public string Name { get; set; }
        public string FromName { get; set; }
        public string Type { get; set; }
        public Cosmetic Item { get; set; }
        public SaturnItem Option { get; set; }
        public ItemType ItemType { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsRandom { get; set; } = false;
        public Cosmetic Random { get; set; } = null;
        public List<ActiveSwap> Swaps { get; set; }
    }

    public class ActiveSwap
    {
        public long Offset { get; set; }
        public string File { get; set; }
        public string ParentAsset { get; set; }
        public bool IsCompressed { get; set; }

        [JsonProperty("EXTData")] public Dictionary<long, byte[]> Lengths { get; set; }
    }
}