using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class RubyShadowsSkinSwap : SkinSwap
{
    public RubyShadowsSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_StreetFashionEclipse",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red.F_MED_Street_Fashion_Red",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_AnimBP.F_MED_Street_Fashion_Red_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/F_MED_Street_Fashion_Red_AnimBP.F_MED_Street_Fashion_Red_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Skins/Eclipse/Materials/F_MED_StreetFashionEclipse_Body.F_MED_StreetFashionEclipse_Body",
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
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_StreetFashionEclipse",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_Angel_Head_01/Meshes/F_MED_Angel_Head_AnimBP_Child.F_MED_Angel_Head_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_Angel_Head_01/Meshes/F_MED_Angel_Head_AnimBP_Child.F_MED_Angel_Head_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_Angel_Head_01/Meshes/F_MED_Angel_Head_01.F_MED_Angel_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Skins/Eclipse/Materials/F_MED_StreetFashionEclipse_Head.F_MED_StreetFashionEclipse_Head",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HeadMaterial
                    },
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_FaceAcc_StreetFashionEclipse",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/Parts/F_MED_Street_Fashion_Red_FaceAcc_AnimBp.F_MED_Street_Fashion_Red_FaceAcc_AnimBp_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/Parts/F_MED_Street_Fashion_Red_FaceAcc_AnimBp.F_MED_Street_Fashion_Red_FaceAcc_AnimBp_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Meshes/Parts/F_MED_Street_Fashion_Red_FaceAcc.F_MED_Street_Fashion_Red_FaceAcc",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_Street_Fashion_Red/Skins/Eclipse/Materials/F_MED_StreetFashionEclipse_Hair.F_MED_StreetFashionEclipse_Hair",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    }
                }
            }
        };
}