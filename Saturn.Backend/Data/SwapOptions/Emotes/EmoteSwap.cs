using Saturn.Backend.Data.Utils.Swaps;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Emotes;

internal abstract class EmoteSwap : AbstractSwap
{
    protected EmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps)
        : base(name, rarity, icon)
    {
        Swaps = swaps;
    }

    public Dictionary<string, string> Swaps { get; }
}
