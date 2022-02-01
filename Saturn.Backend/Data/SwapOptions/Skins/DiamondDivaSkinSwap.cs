using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using Saturn.Backend.Data.Utils.Swaps;

namespace Saturn.Backend.Data.SwapOptions.Skins;

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
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/Parts/F_MED_Raider_Pink_FaceAcc_AnimBP.F_MED_Raider_Pink_FaceAcc_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/Parts/F_MED_Raider_Pink_FaceAcc_AnimBP.F_MED_Raider_Pink_FaceAcc_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/Parts/F_MED_Raider_Pink_FaceAcc.F_MED_Raider_Pink_FaceAcc",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Skins/Silver/Materials/F_MED_Raider_Silver_Face_Acc.F_MED_Raider_Silver_Face_Acc",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Skins/Silver/Materials/F_MED_Raider_Silver_Hair.F_MED_Raider_Silver_Hair",
                        Replace = SwapModel.FaceACCMaterials[1],
                        Type = SwapType.FaceAccessoryMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RaiderSilver",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_Ice_Queen_Head/Meshes/F_MED_IceQueen_Head_Child_AnimBP.F_MED_IceQueen_Head_Child_AnimBP_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_Ice_Queen_Head/Meshes/F_MED_IceQueen_Head_Child_AnimBP.F_MED_IceQueen_Head_Child_AnimBP_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_Ice_Queen_Head/Meshes/F_MED_Ice_Queen_Head.F_MED_Ice_Queen_Head",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_Ice_Queen_Head/Skins/Raider_Silver/Materials/F_MED_Raider_Silver_Head.F_MED_Raider_Silver_Head",
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
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/F_MED_Raider_Pink_AnimBP.F_MED_Raider_Pink_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/F_MED_Raider_Pink_AnimBP.F_MED_Raider_Pink_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Meshes/F_MED_Raider_Pink.F_MED_Raider_Pink",
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
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Raider_Pink/Skins/Silver/Materials/F_MED_Raider_Silver_Body.F_MED_Raider_Silver_Body",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    }
                }
            }
        };
}