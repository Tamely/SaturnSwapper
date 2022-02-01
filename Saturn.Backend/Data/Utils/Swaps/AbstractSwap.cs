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

#region Skins
internal sealed class DefaultSkinSwap : SkinSwap
{
    private readonly Dictionary<string, string> _cps;
    private readonly string _headOrHat;

    public DefaultSkinSwap(string name, string rarity, string icon, Dictionary<string, string> cps, string headOrHat) 
        : base(name, rarity, icon, new MeshDefaultModel())
    {
        _cps = cps;
        _headOrHat = headOrHat;
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Balance/DefaultGameDataCosmetics",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Heroes/Mesh/Wslt/Will/Skid/This/From/Tamely/Because/He/Always/Does/BodyCharacterPartWithExtraLongLength.BodyCharacterPartWithExtraLongLengthTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyTamelyW",
                        Replace = _cps["Body"],
                        Type = SwapType.BodyCharacterPart
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/CharacterParts/Hopefully/Wslt/Doesnt/Skid/This/From/Me/Like/He/Usually/Does/Because/That/Would/Just/Prove/So/Much/Like/When/He/Said/That/I/Dont/Own/Uassets.ICreateMyOwn.ICreateMyOwnThoughL",
                        Replace = _cps[_headOrHat],
                        Type = SwapType.HeadCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_M_Prime",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_M_Prime",
                        Replace = "CP_Athena_Body_M_Pr1me",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_M_Prime_G",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_M_Prime_G",
                        Replace = "CP_Athena_Body_M_Pr1me_G",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_A",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_A",
                        Replace = "CP_Athena_Body_F_Pr1me_A",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_B",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_B",
                        Replace = "CP_Athena_Body_F_Pr1me_B",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_C",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_C",
                        Replace = "CP_Athena_Body_F_Pr1me_C",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime",
                        Replace = "CP_Athena_Body_F_Pr1me",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_E",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_E",
                        Replace = "CP_Athena_Body_F_Pr1me_E",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_Prime_G",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search = "CP_Athena_Body_F_Prime_G",
                        Replace = "CP_Athena_Body_F_Pr1me_G",
                        Type = SwapType.BodyCharacterPart
                    }
                }
            }
        };
}

internal sealed class RedlineSkinSwap : SkinSwap
{
    public RedlineSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_StreetRacer",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/Female_Commando_StreetRacerBlack/Materials/F_MED___StreetRacerBlack.F_MED___StreetRacerBlack",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_StreetRacer",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Skin/F_Med_HIS_StreetRacerBlack.F_Med_HIS_StreetRacerBlack",
                        Replace = SwapModel.HeadSkinColor,
                        Type = SwapType.SkinTone
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                        Replace = SwapModel.HeadHairColor,
                        Type = SwapType.HairColor
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01.F_MED_HIS_Ramirez_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/Female_Commando_StreetRacerBlack/Materials/F_MED_StreetRacerBlack_Head_01.F_MED_StreetRacerBlack_Head_01",
                        Replace = SwapModel.HeadMaterials[1],
                        Type = SwapType.HeadMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/CP_Hat_F_Commando_StreetRacer",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.FaceACCABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Accessories/Hats/Mesh/Female_Outlander_06.Female_Outlander_06",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Accessories/Hats/Materials/Hat_F_StreetRacerBlack.Hat_F_StreetRacerBlack",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 4, 4, 3, 2, 3 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 4, 4, 3, (byte)SwapModel.HatType, 3 }),
                        Type = SwapType.Property
                    }
                }
            }
        };
}

