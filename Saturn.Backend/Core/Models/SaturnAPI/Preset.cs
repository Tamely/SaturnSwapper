using System.Collections.Generic;
using Newtonsoft.Json;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.FortniteAPI;
using Saturn.Backend.Core.Models.Items;

namespace Saturn.Backend.Core.Models.SaturnAPI;

public sealed class Preset
{
    public string Name { get; set; }
    [JsonIgnore]
    public bool IsConverted
    {
        get
        {
            if (Items == null)
            {
                return false;
            }
            
            foreach (var item in Items)
            {
                if (!item.Item.IsConverted)
                {
                    return false;
                }
            }

            return true;
        }
    }
    public List<PresetItem> Items { get; set; }
}

public sealed class PresetItem
{
    public string ToName { get; set; }
    public string FromName { get; set; }
    public Cosmetic Item { get; set; }
    public SaturnItem Option { get; set; }
    public ItemType Type { get; set; }
    public bool IsDefault { get; set; }
}
