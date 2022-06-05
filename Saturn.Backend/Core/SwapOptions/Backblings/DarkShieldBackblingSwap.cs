using Saturn.Backend.Core.Enums;
using Saturn.Backend.Core.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Core.SwapOptions.Backblings;

internal sealed class DarkShieldBackblingSwap : BackblingSwap
{
    public DarkShieldBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_CubeRedKnight",
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
                            "/Game/Accessories/FORT_Backpacks/Textures/Male_Commando_BR_Knight_01/RedKnight_Cube/Materials/F_MED_RedKnightCube_Backpack.F_MED_RedKnightCube_Backpack",
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