using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Backblings;

internal sealed class RedShieldBackblingSwap : BackblingSwap
{
    public RedShieldBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/Male_Commando_BlackKnight_V01",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Mesh/SK_Shield_Blackknight.SK_Shield_Blackknight",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Materials/Male_Commando_BR_BlackknightV01.Male_Commando_BR_BlackknightV01",
                        Replace = Data["Material"],
                        Type = SwapType.BackblingMaterial
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/M_MED_Wegame_Backpack/Meshes/SK_Backpack_Wegame_Skeleton_AnimBlueprint.SK_Backpack_Wegame_Skeleton_AnimBlueprint_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/M_MED_Wegame_Backpack/Meshes/SK_Backpack_Wegame_Skeleton_AnimBlueprint.SK_Backpack_Wegame_Skeleton_AnimBlueprint_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}