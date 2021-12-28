using System.Collections.Generic;
using Newtonsoft.Json;

namespace Saturn.Backend.Data.Models.Items
{
    public class ConvertedItem
    {
        public string ItemDefinition { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
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