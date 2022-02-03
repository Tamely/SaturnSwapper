using System.Collections.Generic;
using System.Threading.Tasks;
using Saturn.Backend.Data.Models.FortniteAPI;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils.Swaps;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal abstract class SkinSwap : AbstractSwap
{
    protected SkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon)
    {
        SwapModel = swapModel;
    }

    public MeshDefaultModel SwapModel { get; }
}

public static class AddSkins
{
    public static Cosmetic AddSkinOptions(this Cosmetic skin)
    {
        skin.CosmeticOptions = new List<SaturnItem>()
        {
            new SaturnItem
            {
                ItemDefinition = "CID_970_Athena_Commando_F_RenegadeRaiderHoliday",
                Name = "Gingerbread Raider",
                Description = "Let the festivities begin.",
                Icon =
                    "https://fortnite-api.com/images/cosmetics/br/cid_970_athena_commando_f_renegaderaiderholiday/smallicon.png",
                Rarity = "Rare"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_784_Athena_Commando_F_RenegadeRaiderFire",
                Name = "Blaze",
                Description = "Fill the world with flames.",
                Icon =
                    "https://fortnite-api.com/images/cosmetics/br/cid_784_athena_commando_f_renegaderaiderfire/smallicon.png",
                Rarity = "Legendary"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_A_322_Athena_Commando_F_RenegadeRaiderIce",
                Name = "Permafrost Raider",
                Description = "What could freeze a heart that burns?",
                Icon =
                    "https://fortnite-api.com/images/cosmetics/br/cid_a_322_athena_commando_f_renegaderaiderice/smallicon.png",
                Rarity = "Epic",
                Series = "FrozenSeries"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_653_Athena_Commando_F_UglySweaterFrozen",
                Name = "Frozen Nog Ops",
                Description = "Bring some chill to the skirmish.",
                Icon =
                    "https://fortnite-api.com/images/cosmetics/br/cid_653_athena_commando_f_uglysweaterfrozen/smallicon.png",
                Rarity = "Epic",
                Series = "FrozenSeries"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_A_311_Athena_Commando_F_ScholarFestiveWinter",
                Name = "Blizzabelle",
                Description = "Voted Teen Queen of Winterfest by a jury of her witchy peers.",
                Icon =
                    "https://fortnite-api.com/images/cosmetics/br/cid_a_311_athena_commando_f_scholarfestivewinter/smallicon.png",
                Rarity = "Rare"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_A_007_Athena_Commando_F_StreetFashionEclipse",
                Name = "Ruby Shadows",
                Description = "Sometimes you gotta go dark.",
                Icon =
                    "https://fortnite-api.com/images/cosmetics/br/cid_a_007_athena_commando_f_streetfashioneclipse/smallicon.png",
                Rarity = "Epic",
                Series = "ShadowSeries"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_936_Athena_Commando_F_RaiderSilver",
                Name = "Diamond Diva",
                Description = "Synthetic diamonds need not apply.",
                Icon =
                    "https://fortnite-api.com/images/cosmetics/br/cid_936_athena_commando_f_raidersilver/smallicon.png",
                Rarity = "Rare"
            },
            new SaturnItem
            {
                ItemDefinition = "CID_294_Athena_Commando_F_RedKnightWinter",
                Name = "Frozen Red Knight",
                Description = "Frozen menace of icy tundra.",
                Icon =
                    "https://fortnite-api.com/images/cosmetics/br/cid_294_athena_commando_f_redknightwinter/smallicon.png",
                Rarity = "Legendary",
                Series = "FrozenSeries"
            }
        };
        return skin;
    }
}
