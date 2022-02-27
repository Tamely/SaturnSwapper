using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.FortniteAPI;

namespace Saturn.Backend.Data.Utils.Swaps;

internal class AbstractGeneration
{
    public void WriteItems(List<Cosmetic> items)
    {
        switch (ItemType)
        {
            case ItemType.IT_Skin:
                File.WriteAllText(Config.SkinsCache, JsonConvert.SerializeObject(items, Formatting.Indented));
                break;
            case ItemType.IT_Backbling:
                break;
            case ItemType.IT_Pickaxe:
                break;
            case ItemType.IT_Dance:
                break;
            case ItemType.IT_Misc:
                Logger.Log("Cannot generate swaps for misc... I have no idea how you got here...", LogLevel.Fatal);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected AbstractGeneration(ItemType itemType)
    {
        Directory.CreateDirectory(Config.CachePath);
        ItemType = itemType;
    }

    public virtual Task<List<Cosmetic>> Generate()
    {
        return Task.FromResult(new List<Cosmetic>());
    }

    private ItemType ItemType { get; set; }
}