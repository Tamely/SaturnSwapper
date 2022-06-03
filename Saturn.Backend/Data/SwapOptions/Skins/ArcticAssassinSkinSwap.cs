using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class ArcticAssassinSkinSwap : SkinSwap
{
    public ArcticAssassinSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset =
                    "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_Wintact_F.CP_Body_Commande_F_Wintact",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Replace = SwapModel.BodyABP ??
                                  "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/BR_06/Materials/F_MED_Commando_Body_BR06.F_MED_Commando_Body_BR06",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    },
                }
            },
            new SaturnAsset()
            {
                ParentAsset =
                     "FortniteGame/Athena/Heroes/Meshes/Heads/F_Med_Head_2_ATH",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ??
                                  "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01.F_MED_HIS_Ramirez_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/Player/Female/Medium/Heads/F_Med_Head_01/Materials/BR06/F_MED_HIS_Ramirez_BR06.F_MED_HIS_Ramirez_BR06",
                        Replace = SwapModel.HeadMaterials[1],
                        Type = SwapType.HeadMaterial
                    },
                    new()
                    {
                        Search =
                            "/Game/Characters/CharacterColorSwatches/Skin/M_Med_HIS.M_Med_HIS",
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
                    "FortniteGame/Characters/CharacterParts/Hats/Glasses_Commando_BR06_F.Glasses_Commando_BR06_F",
                Swaps = new()
                {
                    new()
                    {
                        Search =
                            "/Game/Accessories/Glasses/Meshes/Glasses_Commando_BR06_F.Glasses_Commando_BR06_F",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    }
                }
            }
        };
}
