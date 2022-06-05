using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Skins;

internal sealed class TectonicKomplexSkinSwap : SkinSwap
{
    public TectonicKomplexSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_NeonGraffitiLava",
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
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_BLK_Red_Head_01/Mesh/F_MED_BLK_Red_Head_01_AnimBP_Child.F_MED_BLK_Red_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_BLK_Red_Head_01/Mesh/F_MED_BLK_Red_Head_01_AnimBP_Child.F_MED_BLK_Red_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_BLK_Red_Head_01/Mesh/F_MED_BLK_Red_Head_01.F_MED_BLK_Red_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_BLK_Red_Head_01/Skins/Neon_Graffiti_Lava/Materials/F_Neon_Graffiti_Lava_Head.F_Neon_Graffiti_Lava_Head",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HeadMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_Med_Head_01/Materials/F_MED_Commando_No_Hair.F_MED_Commando_No_Hair",
                        Replace = SwapModel.HeadMaterials[1],
                        Type = SwapType.HairMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_NeonGraffitiLava",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Neon_Graffiti/Meshes/F_MED_Neon_Graffiti_AnimBP.F_MED_Neon_Graffiti_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Neon_Graffiti/Meshes/F_MED_Neon_Graffiti_AnimBP.F_MED_Neon_Graffiti_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Neon_Graffiti/Meshes/F_MED_Neon_Graffiti.F_MED_Neon_Graffiti",
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
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Neon_Graffiti/Skin/Lava/Material/F_MED_Neon_Graffiti_Lava_Body.F_MED_Neon_Graffiti_Lava_Body",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_NeonGraffitiLava",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Neon_Graffiti/Meshes/Parts/F_MED_Neon_Graffiti_FaceAcc_AnimBP.F_MED_Neon_Graffiti_FaceAcc_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Neon_Graffiti/Meshes/Parts/F_MED_Neon_Graffiti_FaceAcc_AnimBP.F_MED_Neon_Graffiti_FaceAcc_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Neon_Graffiti/Meshes/Parts/F_MED_Neon_Graffiti_FaceAcc.F_MED_Neon_Graffiti_FaceAcc",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Neon_Graffiti/Skin/Lava/Material/F_MED_Neon_Graffiti_Lava_FaceAcc.F_MED_Neon_Graffiti_Lava_FaceAcc",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    }
                }
            }
        };
}