internal sealed class FrozenNogOpsSkinSwap : SkinSwap
{
    public FrozenNogOpsSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_UglySweaterFrozen",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                                Replace = SwapModel.HeadHairColor,
                                Type = SwapType.HairColor
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                                Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                                Type = SwapType.HeadAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01.F_MED_ASN_Sarah_Head_01",
                                Replace = SwapModel.HeadMesh,
                                Type = SwapType.HeadMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/UglySweater_Frozen/Materials/F_M_UglySweater_Frozen_Head.F_M_UglySweater_Frozen_Head",
                                Replace = SwapModel.HeadMaterials[0],
                                Type = SwapType.HeadMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Materials/F_MED_ASN_Sarah_Hair_Hide.F_MED_ASN_Sarah_Hair_Hide",
                                Replace = SwapModel.HeadMaterials[1],
                                Type = SwapType.HairMaterial
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_UglySweater",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                                Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                                Type = SwapType.BodyAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
                                Replace = SwapModel.BodyMesh,
                                Type = SwapType.BodyMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                                Replace = SwapModel.BodySkeleton,
                                Type = SwapType.BodySkeleton
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/UglySweater_Frozen/Materials/F_M_UglySweater_Frozen_Body.F_M_UglySweater_Frozen_Body",
                                Replace = SwapModel.BodyMaterials[0],
                                Type = SwapType.BodyMaterial
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/CP_Hat_F_Commando_UglySweaterFrozen",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Accessories/Hats/F_MED_HolidayPJs_FaceAcc/Meshes/F_MED_Holiday_PJs_1_FaceAcc_AnimBP.F_MED_Holiday_PJs_1_FaceAcc_AnimBP_C",
                                Replace = SwapModel.FaceACCABP ?? "/Game/Accessories/Hats/F_MED_HolidayPJs_FaceAcc/Meshes/F_MED_Holiday_PJs_1_FaceAcc_AnimBP.F_MED_Holiday_PJs_1_FaceAcc_AnimBP_C",
                                Type = SwapType.FaceAccessoryAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Accessories/Hats/F_MED_HolidayPJs_FaceAcc/Meshes/F_MED_Holiday_PJs_1_FaceAcc.F_MED_Holiday_PJs_1_FaceAcc",
                                Replace = SwapModel.FaceACCMesh,
                                Type = SwapType.FaceAccessoryMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Accessories/Hats/F_MED_HolidayPJs_FaceAcc/Skins/UglySweater_Frozen/Materials/MI_F_MED_UglySweater_Frozen_FaceAcc.MI_F_MED_UglySweater_Frozen_FaceAcc",
                                Replace = SwapModel.FaceACCMaterials[0],
                                Type = SwapType.FaceAccessoryMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/UglySweater_Frozen/Materials/F_M_UglySweater_Frozen_Hair.F_M_UglySweater_Frozen_Hair",
                                Replace = SwapModel.FaceACCMaterials[1],
                                Type = SwapType.FaceAccessoryMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {4, 4, 3, 2, 4}),
                                Replace = System.Convert.ToBase64String(new byte[] {4,4,3,(byte)SwapModel.HatType,4}),
                                Type = SwapType.Property
                            }
                        }
            }
        };
}

