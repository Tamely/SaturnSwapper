using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.FortniteAPI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Saturn.Backend.Core.Utils.Swaps;

internal abstract class AbstractGeneration
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