using System.Collections.Generic;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;

namespace Saturn.Backend.Data.SwapOptions.Skins;

internal sealed class RustLordSkinSwap : SkinSwap
{
    public RustLordSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_M_Scavenger",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Male/Medium/Bodies/M_MED_RustyRaider_01/Meshes/M_MED_RustyRaider_01_Body_Skeleton_AnimBP.M_MED_RustyRaider_01_Body_Skeleton_AnimBP_C",
                        Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Male/Medium/Bodies/M_MED_RustyRaider_01/Meshes/M_MED_RustyRaider_01_Body_Skeleton_AnimBP.M_MED_RustyRaider_01_Body_Skeleton_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Male/Medium/Bodies/M_MED_RustyRaider_01/Meshes/SK_M_MED_RustyRaider_01_Body.SK_M_MED_RustyRaider_01_Body",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Male/Medium/Base/SK_M_MALE_Base_Skeleton.SK_M_MALE_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Male/Medium/Heads/M_Med_Soldier_Head_01",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Skin/M_Med_CAU.M_Med_CAU",
                        Replace = SwapModel.HeadSkinColor,
                        Type = SwapType.SkinTone
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Male/Medium/Heads/M_MED_CAU_Jonesy_Head_01/Meshes/M_MED_CAU_Jonesy_Head_01_Export_Skeleton_AnimBP_Child.M_MED_CAU_Jonesy_Head_01_Export_Skeleton_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Male/Medium/Heads/M_MED_CAU_Jonesy_Head_01/Meshes/M_MED_CAU_Jonesy_Head_01_Export_Skeleton_AnimBP_Child.M_MED_CAU_Jonesy_Head_01_Export_Skeleton_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Male/Medium/Heads/M_MED_CAU_Jonesy_Head_01/Meshes/M_MED_CAU_Jonesy_Head_01.M_MED_CAU_Jonesy_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/CP_Hat_M_Commando_Scavenger",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Accessories/Hats/Mesh/Male_Commando_19.Male_Commando",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 4, 4, 3, 2, 3 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 4, 4, 3, (byte)SwapModel.HatType, 3 }),
                        Type = SwapType.Property
                    }
                }
            }
        };
}