using Newtonsoft.Json;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.Models.CloudStorage
{
    public class Changes
    {
        [JsonProperty("addItem")] public bool addItem { get; set; }
        [JsonProperty("removeItem")] public bool removeItem { get; set; }
        [JsonProperty("removeOption")] public bool RemoveOption { get; set; }
        [JsonProperty("addOptions")] public bool addOptions { get; set; }
        [JsonProperty("swapOption")] public List<ItemInfo>? SwapOptions { get; set; }
        [JsonProperty("item")] public ItemInfo Item { get; set; }
        [JsonProperty("miscData")] public List<string> MiscData { get; set; }
        [JsonProperty("characterPartsReplace")] public List<string> CharacterPartsReplace { get; set; }
        [JsonProperty("removeOptions")] public List<string> RemoveOptions { get; set; }
    }

    public class ItemInfo
    {
        [JsonProperty("itemName")] public string ItemName { get; set; }
        [JsonProperty("itemDesc")] public string? ItemDescription { get; set; }
        [JsonProperty("itemIcon")] public string ItemIcon { get; set; }
        [JsonProperty("itemID")] public string ItemID { get; set; }
        [JsonProperty("itemType")] public ItemType ItemType { get; set; }
        [JsonProperty("rarity")] public Rarity Rarity { get; set; }
        [JsonProperty("series")] public Series? Series { get; set; }
        [JsonProperty("overrideAssets")] public List<SaturnAsset> OverrideAssets { get; set; }
    }
}