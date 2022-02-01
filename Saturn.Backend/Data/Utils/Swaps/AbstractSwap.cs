using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.Utils.Swaps;

internal abstract class AbstractSwap
{
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

    public AbstractSwap(string name, string rarity, string icon, EFortRarity rarityEnum = EFortRarity.Common)
    {
        Name = name;
        Rarity = rarity;
        Icon = icon;
        this.rarityEnum = rarityEnum;
    }

    public virtual string Name { get; }

    public virtual string Rarity { get; }

    public virtual string Icon { get; }

    public abstract List<SaturnAsset> Assets { get; }
    public virtual EFortRarity rarityEnum { get; set; }
}
