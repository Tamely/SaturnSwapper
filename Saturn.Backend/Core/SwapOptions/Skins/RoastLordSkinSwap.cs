using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Skins;

internal sealed class RoastLordSkinSwap : SkinSwap
{
    public RoastLordSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel)
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_M_ScavengerFire",
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
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Athena/Cosmetics/Blueprints/Part_Modifiers/B_Athena_PartModifier_Scavenger_Fire.B_Athena_PartModifier_Scavenger_Fire_C",
                        Replace = SwapModel.BodyPartModifierBP,
                        Type = SwapType.Modifier
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Male/Medium/Bodies/M_MED_RustyRaider_01/Skins/Fire/Materials/M_MED_ScavengerFire_Body.M_MED_ScavengerFire_Body",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Effects/Fort_Effects/Effects/Characters/Athena_Parts/Scavenger/NS_Scavenger_Fire.NS_Scavenger_Fire",
                        Replace = SwapModel.BodyFX,
                        Type = SwapType.BodyFx
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Male/Medium/Heads/CP_Athena_Head_M_ScavengerFire",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_02.HairColor_02",
                        Replace = SwapModel.HeadHairColor,
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
                        Search = "/Game/Characters/Player/Male/Medium/Heads/M_MED_CAU_Jonsey_Rebirth_Head_01/Meshes/M_MED_CAU_Jonesy_Rebirth_Head_01.M_MED_CAU_Jonesy_Rebirth_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Male/Medium/Bodies/M_MED_RustyRaider_01/Skins/Fire/Materials/M_MED_ScavengerFire_Head.M_MED_ScavengerFire_Head",
                        Replace = SwapModel.HeadMaterials[0],
                        Type = SwapType.HeadMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/FaceAccessories/CP_M_MED_ScavengerFire",
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
                        Search = "/Game/Characters/Player/Male/Medium/Bodies/M_MED_RustyRaider_01/Skins/Fire/Materials/M_MED_ScavengerFire_FaceAcc.M_MED_ScavengerFire_FaceAcc",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 5, 2, 2, 0 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 5, (byte)SwapModel.HatType, 2, 0 }),
                        Type = SwapType.Property
                    }
                }
            }
        };
}