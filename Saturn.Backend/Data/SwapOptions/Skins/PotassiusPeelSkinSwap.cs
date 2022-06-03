using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class PotassiusPeelsSkinSwap : SkinSwap
{
    public PotassiusPeelsSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Athena_Head_M_BananaLeader",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Heads/M_MED_CAU_Jonesy_Head_01/Meshes/M_MED_CAU_Jonesy_Head_01_Export_Skeleton_AnimBP_Child.M_MED_CAU_Jonesy_Head_01_Export_Skeleton_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Male/Medium/Heads/M_MED_CAU_Jonesy_Head_01/Meshes/M_MED_CAU_Jonesy_Head_01_Export_Skeleton_AnimBP_Child.M_MED_CAU_Jonesy_Head_01_Export_Skeleton_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Heads/M_MED_Empty_Head_01/Meshes/M_MED_Empty_Head_01.M_MED_Empty_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Athena_Body_M_BananaLeader",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Banana_Leader/Meshes/M_MED_Banana_Leader_AnimBP.M_MED_Banana_Leader_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Banana_Leader/Meshes/M_MED_Banana_Leader_AnimBP.M_MED_Banana_Leader_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Banana_Leader/Meshes/M_MED_Banana_Leader.M_MED_Banana_Leader",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Banana_Leader/Meshes/M_MED_Banana_Leader_AnimBP.M_MED_Banana_Leader_AnimBP_C",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/CP_M_MED_BananaLeader_FaceAcc",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Banana_Leader/Meshes/Parts/M_MED_Banana_Leader_AnimBP.M_MED_Banana_Leader_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Banana_Leader/Meshes/Parts/M_MED_Banana_Leader_AnimBP.M_MED_Banana_Leader_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Male/Medium/Bodies/M_MED_Banana_Leader/Meshes/Parts/M_MED_Banana_Leader.M_MED_Banana_Leader",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    }
                }
            }
        };
}