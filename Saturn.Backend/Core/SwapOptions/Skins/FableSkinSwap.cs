using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Skins;

internal sealed class FableSkinSwap : SkinSwap
{
    public FableSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_RedRiding",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_RedRiding/Mesh/F_MED_RedRiding.F_MED_RedRiding_RedRidingHood_Proto_MESH",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_RedRiding/Mesh/F_MED_RedRiding_RedRidingHood_Proto_MESH_Skeleton_ABP.F_MED_RedRiding_RedRidingHood_Proto_MESH_Skeleton_ABP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_RedRiding/Mesh/F_MED_RedRiding_RedRidingHood_Proto_MESH_Skeleton_ABP.F_MED_RedRiding_RedRidingHood_Proto_MESH_Skeleton_ABP_C",
                        Type = SwapType.BodyAnim
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
                    "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RedRiding",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Starfish_Head_01/Meshes/F_MED_ASN_Starfish_Head_01_AnimBP_Child.F_MED_ASN_Starfish_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Starfish_Head_01/Meshes/F_MED_ASN_Starfish_Head_01_AnimBP_Child.F_MED_ASN_Starfish_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_ASN_Starfish_Head_01/Meshes/F_MED_ASN_Starfish_Head_01.F_MED_ASN_Starfish_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_RedRiding/Materials/F_MED_ASN_Starfish_Head_01_RedRiding.F_MED_ASN_Starfish_Head_01_RedRiding",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HairMaterial
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_RedRiding/Materials/MI_F_MED_RedRiding_Head.MI_F_MED_RedRiding_Head",
                        Replace = SwapModel.HeadMaterials[1],
                        Type = SwapType.HeadMaterial
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/CharacterColorSwatches/Skin/F_ASN_Starfish.F_ASN_Starfish",
                        Replace = SwapModel.HeadSkinColor,
                        Type = SwapType.SkinTone
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                        Replace = SwapModel.HeadHairColor,
                        Type = SwapType.HairColor
                    },
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_F_MED_RedRiding",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_RedRiding/Mesh/Female_Medium_Starfish_Head_01_lashe_Skeleton_ABP.Female_Medium_Starfish_Head_01_lashe_Skeleton_ABP_C",
                        Replace = SwapModel.FaceACCABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_MED_RedRiding/Mesh/Female_Medium_Starfish_Head_01_lashe_Skeleton_ABP.Female_Medium_Starfish_Head_01_lashe_Skeleton_ABP_C",
                        Type = SwapType.FaceAccessoryAnim
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_MED_RedRiding/Mesh/Female_Medium_Starfish_Head_01_Eyelashes_Export.Female_Medium_Starfish_Head_01_Eyelashes_Export_EYELASHES_MESH",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    }
                }
            }
        };
}