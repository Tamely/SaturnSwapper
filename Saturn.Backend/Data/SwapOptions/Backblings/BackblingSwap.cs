using Saturn.Backend.Data.Utils.Swaps;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal abstract class BackblingSwap : AbstractSwap
{
    protected BackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon)
    {
        Data = data;
    }

    public Dictionary<string, string> Data { get; }
}
