using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.Utils.Swaps;

internal abstract class AbstractSwap
{
    public AbstractSwap(string name, string rarity, string icon, EFortRarity rarityEnum = EFortRarity.Common)
    {
        Name = name;
        Rarity = rarity;
        Icon = icon;
        this.RarityEnum = rarityEnum;
    }
    
    public virtual SaturnOption ToSaturnOption()
    {
        return new SaturnOption()
        {
            Name = Name,
            Rarity = Rarity,
            Icon = Icon,
            Assets = Assets
        };
    }

    public virtual string Name { get; }

    public virtual string Rarity { get; }

    public virtual string Icon { get; }

    public abstract List<SaturnAsset> Assets { get; }
    public virtual EFortRarity RarityEnum { get; set; }
}