internal sealed class BlazeSkinSwap : SkinSwap
{
    public BlazeSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RenegadeRaiderFire",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                                Replace = SwapModel.HeadHairColor,
                                Type = SwapType.HairColor
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                                Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                                Type = SwapType.HeadAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01.F_MED_ASN_Sarah_Head_01",
                                Replace = SwapModel.HeadMesh,
                                Type = SwapType.HeadMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Materials/MI_F_MED_Renegade_Raider_Fire_Head.MI_F_MED_Renegade_Raider_Fire_Head",
                                Replace = SwapModel.HeadMaterials[0],
                                Type = SwapType.HeadMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Materials/MI_F_MED_Renegade_Raider_Fire_Hair.MI_F_MED_Renegade_Raider_Fire_Hair",
                                Replace = SwapModel.HeadMaterials[1],
                                Type = SwapType.HairMaterial
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_RenegadeRaiderFire",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                                Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                                Type = SwapType.BodyAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Athena/Cosmetics/Blueprints/Part_Modifiers/B_Athena_PartModifier_RenegadeRaider_Fire.B_Athena_PartModifier_RenegadeRaider_Fire_C",
                                Replace = SwapModel.BodyPartModifierBP,
                                Type = SwapType.Modifier
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
                                Replace = SwapModel.BodyMesh,
                                Type = SwapType.BodyMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                                Replace = SwapModel.BodySkeleton,
                                Type = SwapType.BodySkeleton
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Materials/MI_F_MED_Renegade_Raider_Fire_Body.MI_F_MED_Renegade_Raider_Fire_Body",
                                Replace = SwapModel.BodyMaterials[0],
                                Type = SwapType.BodyMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Effects/Fort_Effects/Effects/Characters/Athena_Parts/RenegadeRaider_Fire/NS_RenegadeRaider_Fire.NS_RenegadeRaider_Fire",
                                Replace = SwapModel.BodyFX,
                                Type = SwapType.BodyFx
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/CP_Hat_F_Commando_RenegadeRaiderFire",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday_AnimBP.F_MED_Renegade_Raider_Holiday_AnimBP_C",
                                Replace = SwapModel.FaceACCABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday_AnimBP.F_MED_Renegade_Raider_Holiday_AnimBP_C",
                                Type = SwapType.FaceAccessoryAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday.F_MED_Renegade_Raider_Holiday",
                                Replace = SwapModel.FaceACCMesh,
                                Type = SwapType.FaceAccessoryMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Materials/MI_F_MED_Renegade_Raider_Fire_FaceAcc.MI_F_MED_Renegade_Raider_Fire_FaceAcc",
                                Replace = SwapModel.FaceACCMaterials[0],
                                Type = SwapType.FaceAccessoryMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {4,4,3,2,3}),
                                Replace = System.Convert.ToBase64String(new byte[] {4,4,3,(byte)SwapModel.HatType,3}),
                                Type = SwapType.Property
                            }
                        }
            }
        };
}

internal sealed class GingerbreadRaiderSkinSwap : SkinSwap
{
    public GingerbreadRaiderSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RenegadeRaiderHoliday",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                                Replace = SwapModel.HeadHairColor,
                                Type = SwapType.HairColor
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                                Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                                Type = SwapType.HeadAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01.F_MED_ASN_Sarah_Head_01",
                                Replace = SwapModel.HeadMesh,
                                Type = SwapType.HeadMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Materials/F_MED_ASN_Sarah_Head_02.F_MED_ASN_Sarah_Head_02",
                                Replace = SwapModel.HeadMaterials[0],
                                Type = SwapType.HeadMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Materials/MI_F_MED_Renegade_Raider_Fire_Hair.MI_F_MED_Renegade_Raider_Fire_Hair",
                                Replace = SwapModel.HeadMaterials[1],
                                Type = SwapType.HairMaterial
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_RenegadeRaiderHoliday",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                                Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                                Type = SwapType.BodyAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
                                Replace = SwapModel.BodyMesh,
                                Type = SwapType.BodyMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                                Replace = SwapModel.BodySkeleton,
                                Type = SwapType.BodySkeleton
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Materials/M_F_Renegade_Raider_Holiday_Body.M_F_Renegade_Raider_Holiday_Body",
                                Replace = SwapModel.BodyMaterials[0],
                                Type = SwapType.BodyMaterial
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/CP_Hat_F_Commando_RenegadeRaiderHoliday",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday_AnimBP.F_MED_Renegade_Raider_Holiday_AnimBP_C",
                                Replace = SwapModel.FaceACCABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday_AnimBP.F_MED_Renegade_Raider_Holiday_AnimBP_C",
                                Type = SwapType.FaceAccessoryAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday.F_MED_Renegade_Raider_Holiday",
                                Replace = SwapModel.FaceACCMesh,
                                Type = SwapType.FaceAccessoryMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Materials/M_F_Renegade_Raider_Holiday_FaceAcc.M_F_Renegade_Raider_Holiday_FaceAcc",
                                Replace = SwapModel.FaceACCMaterials[0],
                                Type = SwapType.FaceAccessoryMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {4,4,3,2,3}),
                                Replace = System.Convert.ToBase64String(new byte[] {4,4,3,(byte)SwapModel.HatType,3}),
                                Type = SwapType.Property
                            }
                        }
            }
        };
}

