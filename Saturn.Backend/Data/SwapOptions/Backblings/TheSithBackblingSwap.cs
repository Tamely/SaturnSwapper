using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Models.Items;
using System.Collections.Generic;

namespace Saturn.Backend.Data.SwapOptions.Backblings;

internal sealed class TheSithBackblingSwap : BackblingSwap
{
    public TheSithBackblingSwap(string name, string rarity, string icon, Dictionary<string, string> data)
        : base(name, rarity, icon, data)
    {
    }

    public override List<SaturnAsset> Assets =>
        new()
        {
            new SaturnAsset()
            {
                ParentAsset = "FortniteGame/Content/Characters/CharacterParts/Backpacks/CP_Backpack_GalileoSpeedBoat",
                Swaps = new List<SaturnSwap>()
                {
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/M_MED_Celestial_Backpack/M_MED_Celestial.M_MED_Celestial",
                        Replace = Data["Mesh"],
                        Type = SwapType.BackblingMesh
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Backpack_Galileo_Holos/FX/P_Backpack_GalileoSpeedboat_Holo.P_Backpack_GalileoSpeedboat_Holo",
                        Replace = Data["FX"],
                        Type = SwapType.BackblingFx
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Athena/Cosmetics/Blueprints/B_Athena_PartModifier_Generic.B_Athena_PartModifier_Generic_C",
                        Replace = Data["PartModifierBP"],
                        Type = SwapType.BackblingPartBP
                    },
                    new SaturnSwap()
                    {
                        Search =
                            "/Game/Accessories/FORT_Backpacks/Mesh/Male_Commando_Graffiti_Skeleton_AnimBP.Male_Commando_Graffiti_Skeleton_AnimBP_C",
                        Replace = Data["ABP"] ?? "/Game/Accessories/FORT_Backpacks/Mesh/Male_Commando_Graffiti_Skeleton_AnimBP.Male_Commando_Graffiti_Skeleton_AnimBP_C",
                        Type = SwapType.BackblingAnim
                    }
                }
            }
        };
}
