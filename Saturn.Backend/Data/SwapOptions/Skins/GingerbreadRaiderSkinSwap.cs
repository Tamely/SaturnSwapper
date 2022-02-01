using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils.Swaps;

namespace Saturn.Backend.Data.SwapOptions.Skins;

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
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RenegadeRaiderHoliday",
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
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01_AnimBP_Child.F_MED_ASN_Sarah_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Meshes/F_MED_ASN_Sarah_Head_01.F_MED_ASN_Sarah_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Materials/F_MED_ASN_Sarah_Head_02.F_MED_ASN_Sarah_Head_02",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HeadMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Fire/Materials/MI_F_MED_Renegade_Raider_Fire_Hair.MI_F_MED_Renegade_Raider_Fire_Hair",
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
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
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
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Materials/M_F_Renegade_Raider_Holiday_Body.M_F_Renegade_Raider_Holiday_Body",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Hats/CP_Hat_F_Commando_RenegadeRaiderHoliday",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday_AnimBP.F_MED_Renegade_Raider_Holiday_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday_AnimBP.F_MED_Renegade_Raider_Holiday_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Meshes/Parts/F_MED_Renegade_Raider_Holiday.F_MED_Renegade_Raider_Holiday",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Renegade_Raider_Holiday/Materials/M_F_Renegade_Raider_Holiday_FaceAcc.M_F_Renegade_Raider_Holiday_FaceAcc",
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