internal sealed class PermafrostRaiderSkinSwap : SkinSwap
{
    public PermafrostRaiderSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RenegadeRaiderIce",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                                Replace = SwapModel.HeadHairColor,
                                Type = SwapType.HairColor
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                                Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                                Type = SwapType.HeadAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01.F_MED_ASN_Sarah_Head_01",
                                Replace = SwapModel.HeadMesh,
                                Type = SwapType.HeadMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Skins/Ice/Materials/F_MED_Renegade_Raider_Ice_Head.F_MED_Renegade_Raider_Ice_Head",
                                Replace = SwapModel.HeadMaterials[0],
                                Type = SwapType.HeadMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Skins/Ice/Materials/F_MED_Renegade_Raider_Ice_Hair.F_MED_Renegade_Raider_Ice_Hair",
                                Replace = SwapModel.HeadMaterials[1],
                                Type = SwapType.HairMaterial
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_RenegadeRaiderIce",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                                Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                                Type = SwapType.BodyAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
                                Replace = SwapModel.BodyMesh,
                                Type = SwapType.BodyMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                                Replace = SwapModel.BodySkeleton,
                                Type = SwapType.BodySkeleton
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Skins/Ice/Materials/F_MED_Renegade_Raider_Ice_Body.F_MED_Renegade_Raider_Ice_Body",
                                Replace = SwapModel.BodyMaterials[0],
                                Type = SwapType.BodyMaterial
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/CP_Hat_F_Commando_RenegadeRaiderIce",
                Swaps = new List<SaturnSwap>()
                        {
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday_AnimBP.F_MED_Renegade_Raider_Holiday_AnimBP_C",
                                Replace = SwapModel.FaceACCABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday_AnimBP.F_MED_Renegade_Raider_Holiday_AnimBP_C",
                                Type = SwapType.FaceAccessoryAnim
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday.F_MED_Renegade_Raider_Holiday",
                                Replace = SwapModel.FaceACCMesh,
                                Type = SwapType.FaceAccessoryMesh
                            },
                            new SaturnSwap()
                            {
                                Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Skins/Ice/Materials/F_MED_Renegade_Raider_Ice_FaceAcc.F_MED_Renegade_Raider_Ice_FaceAcc",
                                Replace = SwapModel.FaceACCMaterials[0],
                                Type = SwapType.FaceAccessoryMaterial
                            },
                            new SaturnSwap()
                            {
                                Search = System.Convert.ToBase64String(new byte[] {4,4,3,2,3}),
                                Replace = System.Convert.ToBase64String(new byte[] {4,4,3,(byte)SwapModel.HatType,3}),
                                Type = SwapType.Property
                            }
                        }
            }
        };
}

internal sealed class DiamondDivaSkinSwap : SkinSwap
{
    public DiamondDivaSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_RaiderSilver",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/Parts/F_MED_Raider_Pink_FaceAcc_AnimBP.F_MED_Raider_Pink_FaceAcc_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/Parts/F_MED_Raider_Pink_FaceAcc_AnimBP.F_MED_Raider_Pink_FaceAcc_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/Parts/F_MED_Raider_Pink_FaceAcc.F_MED_Raider_Pink_FaceAcc",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Skins/Silver/Materials/F_MED_Raider_Silver_Face_Acc.F_MED_Raider_Silver_Face_Acc",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Skins/Silver/Materials/F_MED_Raider_Silver_Hair.F_MED_Raider_Silver_Hair",
                        Replace = SwapModel.FaceACCMaterials[1],
                        Type = SwapType.FaceAccessoryMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RaiderSilver",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_Ice_Queen_Head/Meshes/F_MED_IceQueen_Head_Child_AnimBP.F_MED_IceQueen_Head_Child_AnimBP_C",
                        Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_Ice_Queen_Head/Meshes/F_MED_IceQueen_Head_Child_AnimBP.F_MED_IceQueen_Head_Child_AnimBP_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_Ice_Queen_Head/Meshes/F_MED_Ice_Queen_Head.F_MED_Ice_Queen_Head",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_Ice_Queen_Head/Skins/Raider_Silver/Materials/F_MED_Raider_Silver_Head.F_MED_Raider_Silver_Head",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HeadMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                        Replace = SwapModel.HeadHairColor,
                        Type = SwapType.HairColor
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_RaiderSilver",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/F_MED_Raider_Pink_AnimBP.F_MED_Raider_Pink_AnimBP_C",
                        Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/F_MED_Raider_Pink_AnimBP.F_MED_Raider_Pink_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/F_MED_Raider_Pink.F_MED_Raider_Pink",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Skins/Silver/Materials/F_MED_Raider_Silver_Body.F_MED_Raider_Silver_Body",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    }
                }
            }
        };
}

