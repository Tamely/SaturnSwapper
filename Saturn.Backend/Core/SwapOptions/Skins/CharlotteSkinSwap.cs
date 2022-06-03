using System.Collections.Generic;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;

namespace Saturn.Backend.Core.SwapOptions.Skins;

internal sealed class CharlotteSkinSwap: SkinSwap
{
    public CharlotteSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Athena_Head_F_PunkKoi",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_Punk_Koi_Head/Meshes/Female_Medium_Punk_Koi_Head_AnimBP.Female_Medium_Punk_Koi_Head_AnimBP_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_Punk_Koi_Head/Meshes/Female_Medium_Punk_Koi_Head_AnimBP.Female_Medium_Punk_Koi_Head_AnimBP_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_Punk_Koi_Head/Meshes/Female_Medium_Punk_Koi_Head.Female_Medium_Punk_Koi_Head",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_F_PunkKoi",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Punk_Koi/Meshes/F_MED_Punk_Koi_AnimBP.F_MED_Punk_Koi_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Punk_Koi/Meshes/F_MED_Punk_Koi_AnimBP.F_MED_Punk_Koi_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Punk_Koi/Meshes/F_MED_Punk_Koi.F_MED_Punk_Koi",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
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
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Punk_Koi/Meshes/Parts/F_MED_Punk_Koi_FaceAcc_AnimBP.F_MED_Punk_Koi_FaceAcc_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Punk_Koi/Meshes/Parts/F_MED_Punk_Koi_FaceAcc_AnimBP.F_MED_Punk_Koi_FaceAcc_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Punk_Koi/Meshes/Parts/F_MED_Punk_Koi_FaceAcc.F_MED_Punk_Koi_FaceAcc",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    }
                }
            }
        };
}