using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class MoltenValkyrieSkinSwap : SkinSwap
{
    public MoltenValkyrieSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_DarkViking_Fire",
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
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Starfish_Head_01/Meshes/F_MED_ASN_Starfish_Head_01_AnimBP_Child.F_MED_ASN_Starfish_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Starfish_Head_01/Meshes/F_MED_ASN_Starfish_Head_01_AnimBP_Child.F_MED_ASN_Starfish_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Starfish_Head_01/Meshes/F_MED_ASN_Starfish_Head_02.F_MED_ASN_Starfish_Head_02",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Skins/Fire/Materials/MI_F_MED_DarkViking_Fire_Head.MI_F_MED_DarkViking_Fire_Head",
                        Replace = SwapModel.HeadMaterials[1],
                        Type = SwapType.HeadMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Starfish_Head_01/Materials/F_MED_ASN_Starfish_Head_01_HairNone.F_MED_ASN_Starfish_Head_01_HairNone",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HairMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Athena/Cosmetics/Blueprints/B_Athena_PartModifier_Generic.B_Athena_PartModifier_Generic_C",
                        Replace = SwapModel.HeadPartModifierBP,
                        Type = SwapType.Modifier
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/FX/P_MED_Dark_Viking_Eyes_Light.P_MED_Dark_Viking_Eyes_Light",
                        Replace = SwapModel.HeadFX,
                        Type = SwapType.HeadFx
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_DarkViking_Fire",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Meshes/F_MED_Commando_DarkViking_AnimBP.F_MED_Commando_DarkViking_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Meshes/F_MED_Commando_DarkViking_AnimBP.F_MED_Commando_DarkViking_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Cosmetics/Blueprints/Part_Modifiers/B_Athena_PartModifier_DarkViking_Fem_Fire.B_Athena_PartModifier_DarkViking_Fem_Fire_C",
                        Replace = SwapModel.BodyPartModifierBP,
                        Type = SwapType.Modifier
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Meshes/F_MED_Commando_DarkViking.F_MED_Commando_DarkViking",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Skins/Fire/Materials/MI_F_MED_DarkViking_Fire_Body_v2.MI_F_MED_DarkViking_Fire_Body_v2",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Skins/Fire/Materials/MI_F_MED_DarkViking_Fire_Body_v2.MI_F_MED_DarkViking_Fire_Body_v2",
                        Replace = SwapModel.BodyMaterials[2],
                        Type = SwapType.BodyMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Effects/Fort_Effects/Effects/Characters/Athena_Parts/DarkVikingFire/P_DarkVikingFireFemale_Smokey.P_DarkVikingFireFemale_Smokey",
                        Replace = SwapModel.BodyFX,
                        Type = SwapType.BodyFx
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_DarkViking_Fire",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Meshes/F_MED_Commando_DarkViking_Head_01_AnimBP.F_MED_Commando_DarkViking_Head_01_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Meshes/F_MED_Commando_DarkViking_Head_01_AnimBP.F_MED_Commando_DarkViking_Head_01_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Meshes/F_MED_Commando_DarkViking_Head_01.F_MED_Commando_DarkViking_Head_01",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Skins/Fire/Materials/MI_F_MED_DarkViking_Fire_Hair.MI_F_MED_DarkViking_Fire_Hair",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Dark_Viking_01/Skins/Fire/Materials/MI_F_MED_DarkViking_Fire_Head.MI_F_MED_DarkViking_Fire_Head",
                        Replace = SwapModel.FaceACCMaterials[1],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 4, 4, 3, 2, 3 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 4, 4, 3, (byte)SwapModel.HatType, 3 }),
                        Type = SwapType.Property
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Athena/Cosmetics/Blueprints/Part_Modifiers/B_Athena_PartModifier_DarkViking_Fire_Female.B_Athena_PartModifier_DarkViking_Fire_Female_C",
                        Replace = SwapModel.FaceACCPartModifierBP,
                        Type = SwapType.Modifier
                    }
                }
            }
        };
}