internal sealed class RubyShadowsSkinSwap : SkinSwap
{
    public RubyShadowsSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_StreetFashionEclipse",
                Swaps = new()
                {
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red.F_MED_Street_Fashion_Red",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_AnimBP.F_MED_Street_Fashion_Red_AnimBP_C",
                        Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_AnimBP.F_MED_Street_Fashion_Red_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Skins/Eclipse/Materials/F_MED_StreetFashionEclipse_Body.F_MED_StreetFashionEclipse_Body",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    },
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    },
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_StreetFashionEclipse",
                Swaps = new()
                {
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_Angel_Head_01/Meshes/F_MED_Angel_Head_AnimBP_Child.F_MED_Angel_Head_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_Angel_Head_01/Meshes/F_MED_Angel_Head_AnimBP_Child.F_MED_Angel_Head_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_Angel_Head_01/Meshes/F_MED_Angel_Head_01.F_MED_Angel_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Skins/Eclipse/Materials/F_MED_StreetFashionEclipse_Head.F_MED_StreetFashionEclipse_Head",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HeadMaterial
                    },
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_FaceAcc_StreetFashionEclipse",
                Swaps = new()
                {
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/Parts/F_MED_Street_Fashion_Red_FaceAcc_AnimBp.F_MED_Street_Fashion_Red_FaceAcc_AnimBp_C",
                        Replace = SwapModel.FaceACCABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/Parts/F_MED_Street_Fashion_Red_FaceAcc_AnimBp.F_MED_Street_Fashion_Red_FaceAcc_AnimBp_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/Parts/F_MED_Street_Fashion_Red_FaceAcc.F_MED_Street_Fashion_Red_FaceAcc",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Skins/Eclipse/Materials/F_MED_StreetFashionEclipse_Hair.F_MED_StreetFashionEclipse_Hair",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    }
                }
            }
        };
}

