using System;
using System.Collections.Generic;
using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;

namespace Saturn.Backend.Core.SwapOptions.Skins;

internal sealed class FrozenRedKnightSkinSwap : SkinSwap
{
    public FrozenRedKnightSkinSwap(string name, string rarity, string icon, MeshDefaultModel swapModel) 
        : base(name, rarity, icon, swapModel)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Athena/Heroes/Meshes/Bodies/CP_Body_Commando_F_RedKnightWinter",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Replace = SwapModel.BodyABP ?? "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01_Skeleton_AnimBP.F_Med_Soldier_01_Skeleton_AnimBP_C",
                        Type = SwapType.BodyAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Meshes/F_Med_Soldier_01.F_Med_Soldier_01",
                        Replace = SwapModel.BodyMesh,
                        Type = SwapType.BodyMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Base/SK_M_Female_Base_Skeleton.SK_M_Female_Base_Skeleton",
                        Replace = SwapModel.BodySkeleton,
                        Type = SwapType.BodySkeleton
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Bodies/F_Med_Soldier_01/Skins/TV_32_RedKnight_Winter/Materials/M_F_MED_Commando_RedKnight_Winter.M_F_MED_Commando_RedKnight_Winter",
                        Replace = SwapModel.BodyMaterials[0],
                        Type = SwapType.BodyMaterial
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Female/Medium/Heads/CP_Head_F_RedKnightWinter",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Skin/F_Med_HIS.F_Med_HIS",
                        Replace = SwapModel.HeadSkinColor,
                        Type = SwapType.SkinTone
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/CharacterColorSwatches/Hair/HairColor_01.HairColor_01",
                        Replace = SwapModel.HeadHairColor,
                        Type = SwapType.HairColor
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Replace = SwapModel.HeadABP ?? "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01_AnimBP_Child.F_MED_HIS_Ramirez_Head_01_AnimBP_Child_C",
                        Type = SwapType.HeadAnim
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_MED_HIS_Ramirez_Head_01/Mesh/F_MED_HIS_Ramirez_Head_01.F_MED_HIS_Ramirez_Head_01",
                        Replace = SwapModel.HeadMesh,
                        Type = SwapType.HeadMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Characters/Player/Female/Medium/Heads/F_Med_Head_01/Materials/Chainmail/F_MED_Commando_RedKnight_Winter.F_MED_Commando_RedKnight_Winter",
                        Replace = SwapModel.HeadMaterials[1],
                        Type = SwapType.HeadMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = Convert.ToBase64String(new byte[] {47,71,97,109,101,47,67,104,97,114,97,99,116,101,114,115,47,80,108,97,121,101,114,47,70,101,109,97,108,101,47,77,101,100,105,117,109,47,72,101,97,100,115,47,70,95,77,101,100,95,72,101,97,100,95,48,49,47,77,97,116,101,114,105,97,108,115,47,70,95,77,69,68,95,67,111,109,109,97,110,100,111,95,78,111,95,72,97,105,114,46,70,95,77,69,68,95,67,111,109,109,97,110,100,111,95,78,111,95,72,97,105,114}),
                        Replace = SwapModel.HeadMaterials[0] ?? "/",
                        Type = SwapType.Base64
                    }
                }
            },
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Hats/Hat_F_Commando_RedKnightWinter",
                Swaps = new()
                {
                    new SaturnSwap()
                    {
                        Search = "/Game/Accessories/Hats/Mesh/Female_Commando_BR_BlackKnight_01.Female_Commando_BR_BlackKnight_01",
                        Replace = SwapModel.FaceACCMesh,
                        Type = SwapType.FaceAccessoryMesh
                    },
                    new SaturnSwap()
                    {
                        Search = "/Game/Accessories/Hats/Materials/Hat_Commando_RedKnight_Winter.Hat_Commando_RedKnight_Winter",
                        Replace = SwapModel.FaceACCMaterials[0],
                        Type = SwapType.FaceAccessoryMaterial
                    },
                    new SaturnSwap()
                    {
                        Search = System.Convert.ToBase64String(new byte[] { 0, 5, 2, 2, 0 }),
                        Replace = System.Convert.ToBase64String(new byte[] { 0, 5, (byte)SwapModel.HatType, 2, 0 }),
                        Type = SwapType.Property
                    }
                }
            }
        };
}