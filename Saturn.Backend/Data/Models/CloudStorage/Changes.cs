using System.Collections.Generic;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;

namespace Saturn.Backend.Data.Models.CloudStorage
{
    public class Changes
    {
        [JsonProperty("addItem")] public bool addItem { get; set; }
        [JsonProperty("removeItem")] public bool removeItem { get; set; }
        
        [JsonProperty("swapOption")] public ItemInfo SwapOption { get; set; }
        [JsonProperty("item")] public ItemInfo Item { get; set; }

        [JsonProperty("overrideAssets")] public List<Asset> OverrideAssets { get; set; }
        
        [JsonProperty("miscData")] public List<string> MiscData { get; set; }
    }

    public class ItemInfo
    {
        [JsonProperty("itemName")] public string ItemName { get; set; }
        [JsonProperty("itemDesc")] public string ItemDescription { get; set; }
        [JsonProperty("itemIcon")] public string ItemIcon { get; set; }
        [JsonProperty("itemID")] public string ItemID { get; set; }
        [JsonProperty("itemType")] public ItemType ItemType { get; set; }
        [JsonProperty("rarity")] public Rarity Rarity { get; set; }
        [JsonProperty("series")] public Series Series { get; set; }
    }

    public class Asset
    {
        [JsonProperty("parentAsset")] public string ParentAsset { get; set; }
        [JsonProperty("swaps")] public List<Swap> Swaps { get; set; }
    }

    public class Swap
    {
        [JsonProperty("search")] public string Search { get; set; }
        [JsonProperty("replace")] public string Replaces { get; set; }
        [JsonProperty("isBothB64")] public bool IsBothB64 { get; set; }
        [JsonProperty("isSearchB64")] public bool IsSearchB64 { get; set; }
    }
}