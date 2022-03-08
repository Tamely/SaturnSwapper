using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class KrisabelleSkinSwap : SkinSwap
{
    public KrisabelleSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_ScholarFestive",
                Swaps = new List<SaturnSwap>()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Festive/Materials/F_MED_Scholar_Festive_Body.F_MED_Scholar_Festive_Body",
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
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/F_MED_Scholar_AnimBP.F_MED_Scholar_AnimBP_C",
                        Type = SwapType.BodyAnim
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_ScholarFestive",
                Swaps = new List<SaturnSwap>()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Festive/Materials/F_MED_Scholar_Festive_Head.F_MED_Scholar_Festive_Head",
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
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_CAU_Jane_Head_01/Meshes/F_MED_CAU_Jane_Head_01_AnimBP_Child.F_MED_CAU_Jane_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_ScholarFestive_FaceAcc",
                Swaps = new List<SaturnSwap>()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Festive/Materials/F_MED_Scholar_Festive_Hair.F_MED_Scholar_Festive_Hair",
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
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Skins/Festive/Materials/F_MED_Scholar_Festive_FaceAcc.F_MED_Scholar_Festive_FaceAcc",
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
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Scholar/Meshes/Parts/F_MED_Scholar_AnimBP.F_MED_Scholar_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    }
                }
            }
        };
}