using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Skins;

internal sealed class NewDefaultSkinSwap : SkinSwap
{
    public NewDefaultSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_M_Prime_G",
                Swaps = new List<SaturnSwap>()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Prime/Skins/M_MED_Prime_BLK/Materials/M_MED_Prime_Body_BLK.M_MED_Prime_Body_BLK",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Base/SK_M_MALE_Base_Skeleton.SK_M_MALE_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Prime/Meshes/M_MED_Prime.M_MED_Prime",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Prime/Meshes/M_MED_Prime_AnimBP.M_MED_Prime_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Prime/Meshes/M_MED_Prime_AnimBP.M_MED_Prime_AnimBP_C",
                        Type = SwapType.BodyAnim
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Male/Medium/Heads/CP_Athena_Head_M_Prime_G",
                Swaps = new List<SaturnSwap>()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/CharacterColorSwatches/Hair/HairColor_02.HairColor_02",
                        Replace = SwapModel.HeadHairColor,
                        Type = SwapType.HairColor
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Heads/M_MED_Elastic_Head/Meshes/Male_Medium_Elastic_BLK_Head.Male_Medium_Elastic_BLK_Head",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Heads/M_MED_Elastic_Head/Meshes/Male_Medium_Elastic_BLK_Head_AnimBP.Male_Medium_Elastic_BLK_Head_AnimBP_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Male/Medium/Heads/M_MED_Elastic_Head/Meshes/Male_Medium_Elastic_BLK_Head_AnimBP.Male_Medium_Elastic_BLK_Head_AnimBP_C",
                        Type = SwapType.HeadAnim
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_M_MED_FaceAcc_Prime_G",
                Swaps = new List<SaturnSwap>()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Elastic/Skins/Prime/BLK/M_MED_Prime_BLK_Hair.M_MED_Prime_BLK_Hair",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Elastic/Meshes/Parts/BLK_Parts/M_MED_Elastic_BLK_1.M_MED_Elastic_BLK_1",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Elastic/Meshes/Parts/BLK_Parts/M_MED_Elastic_BLK_1_AnimBP.M_MED_Elastic_BLK_1_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Elastic/Meshes/Parts/BLK_Parts/M_MED_Elastic_BLK_1_AnimBP.M_MED_Elastic_BLK_1_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    }
                }
            }
        };
}