using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

internal abstract class SkinSwap : AbstractSwap
{
    protected SkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon)
    {
        SwapModel = swapModel;
    }

    public MeshDefaultModel SwapModel { get; }
}

internal abstract class EmoteSwap : AbstractSwap
{
    protected EmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps) 
        : base(name, rarity, icon)
    {
        Swaps = swaps;
    }

    public Dictionary<string, string> Swaps { get; }
}

internal abstract class PickaxeSwap : AbstractSwap
{
    protected PickaxeSwap(string name, string rarity, string icon, Dictionary<string, string> swaps, EFortRarity rarityEnum) 
        : base(name, rarity, icon)
    {
        Swaps = swaps;
    }

    public Dictionary<string, string> Swaps { get; }
}

internal abstract class BackblingSwap : AbstractSwap
{
    protected BackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data) 
        : base(name, rarity, icon)
    {
        Data = data;
    }

    public Dictionary<string, string> Data { get; }
}

#region Emotes
internal sealed class DanceMovesEmoteSwap : EmoteSwap
{
    public DanceMovesEmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps) 
        : base(name, rarity, icon, swaps)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Cosmetics/Dances/EID_DanceMoves",
                Swaps = new List<SaturnSwap>
                {
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Montages/Emotes/Emote_DanceMoves.Emote_DanceMoves",
                        Replace = Swaps["CMM"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Dance.T-Icon-Emotes-E-Dance",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.Modifier
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-Dance-L.T-Icon-Emotes-E-Dance-L",
                        Replace = "/",
                        Type = SwapType.Modifier
                    }
                }
            }
        };
}

internal sealed class BoogieDownEmoteSwap : EmoteSwap
{
    public BoogieDownEmoteSwap(string name, string rarity, string icon, Dictionary<string, string> swaps) 
        : base(name, rarity, icon, swaps)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset
            {
                ParentAsset = "FortniteGame/Content/Athena/Items/Cosmetics/Dances/EID_BoogieDown",
                Swaps = new List<SaturnSwap>
                {
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Boogie_Down/Emote_Boogie_Down_CMM.Emote_Boogie_Down_CMM",
                        Replace = Swaps["CMM"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/Animation/Game/MainPlayer/Emotes/Boogie_Down/Emote_Boogie_Down_CMF.Emote_Boogie_Down_CMF",
                        Replace = Swaps["CMF"],
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-BoogieDown.T-Icon-Emotes-E-BoogieDown",
                        Replace = Swaps["SmallIcon"],
                        Type = SwapType.Modifier
                    },
                    new()
                    {
                        Search = "/Game/UI/Foundation/Textures/Icons/Emotes/T-Icon-Emotes-E-BoogieDown-L.T-Icon-Emotes-E-BoogieDown-L",
                        Replace = "/",
                        Type = SwapType.Modifier
                    }
                }
            }
        };
}
#endregion

#region Backbling
internal sealed class BlackoutBagBackblingSwap : BackblingSwap
{
    public BlackoutBagBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data) 
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_StreetFashionEclipse",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_Pack.F_MED_Street_Fashion_Red_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/F_MED_Street_Fashion_Red/Skins/Eclipse/Materials/F_MED_StreetFashionEclipse_Backpack.F_MED_StreetFashionEclipse_Backpack",
                        Replace = Data["Material"],
                        Type = SwapType.BackblingMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_Pack_AnimBp.F_MED_Street_Fashion_Red_Pack_AnimBp_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_Pack_AnimBp.F_MED_Street_Fashion_Red_Pack_AnimBp_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}

internal sealed class ThorsCloakBackblingSwap : BackblingSwap
{
    public ThorsCloakBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data) 
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_HightowerTapas",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tapas/Meshes/M_MED_Tapas_Pack.M_MED_Tapas_Pack",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tapas/Meshes/M_MED_Tapas_Pack_AnimBP.M_MED_Tapas_Pack_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/Backpack_M_MED_Tapas/Meshes/M_MED_Tapas_Pack_AnimBP.M_MED_Tapas_Pack_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Cosmetics/Blueprints/Part_Modifiers/B_Athena_PartModifier_Backpack_Hightower_Tapas.B_Athena_PartModifier_Backpack_Hightower_Tapas_C",
                        Replace = Data["PartModifierBP"],
                        Type = SwapType.Modifier
                    },
                }
            }
        };
}

internal sealed class WrappingCaperBackblingSwap : BackblingSwap
{
    public WrappingCaperBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data) 
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_CardboardCrewHolidayMale",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/M_MED_Cardboard_Crew_Holiday_Cape/Meshes/M_MED_Cardboard_Crew_Holiday_Cape.M_MED_Cardboard_Crew_Holiday_Cape",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Capes/M_MED_Cardboard_Crew_Holiday_Cape/Meshes/M_MED_Cardboard_Crew_Holiday_Cape_AnimBP.M_MED_Cardboard_Crew_Holiday_Cape_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Capes/M_MED_Cardboard_Crew_Holiday_Cape/Meshes/M_MED_Cardboard_Crew_Holiday_Cape_AnimBP.M_MED_Cardboard_Crew_Holiday_Cape_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}

internal sealed class TheSithBackblingSwap : BackblingSwap
{
    public TheSithBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data) 
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_GalileoSpeedBoat",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/M_MED_Celestial_Backpack/M_MED_Celestial.M_MED_Celestial",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_Galileo_Holos/FX/P_Backpack_GalileoSpeedboat_Holo.P_Backpack_GalileoSpeedboat_Holo",
                        Replace = Data["FX"],
                        Type = SwapType.BackblingFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Cosmetics/Blueprints/B_Athena_PartModifier_Generic.B_Athena_PartModifier_Generic_C",
                        Replace = Data["PartModifierBP"],
                        Type = SwapType.BackblingPartBP
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Mesh/Male_Commando_Graffiti_Skeleton_AnimBP.Male_Commando_Graffiti_Skeleton_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/Mesh/Male_Commando_Graffiti_Skeleton_AnimBP.Male_Commando_Graffiti_Skeleton_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}
#endregion
