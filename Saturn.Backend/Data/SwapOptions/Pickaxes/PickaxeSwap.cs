using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Utils.Swaps;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Pickaxes;

internal abstract class PickaxeSwap : AbstractSwap
{
    protected PickaxeSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum)
        : base(name, rarity, icon)
    {
        Swaps = swaps;
    }

    public Dictionary<string, string> Swaps { get; }
}
