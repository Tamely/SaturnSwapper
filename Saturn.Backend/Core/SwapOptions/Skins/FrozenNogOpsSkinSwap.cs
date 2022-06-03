using System.Collections.Generic;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;

namespace Saturn.Backend.Core.SwapOptions.Skins;

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
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_UglySweaterFrozen",
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
                            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/UglySweater_Frozen/Materials/F_M_UglySweater_Frozen_Head.F_M_UglySweater_Frozen_Head",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HeadMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Sarah_Head_01/Materials/F_MED_ASN_Sarah_Hair_Hide.F_MED_ASN_Sarah_Hair_Hide",
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
                            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/UglySweater_Frozen/Materials/F_M_UglySweater_Frozen_Body.F_M_UglySweater_Frozen_Body",
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
                        Search =
                            "/Game/Accessories/Hats/F_MED_HolidayPJs_FaceAcc/Meshes/F_MED_Holiday_PJs_1_FaceAcc_AnimBP.F_MED_Holiday_PJs_1_FaceAcc_AnimBP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Accessories/Hats/F_MED_HolidayPJs_FaceAcc/Meshes/F_MED_Holiday_PJs_1_FaceAcc_AnimBP.F_MED_Holiday_PJs_1_FaceAcc_AnimBP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/Hats/F_MED_HolidayPJs_FaceAcc/Meshes/F_MED_Holiday_PJs_1_FaceAcc.F_MED_Holiday_PJs_1_FaceAcc",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/Hats/F_MED_HolidayPJs_FaceAcc/Skins/UglySweater_Frozen/Materials/MI_F_MED_UglySweater_Frozen_FaceAcc.MI_F_MED_UglySweater_Frozen_FaceAcc",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/UglySweater_Frozen/Materials/F_M_UglySweater_Frozen_Hair.F_M_UglySweater_Frozen_Hair",
                        Replace = SwapModel.FaceACCMaterials[1],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 4, 4, 3, 2, 4 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 4, 4, 3, (byte)SwapModel.HatType, 4 }),
                        Type = SwapType.Property
                    }
                }
            }
        };
}