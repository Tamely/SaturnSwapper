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
    /// Sets the item type the class will use and creates the base configuration directory.
    /// </summary>
    /// <param name="itemType">Which item type are we using?</param>
    protected AbstractGeneration(ItemType itemType)
    {
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