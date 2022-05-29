using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class AuraSwap : SkinSwap
{
    public AuraSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_TreasureHunterFashion",
                Swaps = new List<SaturnSwap>()
                {
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
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Skins/TreasureHunterF/Materials/F_MED_THFashion_Head.F_MED_THFashion_Head",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HeadMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Skins/TreasureHunterF/Materials/F_MED_THFashion_Hair.F_MED_THFashion_Hair",
                        Replace = SwapModel.HeadMaterials[1],
                        Type = SwapType.HairMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_TreasureHunterFashion",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Treasure_Hunter_Fashion/Meshes/F_MED_Treasure_Hunter_Fashion_AnimBp.F_MED_Treasure_Hunter_Fashion_AnimBp_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Treasure_Hunter_Fashion/Meshes/F_MED_Treasure_Hunter_Fashion_AnimBp.F_MED_Treasure_Hunter_Fashion_AnimBp_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Treasure_Hunter_Fashion/Meshes/F_MED_Treasure_Hunter_Fashion.F_MED_Treasure_Hunter_Fashion",
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
                ParentAsset = "FortniteGame/Characters/CharacterParts/FaceAccessories/CP_F_MED_TreasureHunterFashion",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Treasure_Hunter_Fashion/Meshes/Parts/F_MED_TreasureHunterFash_FaceAcc_AnimBp.F_MED_TreasureHunterFash_FaceAcc_AnimBp_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Treasure_Hunter_Fashion/Meshes/Parts/F_MED_TreasureHunterFash_FaceAcc_AnimBp.F_MED_TreasureHunterFash_FaceAcc_AnimBp_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Treasure_Hunter_Fashion/Meshes/Parts/F_MED_Treasure_Hunter_Fashion_FaceAcc.F_MED_Treasure_Hunter_Fashion_FaceAcc",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    }
                }
            }
        };
}