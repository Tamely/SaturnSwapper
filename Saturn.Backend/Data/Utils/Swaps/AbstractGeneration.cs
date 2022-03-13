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
    /// <summary>
    /// Writes the cache of each item to a json file depending on the item type.
    /// </summary>
    /// <param name="items">The list of items it will write.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if you try to write a type you're not supposed to.</exception>
    public void WriteItems(List<Cosmetic> items)
    {
        switch (ItemType) // Changes what will happen depending on the item type
        {
            case ItemType.IT_Skin: // If the item type is a skin
                File.WriteAllText(Config.SkinsCache, JsonConvert.SerializeObject(items, Formatting.Indented)); // Write the items list to the skin cache file.
                break; // Stop what you were doing.
            case ItemType.IT_Backbling: // If the item type is a backbling
                File.WriteAllText(Config.BackblingCache, JsonConvert.SerializeObject(items, Formatting.Indented)); // Write the items list to the skin cache file.
                break; // Stop what you were doing.
            case ItemType.IT_Pickaxe: // If the item type is a pickaxe
                File.WriteAllText(Config.PickaxeCache, JsonConvert.SerializeObject(items, Formatting.Indented)); // Write the items list to the skin cache file.
                break; // Stop what you were doing.
            case ItemType.IT_Dance: // If the item type is a dance
                File.WriteAllText(Config.EmoteCache, JsonConvert.SerializeObject(items, Formatting.Indented)); // Write the items list to the skin cache file.
                break; // Stop what you were doing.
            case ItemType.IT_Misc:  // If the item type is misc
            default: // Or if the item type is not specified
                Logger.Log("Cannot generate swaps for this... I have no idea how you got here...", LogLevel.Fatal); // Log a fatal error.
                throw new ArgumentOutOfRangeException(); // Throw an exception.
        }
    }

    /// <summary>
    /// Sets the item type the class will use and creates the base configuration directory.
    /// </summary>
    /// <param name="itemType">Which item type are we using?</param>
    protected AbstractGeneration(ItemType itemType)
    {
        Directory.CreateDirectory(Config.CachePath); // Create the base directory.
        ItemType = itemType; // Set the item type.
    }

    /// <summary>
    /// Base method for generating swaps.
    /// </summary>
    /// <returns>List of Cosmetic: The generated swaps</returns>
    public virtual Task<List<Cosmetic>> Generate()
    {
        return Task.FromResult(new List<Cosmetic>());
    }

    private ItemType ItemType { get; set; }
}