internal sealed class BlizzabelleSkinSwap : SkinSwap
{
    public BlizzabelleSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                            "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_ScholarFestiveWinter",
                Swaps = new List<SaturnSwap>()
                        {
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Winter/Materials/F_MED_Scholar_FestiveWinter_Body.F_MED_Scholar_FestiveWinter_Body",
                                Replace = SwapModel.BodyMaterials[0],
                                Type = SwapType.BodyMaterial
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                                Replace = SwapModel.BodySkeleton,
                                Type = SwapType.BodySkeleton
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/F_MED_Scholar.F_MED_Scholar",
                                Replace = SwapModel.BodyMesh,
                                Type = SwapType.BodyMesh
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/F_MED_Scholar_AnimBP.F_MED_Scholar_AnimBP_C",
                                Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/F_MED_Scholar_AnimBP.F_MED_Scholar_AnimBP_C",
                                Type = SwapType.BodyAnim
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset =
                            "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_ScholarFestiveWinter",
                Swaps = new List<SaturnSwap>()
                        {
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Winter/Materials/F_MED_Scholar_FestiveWinter_Head.F_MED_Scholar_FestiveWinter_Head",
                                Replace = SwapModel.HeadMaterials[0],
                                Type = SwapType.HeadMaterial
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Heads/F_MED_CAU_Jane_Head_01/Meshes/F_MED_CAU_Jane_Head_01.F_MED_CAU_Jane_Head_01",
                                Replace = SwapModel.HeadMesh,
                                Type = SwapType.BodyMesh
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Heads/F_MED_CAU_Jane_Head_01/Meshes/F_MED_CAU_Jane_Head_01_AnimBP_Child.F_MED_CAU_Jane_Head_01_AnimBP_Child_C",
                                Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_CAU_Jane_Head_01/Meshes/F_MED_CAU_Jane_Head_01_AnimBP_Child.F_MED_CAU_Jane_Head_01_AnimBP_Child_C",
                                Type = SwapType.HeadAnim
                            }
                        }
            },
            new SaturnAsset()
            {
                ParentAsset =
                            "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_ScholarFestiveWinter_FaceAcc",
                Swaps = new List<SaturnSwap>()
                        {
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Winter/Materials/F_MED_Scholar_FestiveWinter_Hair.F_MED_Scholar_FestiveWinter_Hair",
                                Replace = SwapModel.FaceACCMaterials[0],
                                Type = SwapType.FaceAccessoryMaterial
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Ghoul/Materials/F_MED_Scholar_Glass_Ghoul_FaceAcc.F_MED_Scholar_Glass_Ghoul_FaceAcc",
                                Replace = SwapModel.FaceACCMaterials[1],
                                Type = SwapType.FaceAccessoryMaterial
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Winter/Materials/F_MED_Scholar_FestiveWinter_FaceAcc.F_MED_Scholar_FestiveWinter_FaceAcc",
                                Replace = SwapModel.FaceACCMaterials[2],
                                Type = SwapType.FaceAccessoryMaterial
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/Parts/F_MED_Scholar.F_MED_Scholar",
                                Replace = SwapModel.FaceACCMesh,
                                Type = SwapType.FaceAccessoryMesh
                            },
                            new()
                            {
                                Search =
                                    "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/Parts/F_MED_Scholar_AnimBP.F_MED_Scholar_AnimBP_C",
                                Replace = SwapModel.FaceACCABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/Parts/F_MED_Scholar_AnimBP.F_MED_Scholar_AnimBP_C",
                                Type = SwapType.FaceAccessoryAnim
                            }
                        }
            }
        };
}

internal sealed class FrozenRedKnightSkinSwap : SkinSwap
{
    public FrozenRedKnightSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_RedKnightWinter",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/TV_32_RedKnight_Winter/Materials/M_F_MED_Commando_RedKnight_Winter.M_F_MED_Commando_RedKnight_Winter",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RedKnightWinter",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Skin/F_Med_HIS.F_Med_HIS",
                        Replace = SwapModel.HeadSkinColor,
                        Type = SwapType.SkinTone
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                        Replace = SwapModel.HeadHairColor,
                        Type = SwapType.HairColor
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01.F_MED_HIS_Ramirez_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_Med_Head_01/Materials/Chainmail/F_MED_Commando_RedKnight_Winter.F_MED_Commando_RedKnight_Winter",
                        Replace = SwapModel.HeadMaterials[1],
                        Type = SwapType.HeadMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_Med_Head_01/Materials/F_MED_Commando_No_Hair.F_MED_Commando_No_Hair",
                        Replace = SwapModel.HeadMaterials[0] ?? "/Game/Owen",
                        Type = SwapType.HairMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/Hat_F_Commando_RedKnightWinter",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Accessories/Hats/Mesh/Female_Commando_BR_BlackKnight_01.Female_Commando_BR_BlackKnight_01",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Accessories/Hats/Materials/Hat_Commando_RedKnight_Winter.Hat_Commando_RedKnight_Winter",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 0, 5, 2, 2, 0 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 0, 5, 2, (byte)SwapModel.HatType, 0 }),
                        Type = SwapType.Property
                    }
                }
            }
        };
}
#endregion

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
